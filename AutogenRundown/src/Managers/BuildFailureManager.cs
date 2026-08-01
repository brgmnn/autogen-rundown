using AutogenRundown.Events;
using AutogenRundown.Serialization;
using GameData;
using GTFO.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Managers;

/// <summary>
/// Records levels that could not be generated and locks them out of the rundown.
///
/// When <see cref="FactoryJobManager"/> exhausts its rebuild budget the host aborts the drop,
/// returns everyone to the lobby, and broadcasts the failure. Every peer then persists the
/// failure and marks the expedition as <c>BlockedAndScrambled</c> so it can never be selected
/// again for that Autogen version + rundown seed.
///
/// Storage lives under %AppData%/GTFO-Modding/AutogenRundown/BuildFailures rather than the
/// generated GameData tree, because RundownFactory.CleanFolders() wipes the latter on every
/// launch.
/// </summary>
public static class BuildFailureManager
{
    private const string eventName = "autogen_level_build_failed";

    private static readonly string dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GTFO-Modding",
        "AutogenRundown",
        "BuildFailures");

    /// <summary>
    /// Records by rundown name, e.g. "RndRundownSeed20260725".
    /// </summary>
    private static readonly Dictionary<string, RundownBuildFailureRecord> records = new();

    /// <summary>
    /// Rundown persistent id -> rundown name. Needed to resolve which record to write to when
    /// a failure arrives mid-expedition.
    /// </summary>
    private static readonly Dictionary<uint, string> rundownNames = new();

    /// <summary>
    /// Main layout ids that are currently locked out. This is the fast path used by the rundown
    /// page when re-scrambling already placed icons.
    /// </summary>
    private static readonly HashSet<uint> lockedLayouts = new();

    /// <summary>
    /// Set once the host has decided to give up. Populated on every peer by the abort broadcast
    /// so client rebuild loops stop too.
    /// </summary>
    private static BuildFailedEvent? pendingAbort;

    /// <summary>
    /// Set when the host decides to abort. Kept separate from <see cref="pendingAbort"/> so the
    /// drop still ends even if we could not work out which level to lock -- otherwise the frozen
    /// factory would hang everyone on the drop screen, which is the bug we are fixing.
    /// </summary>
    private static bool abortRequested;

    private static bool abortIssued;

    private static bool lockBroadcast;

    private static int abortRetries;

    private static PopupData? pendingPopup;

    /// <summary>
    /// Clock.Time at which the lobby was first seen settled. The popup waits a moment so the
    /// loadout page's own booster / vanity popups get to go first.
    /// </summary>
    private static float settledAt = -1f;

    private const float SettleDelay = 2f;

    private readonly record struct PopupData(string Tier, int Index, string Name, int Rebuilds);

    #region Setup

    public static void Setup()
    {
        try
        {
            LoadAll();
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"[BuildFailure] Failed to load records: {error.Message}");
        }

        try
        {
            NetworkAPI.RegisterEvent<BuildFailedEvent>(eventName, OnBuildAborted);
        }
        catch (Exception error)
        {
            // Degrades gracefully: the host still ends the session for everyone, clients just
            // miss the lock and the popup.
            Plugin.Logger.LogError($"[BuildFailure] Could not register network event: {error.Message}");
        }
    }

    /// <summary>
    /// Loads every stored record, discards any that were written by a different Autogen version,
    /// prunes files for rundowns that no longer exist, and applies the locks to the loaded
    /// rundown data blocks.
    /// </summary>
    private static void LoadAll()
    {
        var blocks = RundownDataBlock.GetAllBlocks();
        var liveNames = new HashSet<string>();

        foreach (var rundown in blocks)
        {
            rundownNames[rundown.persistentID] = rundown.name;
            liveNames.Add(rundown.name);

            var record = Load(rundown.name);

            if (record == null)
                continue;

            records[rundown.name] = record;

            ApplyLocks(rundown, record);
        }

        Prune(liveNames);
    }

    /// <summary>
    /// Marks every stored failure for this rundown as BlockedAndScrambled on the live data
    /// block. Entries whose main layout id no longer matches are dropped -- the generator
    /// produced a different level for that slot, so the old failure no longer applies.
    /// </summary>
    private static void ApplyLocks(RundownDataBlock rundown, RundownBuildFailureRecord record)
    {
        var stale = new List<BuildFailureRecord>();

        foreach (var entry in record.Levels)
        {
            var tier = GetTier(rundown, entry.Tier);

            if (tier == null || entry.Index < 0 || entry.Index >= tier.Count)
            {
                stale.Add(entry);
                continue;
            }

            var expedition = tier[entry.Index];

            if (expedition.LevelLayoutData != entry.MainLevelLayout)
            {
                Plugin.Logger.LogWarning(
                    $"[BuildFailure] Dropping stale lock {entry.Tier}{entry.Index + 1} in " +
                    $"{rundown.name}: layout {entry.MainLevelLayout} != {expedition.LevelLayoutData}");
                stale.Add(entry);
                continue;
            }

            expedition.Accessibility = eExpeditionAccessibility.BlockedAndScrambled;
            lockedLayouts.Add(expedition.LevelLayoutData);

            Plugin.Logger.LogInfo(
                $"[BuildFailure] Locked {entry.Tier}{entry.Index + 1} in {rundown.name} " +
                $"(layout {entry.MainLevelLayout}, failed after {entry.Rebuilds} rebuilds)");
        }

        if (stale.Count < 1)
            return;

        foreach (var entry in stale)
            record.Levels.Remove(entry);

        Save(record);
    }

    /// <summary>
    /// Deletes records for rundowns that are no longer loaded. This is what expires yesterday's
    /// daily once its seed rolls over.
    /// </summary>
    private static void Prune(HashSet<string> liveNames)
    {
        if (!Directory.Exists(dir))
            return;

        foreach (var path in Directory.GetFiles(dir, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(path);

            if (liveNames.Contains(name))
                continue;

            try
            {
                File.Delete(path);
                Plugin.Logger.LogDebug($"[BuildFailure] Pruned stale record: {name}");
            }
            catch (Exception error)
            {
                Plugin.Logger.LogWarning($"[BuildFailure] Could not prune {name}: {error.Message}");
            }
        }
    }

    #endregion

    #region Queries

    /// <summary>
    /// True if this level has been permanently locked out.
    /// </summary>
    public static bool IsLocked(uint mainLevelLayout) => lockedLayouts.Contains(mainLevelLayout);

    #endregion

    #region Abort

    /// <summary>
    /// Clears the staged abort so a later drop in the same session can abort too. Called from
    /// FactoryJobManager.NewBuild(). The pending popup is deliberately left alone -- it is shown
    /// after we are back in the lobby, which is after this runs for the next expedition.
    /// </summary>
    public static void OnNewBuild()
    {
        pendingAbort = null;
        abortRequested = false;
        abortIssued = false;
        lockBroadcast = false;
        abortRetries = 0;
    }

    /// <summary>
    /// Called by FactoryJobManager on the host when the rebuild budget is exhausted. Only stages
    /// the abort -- we are deep inside LG_Factory.Update here, so the actual session command is
    /// issued from TickAbort() on the next frame.
    /// </summary>
    public static void OnHostGaveUp(int rebuilds)
    {
        if (abortRequested)
            return;

        // Set first and unconditionally: whatever happens below, the drop must end
        abortRequested = true;

        try
        {
            var expedition = RundownManager.ActiveExpedition;
            var active = RundownManager.GetActiveExpeditionData();

            if (expedition == null)
            {
                Plugin.Logger.LogError(
                    "[BuildFailure] No active expedition, aborting without locking the level");

                return;
            }

            pendingAbort = new BuildFailedEvent
            {
                RundownId = ActiveRundownId(),
                Tier = (int)active.tier,
                Index = active.expeditionIndex,
                MainLevelLayout = expedition.LevelLayoutData,
                Rebuilds = rebuilds
            };
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError(
                $"[BuildFailure] Could not identify the failed level, aborting anyway: {error.Message}");
        }
    }

    /// <summary>
    /// Called every frame from Patch_LG_Factory once the factory has been frozen. Issues the
    /// broadcast and the session command exactly once, from outside the factory job loop.
    /// </summary>
    public static void TickAbort()
    {
        // Clients reach here via the host's broadcast (which sets GaveUp). Only the host ends
        // the session.
        if (abortIssued || !abortRequested || !SNet.IsMaster)
            return;

        // Broadcast and lock exactly once, even if the session command below has to be retried
        if (!lockBroadcast)
        {
            lockBroadcast = true;

            if (pendingAbort != null)
            {
                var data = pendingAbort.Value;

                try
                {
                    NetworkAPI.InvokeEvent(eventName, data);
                }
                catch (Exception error)
                {
                    Plugin.Logger.LogError($"[BuildFailure] Failed to broadcast abort: {error.Message}");
                }

                // Apply locally too -- InvokeEvent does not loop back to the sender
                OnBuildAborted(0uL, data);

                Plugin.Logger.LogError(
                    $"[BuildFailure] Aborting expedition after {data.Rebuilds} rebuilds " +
                    $"(layout {data.MainLevelLayout})");
            }
        }

        // Retry until accepted. The factory is frozen at this point, so a command that is
        // rejected once and never retried would hang everyone on the drop screen -- exactly the
        // failure this feature exists to prevent. GS_AfterLevel resends EndPlaying for the same
        // reason.
        if (SNet.Sync.SessionCommand(eSessionCommandType.TryEndPlaying))
        {
            abortIssued = true;

            return;
        }

        if (abortRetries++ % 300 == 0)
            Plugin.Logger.LogError(
                $"[BuildFailure] SessionCommand(TryEndPlaying) rejected, retrying (attempt {abortRetries})");
    }

    /// <summary>
    /// Runs on every peer. Freezes the local factory, persists the failure and locks the level.
    /// </summary>
    private static void OnBuildAborted(ulong sender, BuildFailedEvent data)
    {
        // Stops this peer's rebuild loop even if it never hit its own budget
        FactoryJobManager.MarkGaveUp();

        var rundown = RundownDataBlock.GetBlock(data.RundownId);

        if (rundown == null)
        {
            Plugin.Logger.LogWarning($"[BuildFailure] Unknown rundown {data.RundownId}, not locking");
            return;
        }

        var tierLetter = TierLetter(data.Tier);
        var tier = GetTier(rundown, tierLetter);

        if (tier == null || data.Index < 0 || data.Index >= tier.Count)
        {
            Plugin.Logger.LogWarning(
                $"[BuildFailure] {tierLetter}{data.Index + 1} out of range in {rundown.name}, not locking");
            return;
        }

        var expedition = tier[data.Index];

        // Integrity check: if this peer generated a different level for the same slot then its
        // Autogen output differs from the host's, and locking would blacklist the wrong level.
        if (expedition.LevelLayoutData != data.MainLevelLayout)
        {
            Plugin.Logger.LogWarning(
                $"[BuildFailure] Layout mismatch for {tierLetter}{data.Index + 1} in {rundown.name} " +
                $"({expedition.LevelLayoutData} local vs {data.MainLevelLayout} from host). " +
                "Generator output differs between peers, not locking.");
            return;
        }

        expedition.Accessibility = eExpeditionAccessibility.BlockedAndScrambled;
        lockedLayouts.Add(expedition.LevelLayoutData);

        Record(rundown.name, tierLetter, data);

        pendingPopup = new PopupData(
            tierLetter,
            data.Index + 1,
            expedition.Descriptive?.PublicName ?? "UNKNOWN",
            data.Rebuilds);
    }

    /// <summary>
    /// Adds the failure to this rundown's record and writes it to disk.
    /// </summary>
    private static void Record(string rundownName, string tierLetter, BuildFailedEvent data)
    {
        if (!records.TryGetValue(rundownName, out var record))
        {
            record = new RundownBuildFailureRecord
            {
                Name = rundownName,
                PluginVersion = Plugin.Version
            };
            records[rundownName] = record;
        }

        var existing = record.Levels.Find(l => l.Tier == tierLetter && l.Index == data.Index);

        if (existing != null)
        {
            existing.MainLevelLayout = data.MainLevelLayout;
            existing.Rebuilds = data.Rebuilds;
        }
        else
        {
            record.Levels.Add(new BuildFailureRecord
            {
                Tier = tierLetter,
                Index = data.Index,
                MainLevelLayout = data.MainLevelLayout,
                Rebuilds = data.Rebuilds,
                FirstFailedUtc = DateTime.UtcNow.ToString("o")
            });
        }

        Save(record);
    }

    #endregion

    #region Popup

    /// <summary>
    /// Shows the popup once we are settled back in the lobby. Driven from a postfix on
    /// CM_PageLoadout.Update (see Patch_CM_PageLoadout) -- deliberately NOT from an injected
    /// MonoBehaviour, which crashed the game with an AccessViolationException in the
    /// il2cpp -> managed Update bridge.
    ///
    /// We wait rather than firing on a game state transition because ShowPopup internally does
    /// FocusStateManager.ChangeState(GlobalPopupMessage) and remembers the state it replaced.
    /// Firing while GS_AfterLevel / GS_Lobby are still settling focus would restore the wrong
    /// state when the player dismisses it. The delay also lets the loadout page's own booster and
    /// vanity popups run first, so ours does not queue behind them.
    /// </summary>
    public static void TryShowPopup()
    {
        if (pendingPopup == null)
            return;

        if (GameStateManager.CurrentStateName != eGameStateName.Lobby &&
            GameStateManager.CurrentStateName != eGameStateName.NoLobby)
        {
            settledAt = -1f;

            return;
        }

        // Any open popup forces eFocusState.GlobalPopupMessage, so this also means no other popup
        // is currently up
        if (FocusStateManager.CurrentState != eFocusState.MainMenu)
        {
            settledAt = -1f;

            return;
        }

        if (settledAt < 0f)
        {
            settledAt = Clock.Time;

            return;
        }

        if (Clock.Time - settledAt < SettleDelay)
            return;

        var popup = pendingPopup.Value;
        pendingPopup = null;
        settledAt = -1f;

        try
        {
            Plugin.Logger.LogInfo(
                $"[BuildFailure] Showing unreachable popup for {popup.Tier}{popup.Index}");

            var panel = GlobalPopupMessageManager.ShowPopup(new PopupMessage
            {
                Header = "EXPEDITION UNREACHABLE",
                UpperText =
                    $"<color=orange>{popup.Tier}{popup.Index} :: \"{popup.Name}\"</color>\n\n" +
                    $"The Complex could not resolve a stable layout after {popup.Rebuilds} attempts.\n" +
                    "Expedition data is corrupted and has been locked out.",

                // Never left null -- CM_GlobalPopup.ShowMessage assigns it straight into a
                // TextMeshPro
                LowerText = "",

                // CM_GlobalPopupBase: bare header / body / close button, no decoration
                PopupType = PopupType.Confirmation,

                // Skips the m_contentHolder child walk entirely, so nothing is force-activated and
                // no blink / sound coroutines are left pending on a panel the player can close
                BlinkInContent = false
            });

            Plugin.Logger.LogInfo("[BuildFailure] Popup shown");

            // Before Resize, so the dump shows the prefab's natural structure rather than one our
            // own SetSize has already mutated
            DumpHierarchy(panel);

            Resize(panel);
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"[BuildFailure] Failed to show popup: {error.Message}");
        }
    }

    /// <summary>
    /// Logs the popup's full transform hierarchy.
    ///
    /// The CM_GlobalPopupBase prefab is loaded from the game's asset bundles via Resources.Load, so
    /// its structure cannot be read from the decompile. SetSize on the root resizes the outer rect
    /// but the background frame and the text sit on children that don't follow it, and the fields
    /// exposed on CM_GlobalPopup are not enough to find them. This dump is how we locate the right
    /// children so the panel can be resized without touching the font sizes.
    /// </summary>
    private static void DumpHierarchy(CM_GlobalPopup? panel)
    {
        if (panel == null)
            return;

        const int maxDepth = 6;
        const int maxNodes = 200;

        try
        {
            Plugin.Logger.LogInfo(
                $"[BuildFailure] === popup hierarchy === screen {Screen.width}x{Screen.height}");

            var nodes = 0;

            DumpNode(panel.transform, 0, maxDepth, ref nodes, maxNodes);

            if (nodes >= maxNodes)
                Plugin.Logger.LogInfo($"[BuildFailure] ... node cap ({maxNodes}) reached, truncated");

            Plugin.Logger.LogInfo("[BuildFailure] === end popup hierarchy ===");
        }
        catch (Exception error)
        {
            Plugin.Logger.LogWarning($"[BuildFailure] Could not dump popup hierarchy: {error.Message}");
        }
    }

    private static void DumpNode(Transform node, int depth, int maxDepth, ref int nodes, int maxNodes)
    {
        if (node == null || depth > maxDepth || nodes >= maxNodes)
            return;

        nodes++;

        var indent = new string(' ', depth * 2);
        var line = $"[BuildFailure] {indent}{node.name} active={node.gameObject.activeSelf}";

        var rect = node.TryCast<RectTransform>();

        if (rect != null)
        {
            // rect.width/height is the RESOLVED size -- sizeDelta alone is misleading under stretch
            // anchoring, and the resolved size is what identifies the oversized background
            line +=
                $" resolved=({rect.rect.width:F0}x{rect.rect.height:F0})" +
                $" sizeDelta={rect.sizeDelta}" +
                $" anchors={rect.anchorMin}-{rect.anchorMax}" +
                $" pivot={rect.pivot}" +
                $" pos={rect.anchoredPosition}" +
                $" scale={node.localScale}";
        }

        var components = new List<string>();

        foreach (var component in node.GetComponents<Component>())
        {
            if (component == null)
                continue;

            components.Add(TypeName(component));

            var text = component.TryCast<TMPro.TMP_Text>();

            if (text != null)
            {
                var preview = text.text ?? "";
                preview = preview.Length > 30 ? preview.Substring(0, 30) : preview;
                line += $" text=\"{preview.Replace("\n", "\\n")}\"";
            }
        }

        Plugin.Logger.LogInfo($"{line} [{string.Join(", ", components)}]");

        for (var i = 0; i < node.childCount; i++)
            DumpNode(node.GetChild(i), depth + 1, maxDepth, ref nodes, maxNodes);
    }

    /// <summary>
    /// Il2CppInterop's GetComponents&lt;Component&gt; returns wrappers whose managed type is not
    /// always the most derived one, so prefer the il2cpp type name.
    /// </summary>
    private static string TypeName(Component component)
    {
        try
        {
            return component.GetIl2CppType().Name;
        }
        catch
        {
            return component.GetType().Name;
        }
    }

    /// <summary>
    /// Shrinks the popup panel. The stock CM_GlobalPopupBase prefab fills the screen.
    ///
    /// CM_GlobalPopup extends RectTransformComp, so SetSize writes the root's sizeDelta and the
    /// stretch-anchored children (ContentGroup, Background, the body text rects) follow. The frame
    /// itself is drawn by SpriteRenderers, and UI_SpriteResizer only copies the rect size into
    /// SpriteRenderer.size when its Resize() is called -- nothing re-runs it after the prefab is
    /// instantiated, so the resizers are re-run here or the frame keeps rendering at prefab size.
    ///
    /// UpperText is stretch-anchored, so its sizeDelta is an offset, not a size; it is placed via
    /// offsetMin/offsetMax instead, keeping the prefab's 20px side / 50px top insets and pulling
    /// the bottom inset in to fit the shorter panel. HeaderText is fixed-anchored and left-aligned
    /// and LowerText is always empty here, so both are left alone.
    ///
    /// The full-screen "Huge Background" dim underlay and m_underlayCollider are deliberately
    /// untouched -- their rects don't follow the root, so the click blocker and the modal dim
    /// behind the panel survive the resize.
    /// </summary>
    private static void Resize(CM_GlobalPopup? panel)
    {
        if (panel == null)
            return;

        // Tune here. Vanilla's CM_ExpeditionWindow is 420x515 for scale.
        const float width = 800f;
        const float height = 400f;

        try
        {
            panel.SetSize(new Vector2(width, height));

            var upperRect = panel.m_upperText != null ? panel.m_upperText.rectTransform : null;

            if (upperRect != null)
            {
                upperRect.offsetMin = new Vector2(20f, 60f);
                upperRect.offsetMax = new Vector2(-20f, -50f);
            }

            foreach (var resizer in panel.GetComponentsInChildren<UI_SpriteResizer>())
                resizer.Resize();

            Plugin.Logger.LogDebug($"[BuildFailure] Popup resized to {panel.GetSize()}");
        }
        catch (Exception error)
        {
            // Styling must never take down the popup itself
            Plugin.Logger.LogWarning($"[BuildFailure] Could not resize popup: {error.Message}");
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Persistent id of the rundown the players are currently in.
    /// </summary>
    private static uint ActiveRundownId()
        => RundownManager.ActiveRundownKey switch
        {
            "Local_1" => (uint)PluginRundown.Daily,
            "Local_2" => (uint)PluginRundown.Weekly,
            "Local_3" => (uint)PluginRundown.Monthly,
            "Local_4" => (uint)PluginRundown.Seasonal,

            _ => 0u
        };

    private static string TierLetter(int tier)
        => tier switch
        {
            1 => "A",
            2 => "B",
            3 => "C",
            4 => "D",
            5 => "E",

            _ => "?"
        };

    private static Il2CppSystem.Collections.Generic.List<ExpeditionInTierData>? GetTier(
        RundownDataBlock rundown,
        string tier)
        => tier switch
        {
            "A" => rundown.TierA,
            "B" => rundown.TierB,
            "C" => rundown.TierC,
            "D" => rundown.TierD,
            "E" => rundown.TierE,

            _ => null
        };

    #endregion

    #region Filesystem

    private static string RecordFile(string rundownName)
    {
        var invalidChars = Path.GetInvalidPathChars();
        var filename = invalidChars.Aggregate(rundownName, (current, c) => current.Replace(c, '_'));

        return Path.Combine(dir, $"{filename}.json");
    }

    /// <summary>
    /// Loads a record, discarding (and deleting) it if it was written by a different Autogen
    /// version -- a generator change produces different layouts, so the failure no longer holds.
    /// </summary>
    private static RundownBuildFailureRecord? Load(string rundownName)
    {
        var path = RecordFile(rundownName);

        if (!File.Exists(path))
            return null;

        try
        {
            var data = JObject.Parse(File.ReadAllText(path));
            var record = data.ToObject<RundownBuildFailureRecord>();

            if (record == null)
                return null;

            if (record.PluginVersion != Plugin.Version)
            {
                Plugin.Logger.LogInfo(
                    $"[BuildFailure] Discarding {rundownName}: written by Autogen " +
                    $"{record.PluginVersion}, running {Plugin.Version}");
                File.Delete(path);

                return null;
            }

            return record;
        }
        catch (Exception error)
        {
            Plugin.Logger.LogWarning($"[BuildFailure] Could not read {rundownName}: {error.Message}");

            return null;
        }
    }

    private static void Save(RundownBuildFailureRecord record)
    {
        try
        {
            Directory.CreateDirectory(dir);

            var serializer = new JsonSerializer { Formatting = Formatting.Indented };

            using var stream = new StreamWriter(RecordFile(record.Name));
            using var writer = new JsonTextWriter(stream);

            serializer.Serialize(writer, record);
            stream.Flush();
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"[BuildFailure] Could not save {record.Name}: {error.Message}");
        }
    }

    #endregion
}
