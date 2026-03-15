# Terminal Puzzle & Challenge Ideas

Ideas for new terminal-based challenges that go beyond password locks and simple
command execution. All of these build on existing infrastructure: custom commands,
warden events, log files, passwords, and timed mechanics.

## 1. Terminal Relay Chain (Scavenger Hunt)

Each terminal gives a command name that must be executed on a *different* terminal.
Terminal A's logs say "Execute OVERRIDE_RELAY on TERMINAL_347", which unlocks
Terminal B's logs pointing to Terminal C, etc. Final terminal opens the objective
door. Wrong commands on wrong terminals spawn enemies.

**Implementation:** Custom commands with `OnlyOnce` rules, log files with clues,
warden events for both progression and punishment.

## 2. Cross-Reference Detective Work

Scatter log files across 3-4 terminals with overlapping information. One has
personnel records, another has access logs, a third has incident reports. Players
must cross-reference to deduce a password (e.g., "which employee was on shift
during the containment breach?"). The answer is the password to the objective
terminal.

**Implementation:** Log files + password-protected terminal with
`GeneratePassword = false` (hardcoded password derived from log content). Puzzle
data generated from the seed.

## 3. Terminal Triage / Diagnostics

A critical system is failing. A master terminal shows diagnostic errors
("COOLANT PUMP OFFLINE - Zone 42", "RELAY OVERLOAD - Zone 38"). Players must
split up and execute repair commands (`FLUSH_COOLANT`, `RESET_RELAY`) on the
correct terminals in the correct zones. Each fix triggers a warden intel update.
Completing all fixes unlocks progression. Each repair command also triggers a
localized enemy wave at that terminal's location -- forcing the team to defend
while someone types.

**Implementation:** Multiple custom commands across terminals,
`UpdateCustomSubObjective` events to track progress, `SpawnEnemyWave` on each
command execution.

## 4. Terminal Roulette

A zone has several terminals. One contains the real unlock command, the others
are compromised. Logs scattered earlier in the level hint at which terminal
serial number is safe. Executing the command on a compromised terminal triggers
an error alarm or fog flood. Executing on the correct one opens the door.

**Implementation:** Custom commands on all terminals in the zone. Safe terminal
triggers `OpenSecurityDoor`, compromised ones trigger `SpawnEnemyWave` or
`SetFogSetting`. Log files in earlier zones contain the clue (terminal serial
number or zone position).

## 5. Timed Broadcast Relay

A terminal initiates a "signal broadcast" that must be amplified by executing
commands on relay terminals within a time window. Similar to timed terminal
sequence but with a twist: each relay terminal is in a *different* zone, and the
order is randomized. Miss the window and the sequence resets with enemies. Forces
the team to split up and coordinate over comms.

**Implementation:** Extends existing timed terminal sequence pattern but with
larger zone spread and warden events for resets.

## 6. Terminal Lockout Protocol (Two-Key Auth)

A terminal requires simultaneous authentication -- two players must execute
commands on two different terminals within a short time window (e.g., 10 seconds
of each other). Simulates a "two-key" nuclear launch style security system.
Could gate high-value objectives.

**Implementation:** Two custom commands that each set a `WorldEventCondition`. A
warden event checks both conditions and opens the door. Conditions could expire
via timer, forcing coordination.

## 7. Corrupted Log Reconstruction

A terminal has a corrupted log file -- parts of the text are garbled/missing.
The missing fragments exist as log files on other terminals deeper in the level.
Players must read all fragments, mentally reconstruct the full message, and the
reconstructed message contains the password or command needed to proceed.
Essentially a reading comprehension puzzle under pressure.

**Implementation:** Log files with partial information + password-protected
terminal. The "puzzle" is entirely in the generated text content.

## 8. Terminal Network Ping Trace

Players must use the PING command (built into the game) to locate a hidden
terminal. The target terminal is in a zone they haven't entered yet, forcing them
to push deeper. Once found, the terminal requires a command that was displayed
briefly in the output of the ping sequence on the original terminal -- rewarding
players who pay attention to terminal output.

**Implementation:** Custom welcome text on the origin terminal that flashes a
code, plus a password-protected destination terminal.

## Top Picks

**#2 (Detective Work)**, **#3 (Diagnostics/Triage)**, and **#4 (Terminal
Roulette)** are the most promising -- they're distinct from existing mechanics,
create interesting team coordination moments, and are straightforward to
implement with the current custom command + warden event + log file
infrastructure.
