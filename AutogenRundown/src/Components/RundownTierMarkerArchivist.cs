using AutogenRundown.Managers;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

namespace AutogenRundown.Components;

internal class RundownTierMarkerArchivist : MonoBehaviour
{
    internal CM_RundownTierMarker m_tierMarker;

    private CM_ExpeditionSectorIcon? m_completeWithNoBoosterIcon = null;
    private SpriteRenderer m_icon;
    private SpriteRenderer m_bg;
    private TextMeshPro? m_title;
    private TextMeshPro? m_rightSideText;

    private static int totalRead = 0;
    private static int totalLogs = 0;

    private ArchivistIconWrapper? Wrapper;

    private static byte[] spriteData;
    private static Texture2D texture;
    private SpriteRenderer m_sprite;

    private static GameObject? Icon { get; set; }

    private static List<RundownTierMarkerArchivist> instances = new();

    private Action? pendingAssetLoad;

    internal static void PluginSetup()
    {
        // A single subscriber drives every marker, rather than one subscription per instance.
        // CM_PageRundown_New re-places the whole rundown on every rundown switch, so markers
        // are torn down and rebuilt repeatedly; driving them from a list we can prune keeps a
        // marker whose icon has already been destroyed from throwing on every later update.
        EventManager.OnRundownUpdate += (rundown) =>
        {
            (totalRead, totalLogs) = LogArchivistManager.GetLogsRead(rundown);

            for (var i = instances.Count - 1; i >= 0; i--)
            {
                var marker = instances[i];

                // Unity's == is fake-null aware, so this catches destroyed components
                if (marker == null)
                {
                    instances.RemoveAt(i);
                    continue;
                }

                try
                {
                    marker.OnRundownUpdate(rundown);
                }
                catch (Exception error)
                {
                    instances.RemoveAt(i);

                    Plugin.Logger.LogWarning(
                        $"Archivist tier marker dropped, its icon is gone: {error.Message}");
                }
            }
        };
    }

    internal static void OnAssetBundlesLoaded()
    {
        Icon = AssetAPI.GetLoadedAsset<GameObject>("Assets/Misc/CM_ExpeditionSectorIcon.prefab");

        var dir = Path.Combine(Paths.PluginPath, Plugin.Name);
        var path = Path.Combine(dir, "dlock.png");

        if (!File.Exists(path))
        {
            Plugin.Logger.LogError($"File not found: {path}");
            return;
        }

        spriteData = File.ReadAllBytes(path);

        // Decode into RGBA32 texture (works fine for JPG)
        texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!ImageConversion.LoadImage(texture, spriteData, markNonReadable: false))
            Plugin.Logger.LogError("Failed to decode image.");
    }

    internal void Setup()
    {
        if (m_tierMarker == null)
        {
            Plugin.Logger.LogError("Assign the page instance before setup");
            return;
        }

        if (m_completeWithNoBoosterIcon != null)
        {
            Plugin.Logger.LogError($"longicon is not null");
            return;
        }

        if (Icon == null)
        {
            // Held in a field so it can actually be unsubscribed. Assigning the lambda first
            // and removing it by reference is the only way to do that -- the old code removed
            // a LoadAsset method group that was never added, leaving the lambda live.
            pendingAssetLoad = () =>
            {
                AssetAPI.OnAssetBundlesLoaded -= pendingAssetLoad;
                pendingAssetLoad = null;

                LoadAsset();
            };

            AssetAPI.OnAssetBundlesLoaded += pendingAssetLoad;
        }
        else
        {
            LoadAsset();
        }
    }

    public void OnDestroy()
    {
        if (pendingAssetLoad != null)
        {
            AssetAPI.OnAssetBundlesLoaded -= pendingAssetLoad;
            pendingAssetLoad = null;
        }

        instances.Remove(this);

        // Normally redundant -- the icon is a child of the tier marker being destroyed -- but
        // needed if only the component is removed. Guarded because a throw out of OnDestroy on
        // an injected il2cpp MonoBehaviour surfaces as a trampoline error.
        try
        {
            Wrapper?.Destroy();
        }
        catch (Exception error)
        {
            Plugin.Logger.LogDebug($"Archivist icon already gone: {error.Message}");
        }

        Wrapper = null;

        m_completeWithNoBoosterIcon = null;
        m_title = null;
        m_rightSideText = null;
    }

    private void LoadAsset()
    {
        if (Icon == null)
        {
            Plugin.Logger.LogError("RundownTierMarkerArchivist.Setup: cannot instantiate NoBooster icon...");
            return;
        }

        Plugin.Logger.LogDebug("RundownTierMarkerArchivist.Setup: setting it up");

        m_completeWithNoBoosterIcon = GOUtil.SpawnChildAndGetComp<CM_ExpeditionSectorIcon>(
            Icon, m_tierMarker.m_sectorIconAlign_main);

        Wrapper = new(m_completeWithNoBoosterIcon.gameObject);

        m_bg = Wrapper.BGGO.GetComponent<SpriteRenderer>();
        m_icon = Wrapper.IconGO.GetComponent<SpriteRenderer>();

        m_title = Instantiate(m_tierMarker.m_sectorIconSummaryMain.m_title);
        m_title.transform.SetParent(Wrapper.ObjectiveIcon.transform, false);
        m_rightSideText = Instantiate(m_tierMarker.m_sectorIconSummaryMain.m_rightSideText);
        m_rightSideText.transform.SetParent(Wrapper.RightSideText.transform, false);

        m_completeWithNoBoosterIcon.m_title = m_title;
        m_completeWithNoBoosterIcon.m_rightSideText = m_rightSideText;

        SetupNoBoosterUsedIcon(true);

        const float scale = 0.16f;
        var localScale = new Vector3(scale, scale, scale);

        m_completeWithNoBoosterIcon.transform.localScale = localScale;

        m_completeWithNoBoosterIcon.SetPosition(new Vector2 { x = 0f, y = 155f });

        // Only joins the update list once the icon it draws actually exists
        instances.Add(this);
    }

    internal void SetVisible(bool visible)
    {
        if (m_completeWithNoBoosterIcon == null)
            return;

        m_completeWithNoBoosterIcon.SetVisible(visible);
    }

    private void OnRundownUpdate(PluginRundown rundown)
    {
        if (!PluginRundowns.WithLogs.Contains(rundown))
        {
            SetVisible(false);
            return;
        }

        UpdateText();
        SetVisible(true);
    }

    private void SetupNoBoosterUsedIcon(bool boosterUnused)
    {
        var icon = m_completeWithNoBoosterIcon;
        icon.m_isFinishedAll = true;
        icon.SetupIcon(icon.m_iconMainSkull, icon.m_iconMainBG, false);
        icon.SetupIcon(icon.m_iconSecondarySkull, icon.m_iconSecondaryBG, false);
        icon.SetupIcon(icon.m_iconThirdSkull, icon.m_iconThirdBG, false);
        icon.SetupIcon(icon.m_iconFinishedAllSkull, icon.m_iconFinishedAllBG, false, false, 0.5f);
        //icon.SetupIcon(m_icon, m_bg, true, boosterUnused, 1.0f, 1.0f);
        var cIcon = m_icon.color;
        var cBg = m_bg.color;
        m_icon.color = new(cIcon.r, cIcon.g, cIcon.b, boosterUnused ? 1.0f : 0.4f);
        m_bg.color = new(cBg.r, cBg.g, cBg.b, boosterUnused ? 1.0f : 0.3f);
        m_title.alpha = (boosterUnused ? 1f : 0.2f);

        icon.m_titleVisible = true;
        icon.m_isCleared = boosterUnused;

        m_bg.gameObject.SetActive(false);

        // Create sprite
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f // pixels per unit
        );

        // Assign
        m_icon.sprite = sprite;
        m_icon.color = Color.white; // ensure it's not tinted

        // blink in sound control
        if (boosterUnused)
        {
            icon.m_isFinishedAll = true;
        }
        else
        {
            icon.m_isFinishedAll = false;
            icon.m_type = LevelGeneration.LG_LayerType.MainLayer;
        }

        icon.m_rightSideText.gameObject.SetActive(false);

        icon.m_title.SetText("<size=120>LOG ARCHIVE</size>");
        icon.m_rightSideText.SetText($"<size=120>[{totalRead}/{totalLogs}]</size>");

        UpdateText();

        icon.m_title.gameObject.SetActive(true);
        icon.m_rightSideText.gameObject.SetActive(true);
        SetVisible(true);
    }

    private void UpdateText()
    {
        if (m_rightSideText == null)
            return;

        var readString = $"{totalRead}";

        if (totalRead == 0)
            readString = $"<color=red>{readString}</color>";
        else if (totalRead < totalLogs)
            readString = $"<color=orange>{readString}</color>";

        m_rightSideText.SetText($"<size=120>[{readString}/{totalLogs}]</size>");
    }

    static RundownTierMarkerArchivist()
    {
        ClassInjector.RegisterTypeInIl2Cpp<RundownTierMarkerArchivist>();
    }
}
