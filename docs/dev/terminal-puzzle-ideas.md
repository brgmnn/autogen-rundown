# Terminal Challenge Expansion — Escape Room Mechanics

Ideas for new terminal-based challenges that go beyond password locks and simple
command execution. All build on existing infrastructure: custom commands (5 unique
slots per terminal), warden events, log files, passwords, and timed mechanics.

Inspired by escape room design principles: information asymmetry, cross-referencing,
sequential discovery, red herrings, environmental clues, and time pressure.

---

## Challenge Catalog

### 1. Cross-Reference Dossier

**Concept**: A password-locked terminal guards a door. The password is an employee
ID that can only be determined by cross-referencing partial personnel records
scattered across 2-4 terminals in adjacent zones. One terminal has a duty roster
(names + IDs), another has shift assignments (names + dates + sectors), a third has
incident reports mentioning names. Only one employee satisfies all criteria stated
in the password hint.

**Player experience**: Rifling through filing cabinets in an abandoned office.
Players split up to different terminals, reading logs and relaying findings: "I've
got the shift roster — who was assigned to Sector 7G on November 14th?" Red herring
candidates make it non-trivial.

**Mechanics**:

- Password-protected terminal controlling a locked door
- Password hint describes the criteria: `"ENTER EMPLOYEE ID FOR SECTOR-7G SUPERVISOR ON SHIFT 2023-11-14"`
- Log files across 2-4 terminals contain overlapping personnel data
- Red herring employees match some but not all criteria
- Uses existing `PasswordProtected`, `GeneratePassword`, `PasswordPartCount`,
  `TerminalZoneSelectionDatas`

**Difficulty scaling**:

- A: 2 terminals, 2 logs, one obvious answer, no red herrings
- B: 3 terminals, 3-4 logs, one red herring employee
- C: 3-4 terminals across 2 zones, multiple red herrings, partial data corruption
- D: 4 terminals in fog/dark zones, heavily corrupted logs requiring inference
- E: 4+ terminals, some behind alarm doors, time pressure, heavy red herrings

**Zone layout**: Hub with 2-3 side branches (terminals), locked forward door.

**Implementation**: Extends `BuildChallenge_LockedTerminalPasswordInSide`. Needs a
procedural log content generator that creates coherent cross-referenceable records.
No Harmony patches required.

**Suitability**: Zone progression challenge.

---

### 2. Forensic Reconstruction

**Concept**: Fragmented, out-of-order security logs across multiple terminals
describe several incidents. Players must reconstruct which sector, timestamp, and
subject match a specific breach criteria to form the password. Most logs are
corrupted or partial — only careful cross-referencing eliminates false candidates.

**Player experience**: Piecing together a crime scene from scattered evidence.
Players read different logs, compare timestamps and details to eliminate
possibilities. "My log says Dr. Chen was transferred to Sector 7A at 0200, so the
0300 entry can't be her."

**Mechanics**:

- Password-protected terminal with multi-part password
- Password hint shows format: `"ENTER INCIDENT SEQUENCE: [SECTOR]-[TIME]-[SUBJECT]"`
- 4-8 log files across 3-5 terminals describe multiple plausible incidents
- Deliberately corrupted entries create ambiguity
- `PasswordPartCount` controls complexity

**Difficulty scaling**:

- A: 2 terminals, 2 logs, single clear incident, 1-part password
- B: 3 terminals, 3-4 logs, two plausible incidents
- C: 3-4 terminals, multiple red herrings, 2-part password
- D: 4 terminals in hazardous zones, heavily corrupted logs
- E: 5 terminals across alarm-gated zones, 3-part password, time pressure

**Zone layout**: Linear or branching with terminals spread across zones.

**Implementation**: Same password infrastructure as #1. Needs a narrative template
system for incident reports. No patches required.

**Suitability**: Zone progression challenge. Narrative quality makes it feel premium.

---

### 3. Cascading Lockdown Relay

**Concept**: A multi-terminal chain where each terminal can only unlock the next
terminal in the sequence. Each step triggers environmental hazards (lights off, fog,
enemy spawns) that increase urgency. Relay order is described in each terminal's
log files.

**Player experience**: Terminal A's screen reads `"RELAY NODE A — STATUS: ACTIVE"`.
Executing `RELAY_HANDSHAKE` triggers lights off in corridor B and unlocks Terminal
B. A player at Terminal B runs their relay command, which floods corridor C with fog
but unlocks Terminal C. Terminal C's command finally opens the forward door. Each
step makes the environment worse.

**Mechanics**:

- 3-4 terminals in separate branch zones from a hub
- Each has one `UniqueCommand` with `SpecialCommandRule = OnlyOnce`
- Each command's `CommandEvents` fire `UnlockSecurityDoor` for next zone + hazard
  events (`AllLightsOff`, `SetFogSettings`, `SpawnEnemyWave`) in other zones
- Branch zones start with `ProgressionPuzzleToEnter = Locked`
- Terminal logs describe the relay protocol and which node comes next

**Difficulty scaling**:

- A: 2 relays, mild effects (lights dim)
- B: 3 relays, lights off on step 2
- C: 3 relays, lights off + fog, small enemy wave on step 2
- D: 3-4 relays, escalating hazards, error alarm starts on final relay
- E: 4 relays, fog + darkness + enemy waves, countdown on first activation

**Zone layout**: Hub with 3-4 branches, each 1-2 zones deep. Locked forward door.

```
hub(locked_forward) -> end
hub -> branch_a(terminal_a)
hub -> branch_b(terminal_b)
hub -> branch_c(terminal_c)
```

**Implementation**: Pure event composition. Each `CustomTerminalCommand` with
`CommandEvents` containing door unlock + hazard events. Follows existing
`BuildChallenge_LockedTerminalDoor` and `EventBuilder` patterns. No patches needed.

**Suitability**: Zone progression challenge. Good for mid-to-late objective zones.

---

### 4. Quarantine Cascade

**Concept**: A control terminal can unseal 3-4 locked zones, but unsealing any zone
triggers hazards in others. Log files describe the containment relationships.
Players must plan the optimal release order to create a safe path. Every action has
consequences.

**Player experience**: Control terminal shows 4 zones on lockdown. Each has a
`RELEASE_SECTOR_X` command. Releasing Sector A causes fog in Sector B. Releasing B
spawns enemies in C. Players read the containment matrix logs and strategize: "If
we release A first, B gets fog. But the keycard is in B. So we release B first..."

**Mechanics**:

- Single terminal with 3-4 `UniqueCommand` entries (`RELEASE_SECTOR_A`, etc.),
  each `OnlyOnceDelete`
- Each command fires `UnlockSecurityDoor` for one zone AND hazard events in others
- Log files describe the containment matrix
- Objective item/terminal is in one of the quarantine zones
- Only one release order creates a manageable path

**Difficulty scaling**:

- A: 2 sectors, obvious optimal order, mild consequences
- B: 3 sectors, one clear optimal order, moderate consequences
- C: 3-4 sectors, multiple viable but one optimal, fog + enemies
- D: 4 sectors, complex interdependencies, heavy consequences, time limit
- E: 4-5 sectors, corrupted/missing logs, bosses and error alarms as consequences

**Zone layout**: Central control zone with locked branches.

```
control(terminal) -> sector_a(locked)
control -> sector_b(locked) -> objective_zone
control -> sector_c(locked)
control -> sector_d(locked)
```

**Implementation**: Each `CustomTerminalCommand` with `CommandEvents` containing
`UnlockSecurityDoor` + cross-zone hazards. Uses `ProgressionPuzzle.Locked` for
quarantine zones. Containment matrix log content is procedurally generated. No
patches needed.

**Suitability**: Zone progression challenge. Planning-then-execution structure is
distinct from combat-focused challenges.

---

### 5. System Purge Sequence (Simultaneous Activation)

**Concept**: Multiple terminals across the map must be activated within a tight time
window (10-30 seconds). Extends the existing TimedTerminalSequence concept with
simultaneous rather than sequential activation. A master terminal initiates the
countdown.

**Player experience**: Master terminal reads: `"PURGE PROTOCOL REQUIRES SIMULTANEOUS
ACTIVATION FROM 3 REMOTE NODES WITHIN 15 SECONDS."` Players spread to their
assigned terminals. "Everyone ready? 3, 2, 1, GO!" Voice coordination is essential.

**Mechanics**:

- Master terminal has `START_PURGE_SEQUENCE` (OnlyOnce) that starts a `Countdown`
  (AWO 10010)
- Each node terminal has `INITIATE_PURGE` (OnlyOnce) that fires
  `SetWorldEventCondition` with a unique condition index
- Countdown's `EventsOnDone` check all conditions — all set = door unlock,
  missing = punishment wave + reset
- Alternatively adapt existing `TimedTerminalSequence` infrastructure

**Difficulty scaling**:

- A: 2 terminals, 30-second window, adjacent zones
- B: 3 terminals, 20-second window, one zone behind alarm
- C: 3 terminals, 15-second window, mixed zone challenges
- D: 3-4 terminals, 12-second window, enemies in terminal zones, fog
- E: 4 terminals, 10-second window, fog/darkness/enemies, failure spawns big wave

**Zone layout**: Hub with branches to each node terminal.

```
start -> hub(master_terminal, locked_forward) -> end
         hub -> branch_a -> node_a(terminal)
         hub -> branch_b -> node_b(terminal)
         hub -> branch_c -> node_c(terminal)
```

**Implementation**: Uses `Countdown` + `SetWorldEventCondition` + conditional
events. May be able to reuse `TimedTerminalSequence` infrastructure. Needs testing
of condition-checking on countdown completion.

**Suitability**: Main objective or secondary objective. High coordination makes it
feel climactic.

---

### 6. Dead Man's Switch

**Concept**: A terminal starts a countdown. One player must periodically type
`RESET_CONTAINMENT` or catastrophe triggers (massive wave, fog flood). Meanwhile
the other 3 players push forward to complete a remote objective that permanently
disables the switch. One player is "chained" to the terminal.

**Player experience**: The terminal operator is alone, anxious, periodically typing
resets while hearing distant combat. The team pushes through a branch to find a
keycard/generator/terminal that disables the switch. Reunion is satisfying.

**Mechanics**:

- `ACTIVATE_CONTAINMENT` (OnlyOnce) starts `Countdown` + `EventLoop` for enemy
  spawns near the terminal
- `RESET_CONTAINMENT` (Normal rule) fires `AdjustAwoTimer` (AWO 20007) to reset
  the countdown
- Countdown's `EventsOnDone` trigger catastrophe
- Remote objective completion fires `StopEventLoop` + door unlock + cancel
  countdown

**Difficulty scaling**:

- A: 90-second countdown, no enemies at terminal, simple keycard fetch
- B: 75-second countdown, occasional enemies near terminal
- C: 60-second countdown, regular enemies, remote task has alarm door
- D: 45-second countdown, enemy waves at terminal, generator cell remote task
- E: 30-second countdown, heavy enemies, multi-step remote task, fog

**Zone layout**:

```
hub -> terminal_zone (dead man's switch)
hub -> branch_1 -> branch_2 -> objective_zone
hub -> locked_door -> end
```

**Implementation**: Uses `Countdown` (AWO 10010), `EventLoop` (AWO 20001), and
`AdjustAwoTimer` (AWO 20007). Needs testing of `AdjustAwoTimer` behavior with
countdown reset. May need Harmony patch if timer reset doesn't work as expected.

**Suitability**: Zone progression challenge. Creates memorable "holdout" moments.

---

### 7. Blind Operator

**Concept**: One player operates a terminal but cannot see the environmental changes
their commands cause in a remote zone. Other players stationed in the remote zone
observe effects and relay information to guide the operator's decisions. Classic
"one reads instructions, others do the task" escape room archetype.

**Player experience**: Terminal has 4-5 routing commands (`ROUTE_POWER_ALPHA`,
`ROUTE_POWER_BETA`, etc.) that change lights/fog in a remote zone. The operator
gets cryptic text feedback. Teammates in the remote zone: "Try Beta... no the
lights went red. Try Gamma... yes, fog is clearing!" Once correct configuration is
found, `COMMIT_CONFIGURATION` locks it in.

**Mechanics**:

- Terminal with 4-5 `UniqueCommand` entries (Normal rule, reusable)
- Each fires `LightsInZone`/`SetFogSettings` in a remote zone
- Correct choice sets `SetWorldEventCondition`
- `COMMIT_CONFIGURATION` checks condition — correct = door unlock, wrong =
  punishment wave
- Terminal `PostCommandOutputs` are deliberately ambiguous

**Difficulty scaling**:

- A: 3 options, Zone B visible from Zone A, obvious correct answer
- B: 4 options, zones separated, clearer environmental signals
- C: 4 options, incorrect choices spawn small enemy waves in Zone B
- D: 5 options, wrong choices cause fog/infection/spawns, countdown timer
- E: 5 options, escalating enemies, limited attempts before hard-lock

**Zone layout**:

```
start -> terminal_zone -> corridor -> observation_zone
                       -> locked_door -> end
```

**Implementation**: Uses `SetLightDataInZone` (AWO event) and `SetFogSettings` for
environmental changes. `SetWorldEventCondition` + `ConditionIndex` on events for
conditional branching. Needs testing of conditional event firing on the `COMMIT`
command.

**Suitability**: Zone progression or secondary objective. Asymmetric gameplay is
very memorable.

---

### 8. Terminal Siege (Active Defense)

**Concept**: A long-running terminal operation requires periodic `CONFIRM` inputs
every 15-30 seconds or progress stalls. Three players defend against waves while
one stays on the terminal typing. If the operator leaves to fight, progress halts.

**Player experience**: `EMERGENCY_BROADCAST` starts a progress bar. Every 20-30
seconds the terminal beeps: `"CONFIRM DATA INTEGRITY?"` — operator must type
`CONFIRM`. Meanwhile waves crash against the team. "Keep them off me, I need to
confirm!"

**Mechanics**:

- `EMERGENCY_BROADCAST` (OnlyOnce) starts a failure countdown + enemy `EventLoop`
- `CONFIRM_INTEGRITY` (Normal) fires `AdjustAwoTimer` to reset failure timer
- Separate progress countdown ticks toward completion
- `EventsOnProgress` at 25%/50%/75% escalate enemy spawns
- Completion unlocks door

**Difficulty scaling**:

- A: 90-second broadcast, confirmation every 30 seconds, light waves
- B: 120-second, every 25 seconds, medium waves
- C: 150-second, every 20 seconds, heavy waves with specials
- D: 180-second, every 15 seconds, boss at 75%
- E: 240-second, every 12 seconds, escalating everything, fog at 50%

**Zone layout**: Prep zone with resources -> arena with central terminal -> locked
forward door.

**Implementation**: Uses `Countdown` (AWO 10010) with `EventsOnProgress` and
`EventLoop` (AWO 20001) for enemies. `CONFIRM_INTEGRITY` fires `AdjustAwoTimer`.
Needs testing of dual countdown management (failure timer vs progress timer). May
need Harmony patch for pause/resume behavior.

**Suitability**: Main objective or zone progression. Combat-heavy, good climactic
zone challenge.

---

### 9. Ghost Signal Triangulation

**Concept**: Terminals have a `PING_SIGNAL` command returning signal strength based
on graph distance from a hidden target. Players ping from multiple terminals,
compare strengths, and deduce which zone contains the objective terminal.

**Player experience**: Terminal A: `"SIGNAL STRENGTH: 34% — SOURCE IS DISTANT"`.
Terminal B: `"SIGNAL STRENGTH: 78% — SOURCE IS NEAR"`. Terminal C: `"52% — MODERATE"`.
Players triangulate: "It's near Terminal B — search that area." Finding the target
terminal reveals the unlock command.

**Mechanics**:

- 4-6 terminals with `PING_SIGNAL` unique command
- Each command's `PostCommandOutputs` display procedurally generated signal
  strength based on zone graph distance to target
- Target zone contains a terminal with the objective command (door unlock)
- Signal strengths generated at build time from zone topology

**Difficulty scaling**:

- A: 3 terminals, very clear readings (10%/50%/90%), obvious target
- B: 4 terminals, closer signal strengths, 2 plausible zones
- C: 5 terminals, readings have "noise" (+/- random variation), target in side zone
- D: 5 terminals, some in dark/fog zones, higher noise, target behind locked door
- E: 6 terminals, heavy noise, some give false readings, enemies in target zone

**Zone layout**: Distributed zones with terminals at various distances from target.

```
start -> zone_a(terminal) -> zone_b(terminal) -> hub
                                                  hub -> side_c(terminal)
                                                  hub -> side_d(target_terminal)
                                                  hub -> zone_e(terminal)
```

**Implementation**: Build system calculates zone graph distances and generates
appropriate text in `PostCommandOutputs`. Target terminal uses
`CustomTerminalCommand` to unlock progression. No patches needed, but requires
zone graph distance calculation at build time.

**Suitability**: Zone progression or secondary objective. Exploration-focused, nice
change of pace from combat.

---

### 10. Warden's Riddle

**Concept**: A terminal poses questions whose answers must be found by exploring the
physical environment: zone designations, terminal serial numbers, log file contents.
Each answer must be typed as a password.

**Player experience**: Terminal: `"AUTHORIZATION QUERY: WHAT IS THE DESIGNATION OF
THE MAINTENANCE CORRIDOR IN SECTOR 7?"` Players explore, find the answer on
another terminal's `INFO` output or in a log file, and relay it back. "The sign
says SUBLEVEL MAINTENANCE BAY 3B."

**Mechanics**:

- Password system where password parts are zone aliases or terminal serial numbers
- `Zone.AliasPrefix` and `Lore.TerminalSerial` provide procedural vocabulary
- Password hint describes what to find: format, location hint, type of info
- Multiple queries for higher difficulty via `PasswordPartCount`

**Difficulty scaling**:

- A: 1 query, answer in same zone
- B: 2 queries, answers in adjacent zones
- C: 3 queries, some requiring cross-reference
- D: 3 queries, time limit, answers in dark/fog zones
- E: 4 queries, tight timer, wrong answers spawn enemies, answers behind alarms

**Zone layout**: Central terminal with branching exploration zones.

```
start -> terminal_zone -> locked_door -> end
         terminal_zone -> explore_a (logs/signs)
         terminal_zone -> explore_b (logs/signs)
```

**Implementation**: Uses password system with environment-aware query generation.
`Zone.AliasPrefix` and terminal serial numbers provide the answer vocabulary. No
patches needed, but the query generator needs access to actual zone layout data.

**Suitability**: Zone progression. Best early-to-mid in missions before heavy combat.

---

## Implementation Priority

### Phase 1 — Foundation (no patches, pure event composition)

1. **`LogContentGenerator`** class for procedural log content (personnel records,
   incident reports, containment matrices, signal readings). Shared by challenges
   1, 2, 4, and 9.
2. **Cascading Lockdown Relay (#3)** — simplest new challenge, direct event
   composition with existing patterns.
3. **Quarantine Cascade (#4)** — similar pattern, adds strategic planning element.

### Phase 2 — Information puzzles (log content generation)

4. **Cross-Reference Dossier (#1)** — extends password-in-side with rich log content.
5. **Forensic Reconstruction (#2)** — extends with narrative incident logs.

### Phase 3 — Exploration puzzles (build-time logic)

6. **Ghost Signal Triangulation (#9)** — needs zone graph distance calculation.
7. **Warden's Riddle (#10)** — needs environment-aware query generation.

### Phase 4 — Pressure challenges (AWO event testing required)

8. **Dead Man's Switch (#6)** — test `AdjustAwoTimer` first.
9. **Terminal Siege (#8)** — test dual countdown management.
10. **System Purge Sequence (#5)** — test simultaneous condition checking.
11. **Blind Operator (#7)** — test conditional event firing.

### AWO Events Needing Verification

Before Phase 4, test these Advanced Warden Objective events:

- `AdjustAwoTimer` (AWO 20007): Can it reset a countdown? Does it add/set time?
- `SetWorldEventCondition` (event 19): Can multiple conditions be checked simultaneously?
- `ConditionIndex` on event entries: Does conditional branching on command events work?
- `StartEventLoop`/`StopEventLoop` (AWO 20001/20002): Can they be cancelled from remote terminal commands?

---

## Key Files

- `AutogenRundown/src/DataBlocks/LevelLayout.ZoneBuildChallenge.cs` — new `BuildChallenge_*` methods
- `AutogenRundown/src/DataBlocks/LevelLayout.ZoneProgression.cs` — integrate into progression selection
- `AutogenRundown/src/DataBlocks/LevelLayout.*.cs` — add to per-objective SelectRun blocks
- `AutogenRundown/src/DataBlocks/Objectives/EventBuilder.cs` — event composition helpers
- `AutogenRundown/src/DataBlocks/Terminals/` — terminal placement data structures
- New: `AutogenRundown/src/DataBlocks/Content/LogContentGenerator.cs` — procedural log content
