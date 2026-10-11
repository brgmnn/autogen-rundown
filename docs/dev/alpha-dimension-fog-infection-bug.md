# Bug: infectious fog infects players inside the alpha dimension

**Report:** In an AlphaTerminalCommand mission with `Fog_Low_Infectious`, players take
infection while inside the alpha dimension, even though the dimension sits far above the
Reality level and the fog is low-only.

**Status:** Investigated, not fixed. The analysis comes from reading the R6 decompile
(`gtfo-decompile/Modules-ASM`). The newer build's member signatures in
`gtfo-decompile/mydecompile/cpp2il_out` for `LocalPlayerAgentSettings` and
`EnvironmentStateManager` match, but its method bodies aren't there to compare. Nothing has
been run in-game.

## Summary

Two things combine to move the Reality fog's infection plane about 190 m above the alpha
dimension's floor. Infection applies everywhere below that plane, so the whole dimension sits
inside it:

1. **Autogen:** the fog blocks that AlphaOne and AlphaThree_Top point at (IDs 88 and 109) are
   never written to our `FogSettings` datablock. On warp the game finds no block for the
   dimension and keeps the last Reality fog as its target.
2. **Vanilla:** `UpdateBlendTowardsTargetFogSetting` adds the dimension's floor height
   (`GroundY`) to the infection-plane altitude twice.

This only happens when:

- the alpha dimension is AlphaOne or AlphaThree_Top (AlphaSix is fine), **and**
- a Reality `SetFogSettings` event fired earlier in the run, before the warp.

That fits a report that shows up in some runs and not others.

## How fog infection works (decompile)

### Infection is one plane for the whole level

`Modules-ASM/LocalPlayerAgentSettings.cs` owns one `EV_Plane infectionPlane` that it registers
with `EffectVolumeManager`. The plane has no dimension check and no XZ bounds; it only compares
world Y.

`Modules-ASM/EV_Plane.cs`:

```csharp
float y = targetData.referencePosition.y;
if (y > highestAltitude) return 0f;
return y < lowestAltitude ? 1f : Mathf.Clamp01((y - highestAltitude) / (lowestAltitude - highestAltitude));
```

`Modules-ASM/EffectVolume.cs`: `if (!invert) amount = 1 - amount;`

`invert = DensityHeightMaxBoost > FogDensity`. For `Fog_Low_Infectious` that's 0.045 > 0.0007,
so `invert = true`, which means **infection applies below the plane**.

### Dimensions are high up in world space

`Modules-ASM/LevelGeneration/LG_Floor.cs` `CreateAllDimensions` stacks non-arena dimensions
above Reality, starting at Y=150:

```csharp
float num2 = 150f;
float num3 = num2 + dimensionData3.VerticalExtentsDown;
Vector3 position = up * num3;            // Dimension.GroundY = Position.y
num2 = num3 + dimensionData3.VerticalExtentsUp;
```

For AlphaTerminalCommand the Arena dimension is skipped in this stack, so `Dimension1`'s floor
(`GroundY`) is:

| Dimension      | VerticalExtentsDown | GroundY |
|----------------|---------------------|---------|
| AlphaOne       | 40                  | 190     |
| AlphaThree_Top | 10                  | 160     |
| AlphaSix       | 55                  | 205     |

### Fog state is stored per dimension

`Modules-ASM/EnvironmentStateManager.cs` keeps `FogState[21]`, one per dimension
`CreationOrderIndex`.

- **Elevator land:** `Modules-ASM/ElevatorRide.cs` `PlayerExit` (host only) starts a fog
  transition for every dimension using `DimensionData.DimensionFogData`, falling back to
  `FogTransitionDataOnElevatorLand`. Reality's `DimensionData` is a default instance with
  `DimensionFogData = 0`, and autogen sets `FogTransitionDataOnElevatorLand = 0`, so **Reality
  gets no fog state**.
- **Warp:** `Modules-ASM/Player/PlayerAgent.cs` `WarpTo` calls
  `EnvironmentStateManager.UpdateFogSettings()`, which looks up the fog block for the player's
  new dimension.

## What goes wrong

### 1. The dimension fog block doesn't exist (autogen)

`AutogenRundown/src/DataBlocks/Dimensions/DimensionData.cs`:

- `AlphaOne` → `Fog = new Fog { PersistentId = 88 }`
- `AlphaThree_Top` → `Fog = new Fog { PersistentId = 109 }`
- `AlphaSix` → `Fog = Fog.AlphaSix` (92, persisted in `Fog.SaveStatic`)

These are only placeholders that set `DimensionFogData`. `Fog` has no vanilla `Setup<>` load
(see `BlocksBin.Setup`), and `Fog.SaveStatic()` never persists 88 or 109. Generated IDs start
at 100000 (`Generator.pid`), so nothing else lands on those IDs. Our `FogSettings` file has no
block 88/109, while the `Dimension` block still points at them.

The same problem applies to the other preset placeholders: 87 (AlphaTwo), 93 (Unknown_Three
and `Dimension.cs`), 94 (Unknown_One) and 106 (AlphaThree_Shaft).

`EnvironmentStateManager.UpdateFogSettingsForState`, when the player is in Dimension1:

```csharp
FogSettingsDataBlock block = GetBlock(state.FogStates[dimIdx].FogDataID); // 88 → null
if (block != null)
    LocalPlayerAgentSettings.SetTargetFogSettings(block, startSettings, blend); // skipped
m_hasOngoingFogTransition = true;
LocalPlayerAgentSettings.Current.UpdateBlendTowardsTargetFogSetting(fogBlend); // blend = 1
```

`m_targetFogSettings` keeps whatever Reality target was last set:

- **A Reality fog event fired before the warp:** the target is the Reality infectious fog. This
  goes on to case 2 below, where the player gets infected. Possible sources:
  - `Zone.AlarmModifier_FogFlood` (door-scan fog)
  - `EventBuilder.AddFillFog` in `LevelLayout.ZoneBuildChallenge.cs`
  - `WardenObjective.AddSlowFogFill`
  - any other `AddSetFog`
- **No Reality fog event:** the target is null and `UpdateBlendTowardsTargetFogSetting` returns
  early. The original Reality plane at -4 to -1.5 stays registered, which is harmless at
  Y≈190.

A Reality fog event fired *while* the team is in the dimension doesn't change the target,
because the lookup uses the player's current dimension, and that block is still missing.

### 2. The dimension floor height is added twice (vanilla)

`LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting`:

```csharp
float num = 0f;
if (Dimension.GetDimension(playerAgent.DimensionIndex, out dimension))
    num = dimension.GroundY;
...
prelitVolume.m_densityHeightAltitude = Mathf.Lerp(m_fogSettings.DensityHeightAltitude,
    m_targetFogSettings.DensityHeightAltitude + num, amount);          // + GroundY
...
currentInfection = Mathf.Lerp(m_fogSettings.Infection, m_targetFogSettings.Infection, amount);
infectionPlane.invert = prelitVolume.m_densityHeightMaxBoost > prelitVolume.m_fogDensity;
infectionPlane.lowestAltitude  = prelitVolume.m_densityHeightAltitude + num;                  // + GroundY again
infectionPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + prelitVolume.m_densityHeightRange + num;
```

Worked example: `Fog_Low_Infectious` (altitude -4, range 2.5, infection 0.03, inverted)
applied in AlphaOne (`GroundY = 190`):

| Item                        | World Y       |
|-----------------------------|---------------|
| Dimension floor             | 190           |
| Fog drawn at                | 186           |
| Infection plane bottom/top  | 376 / 378.5   |
| Without the double offset   | 186 / 188.5   |

The player is far below the plane, so they get the full 0.03/s rate (full infection in roughly
33 s) anywhere in the dimension. Without the double offset the plane would sit below the
dimension floor and do nothing. The Reality fog is also drawn 4 m below the dimension floor.

## Why AlphaSix and vanilla don't show this

- **AlphaSix:** block 92 exists with `Infection = 0`. The target becomes that block,
  `currentInfection` blends to 0, and the plane is unregistered.
- **Vanilla:** every dimension fog block exists. The double offset only matters for dimensions
  whose own fog is infectious; in the R6 dump that's `D4_Fourth_dimension` and
  `Rundown006_D4_4`, both using fog 40 (infection 0.03).
- **Vanilla dimension fogs** in the dump (`GameData-Dump/34855`) all have `Infection = 0`:
  87, 88, 92, 93, 94, 106 and 109.
- **GatherSmallItems** also uses `DimensionData.AlphaOne`, so it has the same exposure.

## Second effect when the team warps back

AlphaTerminalCommand warps the team back to Reality after the transfer. Reality has no fog
state (`FogDataID = 0`), so:

1. The block lookup is null again, so the target isn't replaced.
2. `FogState.GetFogBlend()` returns 0: start time and duration are both 0, and
   `Mathf.InverseLerp(a, a, v)` returns 0.
3. `UpdateBlendTowardsTargetFogSetting(0)` snaps back to `m_fogSettings`, the start settings of
   the last transition. Those come from `PreLitVolume.GetFogSettings()`, which never copies
   `Infection`, so `currentInfection = 0` and the plane is unregistered.

Result: after the return, Reality's fog can look like it did before the last event and may stop
infecting.

## Suggested fixes (not applied)

1. **Add the missing fog blocks.** Persist real `Fog` presets for 87, 88, 93, 94, 106 and 109,
   copying the vanilla values from `GameData-Dump/34855/GameData_FogSettingsDataBlock_bin.json`
   the way `Fog.AlphaSix` does. Use those presets in `DimensionData` and persist them in
   `Fog.SaveStatic()`. This removes the null-block path, so the infection in the dimension goes
   away.
2. **Give Reality its own fog state.** For levels with dimensions, set
   `FogTransitionDataOnElevatorLand` to the level fog's `PersistentId`. Otherwise, after fix 1,
   warping back leaves the target on the dimension fog (infection 0) and Reality's infectious
   fog stops working after the return.
3. **Optional:** a Harmony patch for the double `GroundY` in
   `LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting`. Only needed if we ever want
   infectious fog inside a dimension.

## How to confirm in-game

- Check the generated `BepInEx/GameData/<rev>/GameData_FogSettingsDataBlock_bin.json` for
  `persistentID` 88 and 109. They should be missing. This assumes the generated file replaces
  the vanilla one rather than merging with it.
- After the warp, the game log should show the `EnvironmentStateManager` `OnStateChange` line
  with `fogDataId 88` (or 109).
- Reproduce: an AlphaOne or AlphaThree_Top mission with `Fog_Low_Infectious`. Trigger a Reality
  fog event (for example a FogFlood alarm door) before entering the portal, then stand in the
  dimension and watch infection rise.
