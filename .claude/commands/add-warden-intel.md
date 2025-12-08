---
description: Generate new warden intel messages for elevator drops
args: <context> - Where/what objective these messages are for (e.g., "reactor startup", "fog zones", "terminal uplink", "mother boss")
---

# IMPORTANT: ONLY ADD NEW WARDEN INTEL MESSAGES

Your task is to generate NEW warden intel messages and add them to the appropriate location in the codebase. **DO NOT refactor, reorganize, or modify any existing code beyond adding the new intel messages.**

## Context

You are generating warden intel messages for: **{{args}}**

## Task Overview

Warden intel messages are flavor text snippets displayed to players during level loading. They capture chaotic voice transmissions from a previous team of 4 prisoners who attempted the mission, giving hints about what's coming.

## Format Requirements

Each intel message MUST follow this exact format:

- Must always have **Three lines**
- Each line **MUST** starts with `>...`
- Lines separated by `\r\n` (NOT just `\n`)
- **Exactly ONE section** must be in bold red: `<size=200%><color=red>TEXT HERE</color></size>`
- Include atmospheric sound effects/actions in brackets: `[gunfire]`, `[whispering]`, `[static]`, `[screaming]`, `[coughing]`, `[typing]` etc.
  - Use this sparingly, in most cases we want three lines of dialog
- The text should either be dialog from one of the four players, or a sound effect / action in the brackets.
- Capture tension, panic, urgency
- The red text should be the most important/dramatic part, or the part that want's to be emphasized


### Example Format

```csharp
">... [sound effect]\r\n>... This sounds really bad.\r\n>... <size=200%><color=red>They're here!</color></size>"
```

```csharp
">... <size=200%><color=red>I'm out of bullets!</color></size>\r\n>... There must be more somewhere.\r\n>... Keep searching!"
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

1. **Search for context**: Use Grep to find where `level.ElevatorDropWardenIntel` is used related to {{args}}
   - Search in `AutogenRundown/src/DataBlocks/` directory
   - Look for relevant objective or feature files

2. **Review existing examples**:
   - Read `kb/vanilla_warden_drop_intel.txt` for inspiration and tone
   - Look at existing custom messages in the codebase for style consistency

3. **Generate 5-8 NEW messages** that:
   - Relate specifically to {{args}}
   - Follow the exact format above
   - Capture the GTFO atmosphere: tense, scary, desperate, chaotic
   - Include relevant sound effects and actions
   - Have one dramatic red text section per message
   - Hint at what players will face without being too explicit

4. **Identify the exact location** to add them:
   - Find the appropriate method/function where this intel should be added
   - Show the file path and line number

5. **Add ONLY the new messages**:
   - Use the `#region Warden Intel Messages` and `#endregion` wrapper if it doesn't exist
   - Add the `level.ElevatorDropWardenIntel.Add(...)` block
   - **DO NOT modify any other code**
   - **DO NOT refactor existing messages**
   - **DO NOT reorganize the file structure**

## Reference Examples from Vanilla Game

Review `kb/vanilla_warden_drop_intel.txt` for tone and style. Key patterns:

- **Tension**: `>... [whispering] Slowly... Don't wake them up.`
- **Panic**: `>... <size=200%><color=red>INCOMING!!</color></size>\r\n>... [gunfire, commotion]`
- **Dark humor**: `>... Shoot me then, 'cause I'm not going in there.`
- **Desperation**: `>... I'm out of ammo.\r\n>... That was the last of it.\r\n<size=200%><color=red>>... [rustle, unsheathing knife]</color></size>`
- **Instructions**: `>... <size=200%><color=red>Check every box</color></size>, use whatever you find.`

## Quality Checklist

Before adding the messages, verify:

- [ ] Each message has exactly 3 lines
- [ ] Each line starts with `>...`
- [ ] Lines use `\r\n` as separator
- [ ] Exactly ONE `<size=200%><color=red>` section per message
- [ ] Include atmospheric [sound effects] or [actions]
- [ ] Messages relate to {{args}} context
- [ ] Tone matches vanilla GTFO (tense, dark, desperate)
- [ ] 5-8 messages generated
- [ ] Messages are added in the correct location
- [ ] NO OTHER CODE WAS MODIFIED

## Output

1. Show the file path where messages will be added
2. Show the complete code block to add
3. Confirm you are ONLY adding new messages and not modifying anything else
