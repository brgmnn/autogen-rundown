# Autogen Rundown 🎲

Automatic Rundown generation, using procedural seed based generation. Four active rundowns to choose from: the daily, weekly, monthly, and each season. Play with friends with zero configuration of seeds.

![Rundown Selection](https://github.com/brgmnn/autogen-rundown/blob/main/docs/rundown_selection_loop_thunderstore.apng?raw=true "Rundown Selection")

Track your progression in each rundown separately from the base game and other modded rundowns, see if you can clear them "Unaugmented"! (Without boosters)

![Monthly Rundown Preview](https://github.com/brgmnn/autogen-rundown/blob/main/docs/monthly_rundown.jpg?raw=true "Monthly Rundown Preview")

Levels and rundowns are designed to be similar and in the spirit of vanilla GTFO. The largest difference is in the addition of new tilesets (geomorphs) to add more variety to the existing games set of tiles.

## Peer mod support

AutogenRundown supports the following 3rd party peer mods. You can install them along side AutogenRundown and play with modified weapons etc in the random rundowns.

- [Arsenality](https://thunderstore.io/c/gtfo/p/W33B/Arsenality)
- [ArsenalityRebalance](https://thunderstore.io/c/gtfo/p/leezurli/ArsenalityRebalance)
- [VanillaReloaded](https://thunderstore.io/c/gtfo/p/tru0067/VanillaReloaded)
- [GTFriendlyO](https://thunderstore.io/c/gtfo/p/Carb_Crusaders/GTFriendlyO/)
- [OmegaWeapons](https://thunderstore.io/c/gtfo/p/Mimikium/OmegaWeapons/)

## Customizing Autogen Datablocks

You can override any generated datablock or custom JSON file by placing partial files in a `GameData-Custom` folder. Your changes are **deep-merged** into the generated output — you only need to specify the properties you want to change. Everything else is preserved.

Custom overrides are applied **last**, after all rundown generation and peer mod configuration.

### Setup

Create the folder:

```
BepInEx/GameData-Custom/
```

This folder mirrors the structure of `BepInEx/GameData/{revision}/`. Place your override files using the same relative paths. For example:

```
BepInEx/
  GameData-Custom/
    GameData_EnemyBalancingDataBlock_bin.json
    GameData_FogSettingsDataBlock_bin.json
    Custom/
      ExtraEnemyCustomization/
        Property.json
```

### Modifying Datablocks (persistentID matching)

Datablock files (`GameData_*DataBlock_bin.json`) contain a `Blocks` array where each block has a `persistentID`. Your override file only needs the blocks you want to change, with only the properties you want to modify.

**Example** — Make Strikers have 999 health and add a custom enemy:

```json
{
  "Blocks": [
    { "persistentID": 13, "Health": { "HealthMax": 999 } },
    {
      "persistentID": 50000,
      "name": "Custom",
      "internalEnabled": true,
      "Health": { "HealthMax": 100 }
    }
  ]
}
```

- **Block 13** (Striker): Only `HealthMax` is changed. All other Striker properties (armor, name, etc.) are preserved.
- **Block 50000**: New block, appended to the array since no existing block has this ID.
- `LastPersistentID` is automatically recalculated.

### Modifying Arrays by Index (\_\_index)

For JSON files where array elements don't have a `persistentID` (such as files in `Custom/`), you can target specific array positions using `__index`. The `__index` property is stripped from the final output.

**Example** — Change the cost of the second spawn cost entry:

```json
{
  "SpawnCost": [{ "__index": 1, "Cost": 25 }]
}
```

This merges into the object at position 1 of the `SpawnCost` array, changing only `Cost` while preserving all other properties at that index.

### Appending to Arrays (\_\_existing)

To add items to an array while keeping the original contents, use the `"__existing"` string marker. It expands to the full original array at that position — items before it are prepended, items after are appended.

**Example** — Append a new enemy to an existing list:

```json
{
  "enemies": ["__existing", { "name": "CustomBoss", "hp": 500 }]
}
```

**Example** — Prepend and append:

```json
{
  "enemies": [{ "name": "First" }, "__existing", { "name": "Last" }]
}
```

If `"__existing"` is omitted, the array is replaced entirely (see below).

### Replacing Arrays

If your override array contains elements **without** `persistentID`, `__index`, or `__existing`, the entire target array is replaced.

```json
{
  "tags": ["new_tag_a", "new_tag_b"]
}
```

### Non-JSON Files

Non-JSON files (images, icons, etc.) are copied directly into the target directory, overwriting any existing file. New files with no matching target are also copied as-is.

### Merge Rules Summary

| Scenario                           | Behavior                                         |
| ---------------------------------- | ------------------------------------------------ |
| Object property exists in both     | Deep-merged recursively                          |
| Object property only in override   | Added                                            |
| Object property only in generated  | Preserved                                        |
| Array elements have `persistentID` | Matched by ID, deep-merged; new IDs appended     |
| Array elements have `__index`      | Merged at specified position; `__index` stripped |
| Array contains `__existing` marker | Original items placed at marker; new items around |
| Array elements have neither        | Entire array replaced                            |
| Scalar values                      | Override replaces generated                      |
| JSON file with no existing target  | Copied as new file                               |
| Non-JSON file                      | Copied with overwrite                            |

<hr>

See [Github](https://github.com/brgmnn/autogen-rundown) for more details.
