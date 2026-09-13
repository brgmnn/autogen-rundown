# Repo status review — 2026-09-13 (v1.1.1)

A point-in-time audit of the repository: build/test/CI state, outstanding bugs, and
improvement candidates. Everything under "verified" was checked directly in code or by running
commands on this date. Items marked *unverified* came from automated sweeps and were not
independently confirmed. Line numbers are as of `main` at `404811f`.

## Current state (verified)

- **Branch**: `main` at `404811f` (v1.1.1, 2026-09-01), clean working tree. `release/1.1.0` is
  fully merged (PR #42). No `release/1.2.0` branch exists yet.
- **Upstream**: local checkout was 4 commits behind. Two dependency bumps were merged by the
  scheduled `Update Dependencies` workflow: #43 ExcellentObjectiveSetup 1.1.3→1.1.4 (09-08)
  and #44 BorkenCellGeoFix 0.5.5→0.5.6 (09-13). Only `manifest.json` and `thunderstore.toml`
  changed.
- **Build**: `dotnet build AutogenRundown.sln` passes, 0 errors, 177 warnings (21 distinct).
- **Tests**: 351 MSTest methods. They cannot run on the macOS dev box: both projects target
  `net6.0` and only the .NET 10 runtime is installed, so `dotnet test` aborts with
  "framework Microsoft.NETCore.App 6.0.0 missing". CI (`Build .NET`, Ubuntu) runs them and is
  green on every recent run.
- **GitHub**: 1 open issue (#26, merging all EEC properties for peer mods, self-assigned
  Nov 2025). No open PRs.
- **Stale stash**: `stash@{0}` on `4199d13` (Jan 2026) holds one README line
  ("Disable yeetartifactheat mod"). The surrounding README has diverged, so it would conflict
  on pop.
- **gtfo-decompile**: untracked `docs/il2cpp-icf-folded-methods.md` (63 lines) was never
  committed.
- **Reference DLL drift**: `update-dependencies.py` only bumps the two manifests, never the
  compile references in `AutogenRundown/plugins/*.dll`. `AdvancedWardenObjective.dll` was last
  vendored 2025-05-30 (assembly 2.1.0) while the manifest pins AWO 2.5.7; the LocalProgression
  DLL is from 2025-11 vs pin 1.3.7. The compile surface can silently lag the runtime surface.
  (Assembly versions do not always track Thunderstore versions, but the dates alone show the
  drift.)

## Notable warning sites (from the build log)

Mostly benign. Two would throw at runtime if their assumption ever breaks:

- `AutogenRundown/src/DataBlocks/LevelLayout.StartingArea.cs:186` — `bulkhead switch` is not
  exhaustive; `Bulkhead.None` would throw `SwitchExpressionException`.
- `AutogenRundown/src/DataBlocks/LevelLayout.ZoneProgression.cs:1427` — `level.Tier switch` is
  not exhaustive; an unknown tier throws.
- `AutogenRundown/src/DataBlocks/LevelLayout.RetrieveBigItems.cs:135` — `(ZoneNode)startish`
  unchecked null cast.
- `AutogenRundown/src/Patches/CustomTerminals/Patch_SpawnCustomTerminals.cs:363` — CA1416,
  InjectLib JSON deserialize is marked Windows-only. Fine in practice.
- Dead fields (CS0414/CS0169): `RundownFactory.soloPrefix/duoPrefix`,
  `Fix_NavMeshMarkerSubSeed.s_suppressHook`, `*Archivist.m_sprite`.

## Outstanding bugs

### A. Verified, non-generation (safe to fix any time, no Generator draw impact)

1. **`AutogenRundown/src/DataBlocks/BlocksBin.cs:115`** — `MiningMarkers.Save("MiningMarkers")`
   writes `GameData_MiningMarkersDataBlock_bin.json` (plural). The game file, and the matching
   `Setup<MiningMarker>(…, "MiningMarker")` at `Markers/MiningMarker.cs:17`, are singular. The
   plural file is never loaded, so the custom terminal marker (PID 156) added in
   `MiningMarker.SaveStatic` silently never reaches the game. Nothing references it yet, so
   the visible damage today is a dead feature plus a stray unread JSON in the user's GameData
   folder. One-word fix.
2. **`AutogenRundown/AutogenRundown.csproj:23`** — `<DefineConstants>DEBUG</DefineConstants>`
   is gated on `$(Debug) != 'false'`, not on `$(Configuration)`. A plain
   `dotnet build -c Release` (the command documented in the repo `CLAUDE.md`) still defines
   `DEBUG`, shipping the geomorph test level (`RundownFactory.cs:199`) and the travel-path
   debug overlay. CI passes `-p:Debug=false` so published artifacts are clean; local Release
   builds are not. Suggested condition:
   `'$(Configuration)' == 'Debug' And '$(Debug)' != 'false'`.
3. **`AutogenRundown/src/Patches/Patch_LG_Factory.cs:82-83`** — two `[HarmonyPatch]`
   attributes stacked on `SupressEventHandlers`. Harmony merges multiple attributes on one
   method into a single target (last non-null wins) rather than two targets, so only one of
   `Builder.OnFactoryDone` / `EnvironmentStateManager.OnFactoryBuildDone` is actually
   suppressed during rebuilds. `Patch_UplinkWaveIsolation.cs:79-85` shows the correct
   `[HarmonyTargetMethods]` pattern.
4. **`AutogenRundown/src/Patches/Patch_UplinkWaveIsolation.cs:217-241`** —
   `GetEnemyWaveEventIDs` uses `typeof(WardenObjectiveManager).GetField("m_enemyWaveEventIDs")`.
   Il2CppInterop exposes il2cpp fields as properties, so this always returns null, and the
   `as List<ushort>` cast is the managed list type anyway. Result: the "remove from global
   tracking list" step at `:208` is a permanent no-op and the warning at `:236` logs on every
   call. Read `WardenObjectiveManager.Current.m_enemyWaveEventIDs` directly instead.
5. **`AutogenRundown/src/Patches/Patch_LG_Floor.cs:15`** — `_patchedDimensions` is added to at
   `:190`, read at `:234`, and never cleared (no `OnLevelCleanup` / `NewBuild` reset).
   `eDimensionIndex` values repeat across levels, so a later level's Dimension_1 inherits the
   renderer/culler override from an earlier one. Every other cross-build dictionary is reset in
   `Managers/FactoryJobManager.cs:85-92`.
6. **`.github/workflows/release-assets.yml:28`** — selects the build run with
   `--status completed`, which includes failed and cancelled runs. Should be `--status success`.
   Low severity since the workflow is dispatched by hand.
7. **`AutogenRundown/src/Patches/Patch_LG_Layer.cs:39` and
   `Patch/Fix_NavMeshMarkerSubSeed.cs:171`** — both write `zoneData.MarkerSubSeed` in prefixes
   on `LG_Layer.CreateZone`, from two different dictionaries (`ZoneSeedManager.MarkerSubSeeds`
   keyed by dim/layer/zone; `Fix_NavMeshMarkerSubSeed.MarkerSubSeeds` keyed by dim/zone), with no
   `[HarmonyPriority]`. If both hold an entry for a zone, whichever prefix runs last wins.
   *Impact unverified.*

### B. Verified pattern violations in layout builders (generation-affecting)

These change Generator draws. The hand-tuned seed pools depend on draw order, so these can
only land at a seed re-roll point (next monthly roll / 1.2.0), or must be proven to alter draws
only in levels that would otherwise fail to build.

8. **Hub nodes set `MaxConnections` above the hard cap of 3.** Zones have four cardinal sides
   and the parent uses one, so a hub can take at most 3 children. All violations are computed,
   none literal:
   - `AutogenRundown/src/DataBlocks/LevelLayout.ZoneBuildChallenge.cs:525`
     `BuildChallenge_CascadingRelay`: `relayCount + 1` (A: 3, B: 3-4, C: 4, D: 4-5, E: 5), then
     attaches `end` + relayCount children to that hub.
   - `:743` `BuildChallenge_QuarantineCascade`, `:1077` `BuildChallenge_CrossReferenceDossier`,
     `:1202` `BuildChallenge_ForensicReconstruction`: same shape, same tier table.
   - `AutogenRundown/src/DataBlocks/LevelLayout.TerminalUplink.cs:385` and `:883`:
     `Uplink_NumberOfTerminals + 1` (up to 5 on E-Main, always ≥ 4 at `:883`).
   - `AutogenRundown/src/DataBlocks/LevelLayout.Cryptomnesia.cs:427-429`:
     `realityChildren.Count + 1` on the mirrored dimension node (4 when the Reality hub has 3
     children, which `GenHubGeomorph` makes routine).

   The four challenge builders are drawn at C-Main uplink with combined weight ~0.40
   (`TerminalUplink.cs:407-430`) and have shipped since 2026-03-26 (`ed09c45`). Either they are
   producing rebuild loops that the v1.1.0 rebuild cap now reports as "Unreachable", or the cap
   is softer for hubs whose coverage adds extra tiles and expanders. Check rebuild logs for
   seeds that draw these before deciding. Fix shape: chain relays (relay_b off relay_a) instead
   of fanning from one hub, so no node ever exceeds 3.

   Planner context: `Zones/LayoutPlanner.cs` defaults `MaxConnections` to 2; `Connect()` at
   `:248-251` only logs a warning when a node is over capacity and adds the edge anyway;
   `UpdateNode()` at `:272-302` performs no validation; `CountOpenSlots` clamps at 0 so
   over-subscription is invisible to `CanConsumeOpenSlots`. No generation-time invariant check
   exists, so these only surface as the in-game "Failed to find any good StartAreas" loop.

9. **Challenge builders still mutate the elevator/start node** — the pattern fixed in `af5d733`
   for PowerCellDistribution. On main-only levels `start` is the elevator zone
   (`MainOnly_NoBulkhead` → `BuildStartingArea_Default` builds 0 extra zones, and
   `StartingArea_GetBuildStart` returns `GetLastZone(Main)`; `StartingArea.cs:150-155`,
   `:246-252`). On `DisableStartingArea` objectives (ReachKdsDeep, Cryptomnesia) it is zone 0
   regardless of bulkhead config (`StartingArea.cs:140-142`). The known harm is
   `MaxConnections = 3` / `Coverage = Small` on the elevator zone, a rebuild-loop trigger since
   the elevator tile pool often lacks a third free expander. A hub geomorph on Reality's
   elevator zone is probably tolerated (the shaft is its own tile) but is fatal on dimension
   origin tiles, as the comment at `LevelLayout.AlphaTerminalCommand.cs:36-40` records.

   Highest-value sites:
   - `LevelLayout.GatherSmallItems.cs:182-184` passes **2** side zones to
     `BuildChallenge_LockedTerminalDoor(start, …)` precisely when `Bulkheads == Main`.
     Also `:149, 293, 306, 520, 533, 1034-1035` (hub/corridor on `start`), `:164, 173, 330,
     339, 349, 584, 592, 602` (`GeneratorCellInSide` / `KeycardInSide` on `start`), and
     `:193, 380, 614` (`BossFight` on `start`).
   - `LevelLayout.HsuFindSample.cs:77, 96-97, 216, 224, 258-259, 437-438, 451-452, 505-506,
     761-762, 1070-1071, 1132-1133`: `UpdateNode(start with { MaxConnections = 3 })` followed
     by `GenHubGeomorph(start)` / `GenTGeomorph(start)` with no main-only guard. (Sibling
     lambdas at `:231, :255, :433` are safe: they branch first.)
   - `LevelLayout.TerminalUplink.cs:218, 409, 416, 423, 430, 654, 669, 842, 910` pass raw
     `start` into the item-8 hub builders or `BossFight` (double bug on main-only levels).
   - `LevelLayout.TimedTerminalSequence.cs:21` `GenCorridorGeomorph(start)` as the first
     statement.
   - `LevelLayout.CentralGeneratorCluster.cs:1355` `GenCorridorGeomorph(start)` on a 0.7 flip.
   - `LevelLayout.Cryptomnesia.HubChain.cs:40` hub on the Reality elevator (every
     Cryptomnesia level, since HubChain is the only enabled layout).
   - `LevelLayout.Reactor.cs:34,42` `BuildReactor` rewrites its argument's branch and
     geomorph (`GenReactorCorridorGeomorph`, `MaxConnections → 1`); called with raw `start`
     from `ReactorStartup.cs:50` (where `preludeCount == 0` at tier A returns `[start]`) and
     throughout `ReactorShutdown.cs`.
   - Unconditional mutators in `ZoneBuildChallenge.cs`: `GeneratorCellInSide` (`:108-114`),
     `KeycardInSide` (`:162-168`), `LockedTerminalPasswordInSide` (`:216-222`), `BossFight`
     (`:282-288`, via `AddAlignedBoss` → `GenBossGeomorph`), plus the four hub builders in
     item 8.
   - Low priority: `SpecialTerminalCommand.cs:144, 153, 686, 905, 924, 930, 958` in
     `Bulkhead.Main` cases (effectively dead: `BuildDirector.cs:113-114` removes the objective
     from Main draws). `AlphaTerminalCommand.cs:144` passes `start` instead of the
     `challengeRoot` it created (ApexAlarm does not mutate, so latent only).

   Confirmed safe for contrast: `GatherTerminal.cs:67-68` and
   `PowerCellDistribution.cs:113, 138, 153, 163, 299, 346, 356` carry the main-only approach
   guard; `AlphaTerminalCommand.cs:42` isolates via a corridor. Fix shape: that guard, or a
   single helper that inserts a corridor when the node carries the elevator tag.

10. **`Level.CanConsumeOpenSlots` (`Level.cs:1372-1375`) returns `true` for
    `DisableStartingArea` levels** even though `BuildStartingArea` returns right after
    `CreateElevatorZone()` (`StartingArea.cs:102-107`) and so never pre-places the Extreme /
    Overload entrances; those are deferred to `StartingArea_GetBuildStart:210-217`. The
    v1.1.1 med-bay/disinfection gates (`ZoneBuildOptional.cs:159`, `LevelLayout.cs:1096`,
    `:1122`) are therefore inert on ReachKdsDeep and Cryptomnesia. Related:
    `LevelLayout.CryptomnesiaTheme.cs:307-313` (`ApplyTheme_InfectionFog`) adds a dead-end
    disinfection zone from `Cryptomnesia.cs:119`, before
    `PrePlaceCryptomnesiaSecondaryBulkheads()` at `:191`, with no gate at all.

11. **`LevelLayout.ZoneBuildOptional.cs:162`** calls `GetOpenZones(director.Bulkhead, null)`
    without the `Dimension` argument (defaults to Reality) while the gate three lines above
    passes `Dimension`. Only matters if the med bay is ever built for a dimension layout.
    `ZoneProgression.cs:1886-1888` (`AddKeyedPuzzle`) has the same missing argument.

### C. Minor / latent (verified unless marked)

- `Level.cs:1228-1240` and `:1689` index `Director[bulkhead]` directly. `GetDirector()`
  (`Level.cs:416`) exists because composite flags like `Main | StartingArea` throw
  `KeyNotFoundException` on the raw dictionary. All current callers pass clean values, so
  this is latent. The TODO at `Level.cs:389` is legitimate; ~2 of ~46 sites are migrated.
- `Fix_NavMeshMarkerSubSeed.cs:257-258`: a Zone_0 zone gets its own sub-seed rerolled but the
  parent reroll is skipped (the parent lives in another layer). Only a gap when the parent's
  seed is the actual cause. Dimensions are already passed through.
- `ChainedPuzzle.cs:1311` and `:1348`: the 5 s `Environment_DoorUnstuck` telegraph before a
  secret Tank/Mother spawn never plays; the 8 s roar at delay 8.0 still does. Cosmetic.
- `Patch_LG_NodeTools.cs:700-705`: `_isClusterPlacement` is set in a prefix and cleared in a
  postfix; if `CP_Cluster_Core.Setup` throws it stays set for the session and is never reset
  on cleanup. Should be a `[HarmonyFinalizer]` like `Fix_MissingSpawnAligns.cs:190`.
  *Unverified.*
- `Patch_LG_Floor.cs:42-152`: ~35 unconditional `LogDebug` calls plus `GetComponents` /
  `GetComponentsInChildren<MeshRenderer>` sweeps in the `CreateDimension` postfix, in Release.
  Leftover instrumentation. Similar per-expander `LogInfo` at
  `Patch_LG_ZoneJob_CreateExpandFromData.cs:194-224` and the per-zone "GOTCHA!!!" at
  `Fix_NavMeshMarkerSubSeed.cs:147-149`. *Unverified.*
- `Dimension.cs:129` sharing the WaveSettings PID counter is **not** a collision bug: neither
  the Dimension nor the WaveSettings bin is seeded with vanilla blocks (both `Setup()`s are
  empty), so each output file contains only mod blocks. A dedicated `PidOffsets.Dimension`
  would be for clarity only.
- `LevelSettings.cs:213` TODO says `GenerateHibernatingEnemyPack` is unused; it is called at
  `:578`. Whether `EnemyHibernationPack` has any downstream reader is the real question.
- `HideMenuItems.cs:39` postfixes `CM_PageRundown_New.OnExpeditionUpdated`, which is empty in
  the R6 decompile (`Modules-ASM/CellMenu/CM_PageRundown_New.cs:564-566`). That is the
  IL2CPP identical-code-folding trap described in
  `gtfo-decompile/docs/il2cpp-icf-folded-methods.md`, but the live build is newer than the
  decompile and this patch has shipped for a long time without a crash. Verify with the
  `NativeMethodInfoPtr` recipe in that doc rather than changing it blind. Same caveat for
  `Patch_CM_RundownSelection.cs:65` (`GUIX_Layer.SetColor`), `RundownTimers.cs:36, 62`
  (`GUIX_Layer.OnEnable/OnDisable`), and `HideMenuItems.cs:43` (`UpdateVanityItemUnlocks`),
  whose bodies are not in the decompile. Other small-bodied targets flagged by the sweep,
  all *unverified* for fold risk: `Patch_GTFuckingXP.cs:5` (`PlayerGuiLayer.Update`, 2 stmts),
  `Patch_LG_Floor.cs:291` (`LG_ZoneExpander.IsLayerSource`, 1 stmt),
  `Patch_SpawnCustomTerminals.cs:87` (`LG_DistributionSetup.Build`, 3 stmts),
  `Patch_CentralGeneratorCluster.cs:52`, `Watermark.cs:6`, `Patch_ArtifactHeatIntel.cs:15`.
- Unguarded il2cpp dereferences in patches (*unverified*, from the sweep, highest first):
  `Glowsticks.cs:54` (`m_light` null when `TryAllocateFXLight` fails, plus `Owner.Owner`);
  `Patch_LG_ComputerTerminal_Setup.cs:273` (`SpawnNode` is nullable in vanilla `Setup`);
  `Patch_SpawnCustomTerminals.cs:124` and `:68` (same `SpawnNode`, and a managed null check on
  a Unity object); `Patch_LG_NodeTools.cs:160-161` (`m_reachableNodes` inside a raw native
  detour with no try/catch, where a managed exception is a hard crash);
  `Patch_SustainedTravelReverse.cs:137-139, 158-159` (indices bounded by `AmountOfPositions`,
  not `positions.Count`); `TerminalUplink.cs:932, 920` (`UplinkPuzzle` unchecked; `(object)`
  cast bypasses Unity fake-null); `Patch_UplinkWaveIsolation.cs:60, 134`;
  `Patch_LG_SecurityDoor.cs:195-197`; `Patch_CM_ExpeditionWindow.cs:31-40`;
  `LogArchivist.cs:17, 26`; `Fix_NavMeshMarkerSubSeed.cs:271` (`m_zoneSettings` unchecked
  while `m_zoneData` is).
- Lifecycle (*unverified*): `Patch_LG_SecurityDoor.cs:186-226` `_deferredActions` is only
  cleared on `OnLevelCleanup`, which the rebuild path deliberately skips
  (`FactoryJobManager.cs:119-125`), so closures over destroyed door objects survive a rebuild.
  `TravelScanRegistry.cs:168` `Clear()` is likewise wired only to `OnLevelCleanup`; its
  `HashSet<IntPtr>` of object addresses can match recycled allocations after a rebuild.
- `LG_ZoneJob_CreateExpandFromData.Build` has 7 prefixes, 2 postfixes and 1 finalizer across
  five files with no `[HarmonyPriority]` anywhere. Concrete hazard (*unverified*):
  `Patch_ForceMinAreaCount.Pre_Build` sets `m_minCoverage = 1e9f` (`:160`) and
  `Patch_LG_ZoneJob_CreateExpandFromData.FixCustomGeoCoverageExhaustion` reads it (`:378`); if
  the former runs first the custom-geo retry path is dead.
- Never-reset statics (*unverified*, low impact): `Patch_GTFuckingXP._done` (`:8`),
  `RundownTimers.TimerData` (`:12`), `ScanSpeedNormalize.Pending`
  (`Patch_NormalizeScanSpeed.cs:427`, cleared in a postfix not a finalizer).
- `Patch_LG_Sign.cs:10` has its class-level `[HarmonyPatch]` commented out, so `PatchAll`
  skips the class and both method patches are dead. Looks intentional.
  `Patch_CP_Bioscan_Hud.cs` is an empty class still carrying a `[HarmonyPatch]` attribute.
- `Fix_WeakDoorRecall.cs:338-350`: a `[HarmonyFinalizer]` on `SNet_Replicator.RevieveBytes`
  returning `null` swallows every exception from every replicator recall, mod-wide. Deliberate
  and documented, but it hides genuine bugs in unrelated replicators behind a `LogWarning`.
- Audit claim **rejected**: "`Fix_NavMeshMarkerSubSeed.cs:266` NREs when another prefix
  returns false". `CaptureState` (`:246`) returns void with `out __state`; Harmony only skips
  bool-returning prefixes (or those taking `ref __runOriginal`) once `runOriginal` is false,
  so the state is always captured.
- Audit result **confirmed clean**: no slot-indexed player enumeration anywhere in `src`
  (all sites use `PlayerManager.PlayerAgentsInLevel` or `GetLocalPlayerAgent`;
  `Patch_NormalizeScanSpeed.cs:475` clamps into the game's own `float[4]` correctly).
- `#if DEBUG` / `#if true` / `#if false` blocks are all correctly excluded in a true Release
  build: the `#if true` at `RundownFactory.cs:200` is nested inside `#if DEBUG`; `#if false`
  at `RundownFactory.cs:312-371`; `ENABLE_SOLO_RUNDOWN` / `ENABLE_DUO_RUNDOWN` are never
  defined. The only problem is item 2 (which builds are actually Release).

## Improvements (non-bug)

- **Docs**: the repo `CLAUDE.md` says version 0.80.0 (actual 1.1.1), says config binds live in
  `Plugin.cs` (they moved to `src/Config/PluginConfig.cs`; `Plugin.cs` has no `Config.Bind`),
  lists 3 config keys where `PluginConfig.cs` binds ~15 across the main file and the
  undocumented `…AutogenRundown.Advanced.cfg` (`PluginConfig.cs:103-160`: `UnlockAllLevels`,
  `MaxLevelRebuilds`, `Daily/ForceComplex`, `Daily/PreferGardens`, 8 `RebuildChecks/*`),
  omits `src/Config/` and six root files (`Collections.cs`, `CustomGameData.cs`,
  `LocalProgression.cs`, `Peers.cs`, `PluginRundown.cs`, `Prefab.cs`), and omits the
  `GTFuckingXP.cs` / `SupportedMod.ExtraEnemyCustomization.cs` peer-mod files. `AGENTS.md`
  omits the Gale deploy target and the `PostClean` target (`csproj:98-126`), and its
  "use the default build to deploy" instruction silently no-ops on macOS
  (`$(OS) == 'Windows_NT'` condition). Its `net6.0` claim is correct.
- **Release safety**: three hand-edited copies of the version (`manifest.json:3`,
  `thunderstore.toml:8`, `Plugin.cs:34`; no `<Version>` in either csproj) with no CI check that
  they agree. `build.yml:29` reads only `manifest.json`. Neither `release-assets.yml` nor
  `release-thunderstore.yml` re-runs tests; the latter just republishes the attached asset.
- **CI blind spot**: the six csproj deploy/clean targets are Windows-only and CI is Ubuntu with
  `Debug=false`, so they are never exercised anywhere but a Windows dev box.
- **Tests**: well covered are Generator, BuildDirector, ChainedPuzzle, LayoutPlanner (partial),
  ZoneSensors, TravelScan surface tracing, WardenObjectiveEventCollections. Zero coverage on
  all 24 `LevelLayout.*.cs` builders, all `WardenObjective.*.cs`, `RundownFactory.cs`,
  `Zone.cs` alarm/fog rolls, `LevelSettings.cs`, `BlocksBin.cs` (a filename round-trip test
  would have caught bug 1), `PeerMods/`, `Managers/`, and most of `Patches/`.
  `LayoutPlanner.UpdateNode` (`LayoutPlanner.cs:272`) has a "TODO: add unit tests" and
  `LayoutPlannerTests.cs` already exists. A generation-time planner invariant check
  (children ≤ 3 and ≤ `MaxConnections`) would turn item 8 into a log line instead of an
  in-game rebuild loop.
- **Local test loop**: install the .NET 6 runtime, or move the test project to a TFM the Mac
  has, so `dotnet test` works locally.
- **Dependency script**: extend `update-dependencies.py` to refresh `AutogenRundown/plugins/`
  DLLs when it bumps a pin.
- **Roadmap items already tracked in-repo**: README TODO "Weekly E1 (2027_07_27) not loading
  (Extreme built from a Max=0 side zone)" still listed after 1.1.1; cycling security scan
  global sound still open; `UpkeepProtocol` signature implemented but disabled pending Windows
  playtest (`LevelSettings.cs:513`); E-tier C4 pure-population alarms and C5 trapped doors next
  per `docs/dev/e-tier-difficulty.md`; 123 TODO/FIXME markers in `src`.

## Recommended plan

### Batch 1 — any time, on `main` (no generation impact)

Fix items 1-6. Each is a one-to-five-line change:
- `BlocksBin.cs:115` → `"MiningMarker"`.
- `AutogenRundown.csproj:23` → condition on `'$(Configuration)' == 'Debug' And '$(Debug)' != 'false'`.
- `Patch_LG_Factory.cs:82-83` → `[HarmonyTargetMethods]` returning both targets.
- `Patch_UplinkWaveIsolation.cs:217-241` → drop reflection, read the interop property.
- `Patch_LG_Floor.cs` → clear `_patchedDimensions` where the other cross-build state is reset.
- `release-assets.yml:28` → `--status success`.

Plus: `git pull`, drop the stale stash, commit the decompile doc, refresh `CLAUDE.md` and
`AGENTS.md`.

### Batch 2 — with the next seed roll (generation-affecting)

Items 8-11, in this order: add the planner invariant check first (so the rest can be verified
from generation logs rather than in-game), then cap the hub builders at 3 by chaining, then
apply the approach-branch guard to the item-9 sites, then extend `CanConsumeOpenSlots` for
`DisableStartingArea` and gate the Cryptomnesia theme's disinfection zone. Verify with a
GameData JSON byte-diff against the current seeds that only would-fail levels change.

## Verification

- Batch 1: `dotnet build AutogenRundown.sln -c Release` (without `-p:Debug=false`) must produce
  a DLL with no geomorph test level; the build output must contain
  `GameData_MiningMarkerDataBlock_bin.json` and no plural file; CI `Build .NET` green; an
  in-game rebuild on Windows still works with both factory-done handlers suppressed.
- Batch 2: a full daily/weekly/monthly/seasonal regeneration shows no
  `Planner.Connect() exceeded MaxConnections` warnings; byte-diff of `build/<rev>/GameData_*`
  against the pre-change output; Windows playtest of one C-Main uplink seed that draws
  CascadingRelay.
