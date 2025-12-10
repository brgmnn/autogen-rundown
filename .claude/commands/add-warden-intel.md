---
description: Generate new warden intel messages for elevator drops
args: <context and location> - What the messages are about AND where to add them, AND optionally how many (e.g., "reactor startup in WardenObjective.Reactor.cs", "fog zones in LevelLayout.cs AddFogZone method", "terminal uplink", "mother boss fight in ZoneProgression.cs", "tank fight in LevelLayout.ZoneProgression.cs with 50 messages")
---

# IMPORTANT: ONLY ADD NEW WARDEN INTEL MESSAGES

Your task is to generate NEW warden intel messages and add them to the appropriate location in the codebase. **DO NOT refactor, reorganize, or modify any existing code beyond adding the new intel messages.**

## Context and Location

You are generating warden intel messages for: **{{args}}**

The args should specify:
1. **What the messages are about** - The context/objective/feature (e.g., "reactor startup", "fog zones", "security sensors")
2. **Where to add them** - File path, method name, or code location (e.g., "WardenObjective.Reactor.cs", "AddFogZone method", "LevelLayout.SecuritySensors.cs line 120")
3. **Optionally: how many messages to add** -

If location is not specified in args, search for the most appropriate location based on the context.

## Task Overview

Warden intel messages are flavor text snippets displayed to players during level loading. They capture chaotic voice transmissions from a previous team of 4 prisoners who attempted the mission, giving hints about what's coming. The messages should be dark, grim, gritty, and possibly a bit scary for the players.

## Format Requirements

Each intel message MUST follow this exact format:

- Must always have **Three lines**
- Each line **MUST** starts with `>...`
- Lines separated by `\r\n` (NOT just `\n`)
- **Exactly ONE section** must be in bold red: `<size=200%><color=red>TEXT HERE</color></size>`
- Include atmospheric sound effects/actions in brackets: `[gunfire]`, `[whispering]`, `[static]`, `[screaming]`, `[coughing]`, `[typing]` etc.
  - **Sound effects MUST be 1-2 words maximum** that describe actions or atmospheric sounds
  - Use verbs or onomatopoeia (e.g., `[gunfire]`, `[static]`, `[screaming]`, `[breathing]`, `[alarm blaring]`)
  - Do NOT use plain nouns (e.g., NOT `[metal]`, `[door]`, `[weapon]`)
  - Examples: `[gunfire]`, `[static crackling]`, `[alarm blaring]`, `[screaming]`
  - Use this sparingly, in most cases we want three lines of dialog
- The text should either be dialog from one of the four players, or a sound effect / action in the brackets.
- Capture tension, panic, urgency
- The red text should be the most important/dramatic part, or the part that want's to be emphasized
- Prefer spelling out numbers rather than writing as integers. E.g. "nine" instead of "9", or "fifteen" instead of "15"

### Avoid the following in messages

- Any messages covering a victory. e.g completing the level, completing an objective, successfully extracting
- Hopeful, positive, or upbeat messages.
- Avoid using specific zone / item numbers. Don't say "Zone 123", instead say "in the zone" or "the zone" or "that zone" etc.
- Never use the following words:
  - Kite
- Never reference the level tier
- Never reference the number of points of enemies
- Never reference the victory screen / failure screen. Or any of the out-of-game menus etc.
- Avoid using "Revive", instead say things like "get him back up"

### Example Format

```csharp
">... [static]\r\n>... This sounds really bad.\r\n>... <size=200%><color=red>They're here!</color></size>"
```

```csharp
">... <size=200%><color=red>I'm out of bullets!</color></size>\r\n>... There must be more somewhere.\r\n>... Keep searching!"
```

```csharp
">... [screaming]\r\n>... [gunfire]\r\n>... <size=200%><color=red>Fall back!</color></size>"
```

## Code Structure

The messages are added using this pattern:

```csharp
#region Warden Intel Messages
level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
{
    ">... Line one here.\r\n>... Line two here.\r\n>... <size=200%><color=red>Line three dramatic!</color></size>",
    ">... Another message.\r\n>... <size=200%><color=red>Red text can be anywhere!</color></size>\r\n>... Last line.",
    // 5-8 messages total
}))!);
#endregion
```

## Steps to Complete

1. **Identify the target location from args**:
  - Parse {{args}} to extract both the context (what it's about) and location (where to add)
  - If a specific file/method is mentioned in args, navigate to that location
  - If no location is specified, search for the most appropriate place:
    - Use Grep to find where `level.ElevatorDropWardenIntel` is used related to the context
    - Search in `AutogenRundown/src/DataBlocks/` directory
    - Look for relevant objective or feature files

2. **Review objective documentation in docs folder**:
  - **Check `docs/game/objectives/` for relevant objective information**
  - Read the relevant doc file to understand:
    - **Objective mechanics and phases**: What happens during the mission
    - **Common challenges**: What players struggle with (wave overwhelm, resource depletion, timer pressure, etc.)
    - **Core mechanics**: Specific game mechanics involved (verification codes, bioscans, alarms, etc.)
    - **Enemy types**: What enemies appear in this objective
    - **Atmosphere and tone**: The tension and situations players face
  - Use this information to make intel messages contextually accurate and atmospheric

3. **Review the target location**:
  - Read the file and understand the surrounding code
  - Look for existing `#region Warden Intel Messages` blocks nearby
    - If it exists, look for comments like `// add warden intel here`, `// TODO: add warden intel`, or `// claude add intel here` and ONLY ADD THE WARDEN INTEL MESSAGES THERE.
  - Understand what the method/function does to ensure messages are contextually appropriate
  - If adding to a WardenObjective file and there's no existing input, usually it should be added at the end of the `Build_*` method.

4. **Review existing examples for style**:
  - Read `kb/vanilla_warden_drop_intel.txt` for inspiration and tone
  - Look at existing custom messages in the codebase for style consistency
  - Match the tone and theme of the context

5. **Generate the 200 (or specified) new messages** that:
  - Relate specifically to the context from {{args}}
  - **Incorporate details from the objective docs** (if applicable):
    - Reference specific mechanics (verification codes, bioscans, wave timers, fog, etc.)
    - Mention specific challenges (timer pressure, code retrieval, resource depletion, etc.)
    - Include relevant enemy types (Giants, Chargers, Mothers, Tanks, etc.)
    - Capture the specific atmosphere of that objective type
  - Follow the exact format above
  - Capture the GTFO atmosphere: tense, scary, desperate, chaotic
  - Include relevant sound effects and actions
  - Have one dramatic red text section per message
  - Hint at what players will face without being too explicit

6. **Add ONLY the new messages** at the specified location:
  - Use the `#region Warden Intel Messages` and `#endregion` wrapper if it doesn't exist
  - Add the `level.ElevatorDropWardenIntel.Add(...)` block
  - Place it in the appropriate location within the method/function specified in args
  - Show the exact line number where it was added
  - **DO NOT modify any other code**
  - **DO NOT refactor existing messages**
  - **DO NOT reorganize the file structure**

## Reference Examples from Vanilla Game

Review `kb/vanilla_warden_drop_intel.txt` for tone and style. Key patterns:

- **Tension**: `>... [whispering] Slowly... Don't wake them up.`
- **Panic**: `>... <size=200%><color=red>INCOMING!!</color></size>\r\n>... [gunfire]`
- **Dark humor**: `>... Shoot me then, 'cause I'm not going in there.`
- **Desperation**: `>... I'm out of ammo.\r\n>... That was the last of it.\r\n<size=200%><color=red>>... [clattering]</color></size>`
- **Instructions**: `>... <size=200%><color=red>Check every box</color></size>, use whatever you find.`

## Incorporating Objective Documentation

When generating messages, use the `docs/game/objectives/` files to add specific, contextually accurate details:

**Reactor Startup Example** (from `reactor-startup.md`):
- References waves, verification codes, timer pressure
- `>... Wave four incoming!\r\n>... <size=200%><color=red>Where's the verification code?!</color></size>\r\n>... [frantic typing]`
- `>... The verification timer!\r\n>... <size=200%><color=red>We're out of time!</color></size>\r\n>... [alarm blaring]`

**Terminal Uplink Example** (from `terminal-uplink.md`):
- References IP addresses, UPLINK commands, alarm triggers
- `>... UPLINK_CONNECT entered.\r\n>... <size=200%><color=red>Alarm's triggered!</color></size>\r\n>... [klaxon blaring]`
- `>... Code's not matching!\r\n>... X04... where's X04?!\r\n>... <size=200%><color=red>Stage reset!</color></size>`

**Survival Example** (from `survival.md`):
- References timer countdown, position defense, environmental changes
- `>... Thirty seconds left!\r\n>... <size=200%><color=red>Hold the position!</color></size>\r\n>... [gunfire intensifies]`
- `>... Security doors opening!\r\n>... New spawn points!\r\n>... <size=200%><color=red>Reposition!</color></size>`

These examples show how objective-specific details make messages more immersive and contextually appropriate.

## Quality Checklist

Before adding the messages, verify:

- [ ] Args parsed correctly for both context and location
- [ ] Relevant objective docs reviewed (if applicable) from `docs/game/objectives/`
- [ ] Messages incorporate specific details from objective docs (mechanics, challenges, enemies)
- [ ] Target file and method/location identified correctly from args
- [ ] Each message has exactly 3 lines
- [ ] Each line starts with `>...`
- [ ] Lines use `\r\n` as separator
- [ ] Exactly ONE `<size=200%><color=red>` section per message
- [ ] Include atmospheric [sound effects] or [actions] (1-2 words max, describing actions/sounds, not plain nouns)
- [ ] Messages relate to the context from {{args}}
- [ ] Messages are thematically appropriate for the target location
- [ ] Tone matches vanilla GTFO (tense, dark, desperate)
- [ ] The right number of messages are entered. If not specified in the arguments default to 200 individual messages.
- [ ] Messages are added in the location specified in {{args}}
- [ ] NO OTHER CODE WAS MODIFIED

## Output

1. Confirm the context and location parsed from args
2. Note which objective doc (if any) was reviewed from `docs/game/objectives/`
3. Show the exact file path and line number where messages will be added
4. Show the complete code block to add (with proper indentation)
5. Confirm you are ONLY adding new messages and not modifying anything else
