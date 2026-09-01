# E-Tier Difficulty — Investigation & Proposals

## Problem statement

Players consistently report that E-tier levels feel like D-tier levels. The README already
records this (`README.md:28` "Make E-tier shorter but harder", `README.md:38` "E-tier valiant
way too easy").

Hard constraints on any fix:

1. **Never make levels longer.** More zones, more objectives, and higher-class alarms all add
   *time*, not challenge. Length is explicitly not a difficulty lever.
2. **Always completable.** GTFO is brutal but fair — every level must be beatable.
3. **Prefer novel interactions and level design** over raw stat inflation.
4. **Resources: parity only.** E must not be better-supplied than D (it currently is), but we
   never push below the D baseline. Difficulty comes from composition and mechanics, not
   starvation.

## Diagnosis: why E feels like D

The systems that dominate moment-to-moment feel — *which enemies are in the level* and
*how many* — are nearly identical between D and E:

| System | D | E | Verdict |
|---|---|---|---|
| Hibernation roster (`EnemyPopulation.cs:301-392` vs `:394-489`) | full list | **byte-for-byte identical** | zero delta |
| Hibernation group shapes (`EnemyGroup.cs:1133` vs `:1199`) | Shadows MaxScore 8 | Shadows MaxScore **4** | **E is easier** |
| Scout pack (`LevelLayout.cs:457` vs `:492`) | chance 0.3, uncapped, 23 entries | identical | zero delta |
| Enemy points per zone (`BuildDirector.cs:291-292`) | `Between(30,35)` | `Between(35,40)` | +15% |
| Ammo packs per zone (`Zone.cs:1332-1337`) | 5 | **6** | **E is easier** |
| Group-mix selection weights (`LevelLayout.cs:686`, `BuildStandardChoices`) | modifier-driven, tier-blind | tier-blind (only shadows ×1.2, `LevelLayout.cs:656-660`) | ~zero |
| Level modifier rolls (`LevelSettings.cs:490`) | — | ~+10pp per "has X" roll | small |
| Objective pool (`RundownFactory.cs:381-435`) | shared pool | same shared pool; D and E loops are copy-paste | zero delta |

The one clear D→E step-up is **wave settings composition** (`WaveSettings.BuildPack()`):
D draws 54% `Baseline_Hard` / 36% `Baseline_VeryHard` / 10% `MiniBoss_Hard`; E draws
18% / 72% / 10%. (`Baseline_VeryHard` = 25→30 pts with a 45s ramp vs Hard's 18→28 pts /
100s ramp.) Alarm waves at E genuinely hit harder — but everything *between* alarms plays
like D.

### Genuine but marginal E edges (for fairness)

These exist, but are whiskers, not a tier:

- Boss roll per zone 0.25 vs 0.20; boss pack cap 4 vs 2 (`LevelLayout.cs:529-534`,
  `LevelSettings.cs:337-343`). On ~6 zones that's ~1.5 expected boss rolls.
- Aligned-boss options: D has one (PMother); E has three, including Tank+TankPotato in the
  dark with 50 corpses (`LevelLayout.ZoneProgression.cs:995-1153`).
- Blood doors: chance 0.15→0.20, cap 3→uncapped (never binds), composition step-ups
  (PMother 0.20, Tank_x2 0.15) (`LevelLayout.cs:148-158`, `:201-226`).
- Error alarms: E rolls `{0.15→0, 0.50→1, 0.35→2}` at `Hard|VeryHard` vs D's `Flip(0.3)` at
  `Normal|Hard`; `MaxErrorAlarms` 5 vs 3 (`LevelLayout.cs:782-830`, `LevelSettings.cs`).
- Alarm class pack shifts ~+9pp toward Class VI/VII, and scan geometry stretches
  (components ×1.10-1.6, start-distance bumps) (`ChainedPuzzle.cs:268-436`,
  `Zone.cs:361-437`).
- Alarm modifier chances roughly double (LightsOff 0.12→0.25 etc., `Zone.cs:440-611`) —
  but they roll *independently*, so most E alarms still have no twist.
- Global live-enemy soft cap 30→35 (`Level.cs:748-749`); scout waves use `Scout_VeryHard`
  payloads.

**Summary:** E = D + hotter alarm waves + a minority of spicier rolls. The baseline zone
experience — the roster you sneak past, the packs you clear, the ammo you find — is D.
Two knobs are actually *inverted* (shadow group size, ammo).

## What official E-levels do

All six official E-tiers are documented in `docs/game/rundowns/`. One-line signatures:

- **R2E1 Crib** — infectious fog + Shadows/Big Shadows + surge alarms + rolling blackouts +
  a permanent error alarm that scales for the rest of the expedition.
- **R4E1 Downwards** — error alarm spawns a Tank every ~4 minutes (uncapped); fog turns
  infectious after objective progress; one disinfection pack in the whole sector.
- **R5E1 KDS Deep** — players *start* at 100% infection (health capped ~15%); scouts spawn
  exclusively Giants/Hybrids; door ambushes; `DEACTIVATE_ALARMS` terminal counterplay.
- **R7E1 Chaos** — The Immortal (invincible Tank) stalks the team all level; flesh walls that
  enemies pass through but players can't.
- **R8E1 Valiant** — fixed countdown timeline that unlocks doors and spawns waves on a
  schedule; opting into the secondary adds enemy types level-wide.
- **R8E2 Release** — a surge error held at bay only by repeatedly running
  `ADMIN_TEMP_OVERRIDE` at terminals; pure-Nightmare and pure-Shadow Class VI alarms;
  corrupt scans with lights toggling; 22 of 28 enemy types in one level.

**The thesis: official E-levels are hard through *stacked, interacting pressures* and
*composition*, never through length.** Fog × infection × darkness × a persistent threat ×
an upkeep mechanic — each individually manageable, together defining. Several E-levels are
*shorter* than the D-levels in the same rundown.

## Proposals

### Group A — Parity fixes (cheap, immediate, length-neutral)

**Status (2026-08): A1-A4 shipped for 1.1.0** as one unit (playtest pending). **A5 and A6
rejected** — maintainer decision: both are raw enemy-count ramps, which is not the desired
difficulty lever, especially alongside A4's ammo reduction. Composition over density.

| # | Fix | Site | Change | Risk |
|---|---|---|---|---|
| A1 ✅ | Un-invert shadow clumping. E shadow packs were *half* the size of D's. | `EnemyGroup.cs:1199` | E Shadows `MaxScore = 4` → **8** (D parity; 10 remains as tuning headroom) | Near zero — bug fix |
| A2 ✅ | Differentiate the E roster. Same enemies, heavier mix; makes the Tier-E comment ("All enemies are available") true in practice. | `EnemyPopulation.cs:398-476` | ShooterGiant 0.3→0.5, ChargerGiant 0.4→0.6, ShadowGiant 0.4→0.6, NightmareGiant 0.2→0.35 | Low — giants drain ammo; pairs with A4 |
| A3 ✅ | E scout *composition*, not count. More scouts = slower, not harder; nastier scouts = harder. R5E1 precedent. | `LevelLayout.cs:492-525` | Chance stays 0.3, uncapped, 23 entries; three plain `Scout` entries (5/5/10) swapped for `ScoutShadow` @10+@15 and `ScoutNightmare` @10 | Low — scout points already substitute for hibernation points |
| A4 ✅ | Ammo parity. E was the best-supplied tier in the mod. | `Zone.cs:1332-1337` | `"E" => 6` → `5` (= D). Never below D (decision above). The BossAlarm/Stalker `+1` signature bump is untouched. Health (5) / Tool (3.5) stay tier-blind. | Medium *only* in combination with A2 — playtest together |
| A5 ❌ | ~~Widen the points gap~~ **Rejected** — pure density ramp. | `BuildDirector.cs:292` | not doing | — |
| A6 ❌ | ~~Tier-scale the group-mix table~~ **Rejected** — pure density ramp. | `LevelLayout.cs:686` | not doing | — |

### Group B — Turn the existing machinery on

Mechanisms that already exist in the codebase but are dormant or never combined. Each is
E-scoped, length-neutral, and lists its completability guardrail.

**B1. Implement `AddScriptedErrorAlarm`. ✅ Done** (now in
`Extensions/WardenObjectiveEventCollections.cs`; the empty `EventBuilder` stub is removed).
An R7D1-style pseudo-error — periodic small waves via an `EventLoop`, no combat
music, no `DEACTIVATE_ALARMS`, stamina regen between pulses. This is the R2E1/R4E1
signature: persistent upkeep pressure over the level's *existing* footprint.
*Guardrail:* interval ≥ 3-4 min, payloads ≤ 4 pts or a single enemy. R7D1 uses a finite
count (19); the helper's default is now `-1` (infinite), which is the Stalker's design.
When a signature that adds its own wave or environment pressure is active (Stalker,
BossAlarm, CyclingFog, UpkeepProtocol — *not* StartWithInfection, which is a static
handicap), `AddErrorAlarm` steps `Error_VeryHard` down to `Error_Hard` so the two
pressure sources don't stack.

**B2. Starting-state handicaps (`Level.cs:678-690`). ✅ Done** — shipped as the
`LevelSignature.StartWithInfection` signature (see C2c) rather than a standalone roll,
and harder than proposed here: usually max infection, no added relief. The remaining
starting-state levers (`StartingHealth` / `StartingMainAmmo` / `StartingSpecialAmmo` /
`StartingTool`) are still plumbed and unused.

**B3. An E-specific `GlobalWaveSettings` preset (`Level.cs:748-749`).**
Today only the soft cost cap varies (D 30 / E 35). Add a fourth preset (PersistentId 4):
MaxCost 35 + specials `BaseWeights` 0.5→0.65 + `WaveEnemyTypeLimits` specials 1→2. Same
wave cost and duration — but E waves *feel* different mid-wave (more specials mixed in).
*Guardrail:* leave boss weights (indices 3-4) alone; those change wave lethality
nonlinearly.

**B4. Tier-varied `ExpeditionBalance` (`Level.cs:546`).**
One global preset exists today. Add an E preset with exactly two deltas:
`StaticEnemiesMaxPerZone` 10→14 (+1 on the per-area-size caps) so A5's density actually
lands, and `WeakDoor4x4Health` 7→5 so weak doors fail faster under B1-style pressure — no
free fortresses, without touching alarms.
*Guardrail:* do **not** touch resource fractions here (double-dips with A4).

**B5. Guaranteed alarm-modifier stacking at E (`Zone.cs:557-610`).**
The modifiers (LightsOff 0.25, CyclingLights 0.12, FogFlood 0.11, SecuritySensors 0.10,
mid-scan hybrid wave) roll independently — most E alarms still roll clean. Change: for E
alarms above a class threshold, draw **one guaranteed modifier** from a weighted tuple
list, then keep the existing independent rolls for a possible second. Every serious E
alarm has a twist; ~30% have two.
*Guardrail:* encode exclusions — never FogFlood + LightsOff together (zero-visibility scan
is the known over-tuning failure mode).

**B6. Respawn zones as backtrack pressure (`SetRespawnVibe`, `ZoneProgression.cs:94`).**
Per-zone `EnemyRespawning` knobs (`Zone.cs:1205-1243`) are used exactly once in the whole
generator. Add a finalize-time pass (in the main `LevelLayout.cs` partial, per convention)
that at E marks 1-2 non-objective zones with respawn interval 45-60s and
`EnemyRespawnCountMultiplier` ≤ 1.0. The same corridor is dangerous *twice* — punishes
slow play without adding geometry. This is the official-game cocoon pattern (R5D2, R8D1,
R8E2).
*Guardrail:* never the elevator zone or the extraction zone; keep the scout exclude list;
the long interval means a competent team simply outruns it.

**B7. Blood-door chance actually using the uncapped max (`LevelLayout.cs:148-158`).**
E is `(-1, 0.20, 0.80)` — uncapped but at 0.20/zone the cap never binds. Raise E chance to
0.30. The E door/area packs already step up (PMother, Tank_x2, Pouncer_x3); this makes
them appear.
*Guardrail:* self-balancing — blood-door zones already get ×0.66 hibernation points and
halved scout chance. Length-neutral: blood doors gate doors that already exist.

**Deferred (good, but each deserves its own pass):**
- Scout-scream rescripting via `EventsOnScoutScream` + `SuppressVanillaScoutWave`
  (R5E1's "scouts spawn only Giants" is one config away) — interacts with alarm placement.
- `SecurityGate.FreePassage` ambush zones (doorless boundaries: no chokepoint, no c-foam
  anchor) — can only replace doors that carry no alarm/chained puzzle, which constrains
  placement.

### Group C — Signature mechanics (max one per E level)

Official E-levels each have an *identity* — one defining mechanic you remember. Gate these
behind a generic `LevelSignature` enum on `LevelSettings`, rolled only in the E branch of
`Generate()`; at most one signature per level.

**Status (2026-08, updated for 1.1.0):** `None` is back in the roll at weight 0.7
(`b847a65` — a small chance of a signature-less E level); current weights are None 0.7 /
StartWithInfection 1.0 / Stalker 1.0 / CyclingFog 1.0 / BossAlarm 1.0 (raised from 0.6).
UpkeepProtocol is **disabled** — implemented but commented out of the roll
(`LevelSettings.cs:514`, `a39cb0b`) pending its Windows playtest.
Levels whose Main objective is Survival / ReachKdsDeep / Cryptomnesia are demoted back to
`None` in `Level.Build` (with a fog-modifier re-roll for CyclingFog) so the level-wide
signature consumers — the B1 error damp, the apex/ClearPath boss suppression, the
per-zone ammo bump — don't fire on a level with no signature content. Two **re-roll**
rules (both to Stalker or StartWithInfection, preserving 100% incidence) run after the
objective prebuild: BossAlarm re-rolls when *any* bulkhead carries AlphaTerminalCommand,
TimedTerminalSequence, TerminalUplink, or CorruptedTerminalUplink — all of them can fire
an identifier-less global wave stop mid-level (TTS's `DEACTIVATE_ALARMS` turn-off zone
exists on secondaries too, and the uplink-completion stop can fall through
`Patch_UplinkWaveIsolation` to the vanilla global call), which would silently kill the
untagged boss stream; UpkeepProtocol re-rolls when *any* bulkhead carries ReactorStartup,
ReactorShutdown, or TimedTerminalSequence — long stationary phases (and a terminal-less
reactor zone) starve its override economy. Note the reactors and TTS use the
interaction-layer progress bar, *not* the countdown widget — the exclusion is economic,
not a HUD conflict.

**C1. Upkeep protocol** *(R8E2; the maintainer's own idea, `README.md:71-72`)* **✅ Done,
currently disabled** (`LevelSignature.UpkeepProtocol`, commented out of the roll in
`a39cb0b` pending Windows playtest; applied in `Level.ApplyUpkeepProtocol()`,
`Level.UpkeepProtocol.cs`)
The level starts an AWO countdown at drop (initial = first Main zone's
`GetClearTimeEstimate()` + 60s grace, 5s after landing) and **every terminal in every
Reality zone** (all bulkheads) carries a one-use `ADMIN_TEMP_OVERRIDE` command
(`CommandRule.OnlyOnceDelete`) whose `CommandEvents` fire `AddAdjustTimer` for its own
zone's clear estimate ×1.2 — the margin funds objective dwell (uplinks, HSU extract,
Alpha transfer). The whole budget ≈ the level's expected clear time: the ritual is the
pressure, not added length. Expiry does **not** fail the level — the
`AddCountdownWithExpiryChain` helper pre-builds 8 fallback windows of 240s: each expiry
scope-stops any prior stream, spawns a fresh `GenericWave.UpkeepSurge` — a true surge of
nightmares (`WaveSettings.Surge` + `OnlyNightmares`, R8E2's surge error; tagged
`"upkeep_surge"`), survivable because it is player-terminated, not cleared — and re-arms
the next window; the override's scoped stop silences the surge. The chain is pre-built
because AWO runs one global countdown and starting a new one drops the old
`EventsOnDone`. Once every terminal is spent (typically extraction) the surge runs to
level end — R8E2's sprint finish, by choice. Warnings fire at 75%/90% elapsed.
Applied in the `Level.Build` finalize phase (clear estimates need every bulkhead's
`FinalizeLayout`). Interactions: joins the B1 damp; auto-excluded from the HSU/GSI
drop-time alarms (allow-list); no ammo bump (conditional pressure, CyclingFog precedent);
error alarms with turn-off terminals stay rollable but their persistent `CustomHudText`
banner becomes a one-shot fly-in message — `CustomHudText` writes the countdown's HUD
widget and silently kills a running countdown. Tagged surge waves survive
Alpha/TTS/uplink global stops (identifier-less stops only kill untagged waves), so only
the economy re-roll above is needed. Known limits (accepted): checkpoint restore aborts
the countdown (AWO, Survival parity); same-frame double overrides lose one grant;
warnings are one-shot per countdown. Zones with deliberately stripped terminals (reactor,
hill closets) get no command. Tuning order if it overshoots: roll weight → `UpkeepSurge`
settings (`Surge`→`Surge_Easy`) → fallback window → grant factor.
Needs Windows playtest — including a KingOfTheHill secondary seed (300s holdout vs the
economy).

**C2. Recurring stalker** *(R7E1-lite)* **✅ Done** (first `LevelSignature`; weight 1.0,
applied in `LevelLayout.ApplyLevelSignature()`)
B1's pseudo-error with a `GenericWave.SinglePouncerShadow` payload every 230-270 s (grace
35-70 s) plus a `Sound.EnemyHeartbeat` tell — something hunts you all level. Explicitly
**not** invincible (see What NOT to do): pouncers down players rather than kill, and they
die. *Guardrail:* payload never escalates; single-enemy pulses only. **Deliberately
infinite** (`waveCount: -1`, tightened in `df2ef09`): the loop is immune to
`StopAllWavesBeforeGotoWin` and keeps hunting through the exit scan — that extraction
pressure is the design. Demoted on Survival / ReachKdsDeep / Cryptomnesia mains.

**C2b. Boss alarm** *(R4E1's Tank error)* **✅ Done** (`LevelSignature.BossAlarm`, weight
1.0 — raised from 0.6 in `b847a65`)
A real `TriggerAlarm` boss error wave (Tank @240s / TankPotato @180s / Mother @240s,
weighted) in `WavesOnElevatorLand` — the game starts the alarm ambience at drop — running
until the Main objective completes, where `StopAllWavesBeforeGotoWin` cancels all waves
and the ambience before exit waves spawn. The elevator warden intel was later removed as
too noisy (`a56f466`); only the drop-screen `MarkAsBossErrorAlarm` intel remains. Rolled
error alarms keep the one-notch damp; their `DEACTIVATE_ALARMS` stops are
identifier-scoped (honored by AWO) and the boss stream carries no identifier, so it
survives them. The apex-alarm boss default is suppressed while the signature is active
(ClearPath's own level tank alarm branch was removed outright — dead code once every E
level rolls a signature). Re-rolled in `Level.Build` (to Stalker or StartWithInfection)
when any bulkhead carries a global-wave-stop objective — see the Status note above; the
HSU/GSI E-Main drop-time error alarms are an allow-list (`None` or `StartWithInfection`
only) so drop pressure never double-stacks with any wave-pressure signature.

**C2c. Infected start** *(R5E1)* **✅ Done** (`LevelSignature.StartWithInfection`, weight
1.0)
Players spawn at max infection (hard-coded 1.0 at E; the 1.0/0.75/0.5 weighted draw
survives only as the non-E fallback) via
`SpecialOverrideData.InfectionLevelAtExpeditionStart`. The engine soft-caps infection at
0.85 — a max start settles there in ~15s, leaving a 15% health floor exactly like R5E1 —
and applies the value only at the initial elevator spawn (checkpoint recall keeps captured
infection). The signature forces at least `LevelModifiers.Infection` so the level reads as
infected. The standing relief rolls are fog-gated and **cannot fire on a NoFog roll**
(~30% of these levels), so relief is a dedicated low roll instead: a 0.35 chance of a
disinfection side zone restricted to the **second half** of the Main progression — players
play through the infection before any relief appears. No relief in the first half, ever.
Keeps the full-strength `Error_VeryHard` error alarms (the B1 damp only applies to
wave/environment-pressure signatures) and is the one signature that still allows the
HSU/GSI drop-time error alarms.

**C2d. Cycling fog** **✅ Done** (`LevelSignature.CyclingFog`, weight 1.0)
Whole-level cycling fog: the ventilation fails on a cycle — fog rises to heavy, holds,
recedes, repeats for the entire level via the existing `AddCyclingFog(level)` EventLoop
helper on `EventsOnElevatorLand` (E cadence: infectious ≈177s cycle with a 90s heavy
hold, non-infectious ≈140s; `startDelay` gives a 45-90s clear grace at drop).
Infectious iff the level rolls `FogIsInfectious` (the infectious cycle fogs use
`INFECTION_SLOW` = 0.015 — an earlier 0.01 sat exactly on the `IsInfectious` threshold
and classified as non-infectious). The signature owns fog end to end: the
E fog modifier roll is skipped (keeping `Zone.RollFog` inert), `Level.Build` sets the
base `FogSettings` to the cycle's clear trough and reserves `FogUsage.LongDuration` so
fog-flood alarms and objective fog challenges can't stack on top. CyclingFog levels
exclude CentralGeneratorCluster from the Main-objective draw entirely (RundownFactory
constructs the Level before drawing the objective and uses the excluded-predicate
`DrawSelect` overload — CGC's generator fog steps would fight the loop; CGC's layout now
also self-defends by skipping its fog steps when its `TrySetFogUsage` reservation fails);
Survival / ReachKdsDeep / Cryptomnesia demote to None with a normal fog re-roll.
Guardrails: a fog turbine + repellers guaranteed in the elevator zone, plus 0.5-chance
rolls for an extra turbine and 1-2 repeller drops in the **second half** of the level
(blind heavy fog is the pain point); the B1 one-notch error damp applies. Because the
forced NoFog modifier means the fog-gated relief can never fire, infectious cycling levels
share StartWithInfection's dedicated relief roll: a 0.35 chance of a disinfection side
zone, second half only — never earlier.
Needs Windows playtest: AWO loop first-iteration timing (drop should be clear until the
grace elapses).

**C3. Lights-out travel scan** *(maintainer's own idea, `README.md:75`)*
A sustained travel scan (reverse-on-exit already implemented in
`Patch_SustainedTravelReverse.cs`) that fires `AddAllLightsOff` on scan start, restoring
light near completion via `AddSetZoneLights` / `AddRevertZoneLights`.
*Effort:* medium (event wiring on the scan's existing event surface).
*Guardrail:* **static scans only** (`README.md:41`: big moving scans are already too hard);
no wave attached — the darkness *is* the difficulty; guarantee glowsticks via
`ConsumableDistributionInZone` in the preceding zone.

**C4. Pure-population alarms** *(R8E2's pure-Shadow / pure-Nightmare Class VIs)*
`WavePopulation.OnlyShadows` / `OnlyNightmares` on **normal-class** E alarms, following the
proven `OnlyInfectedHybrids` pattern. Same class, same scan duration, same wave cost —
composition alone is the difficulty. This is the purest expression of "harder, not longer."
*Effort:* low. *Guardrail:* same wave settings the alarm would otherwise roll; never
combine pure-Shadows with the LightsOff modifier.

**C5. Trapped doors** *(R5E1 ambushes + the README "bait" idea, `README.md:29-30`)*
Expand the existing E open-door pouncer roll (`Zone.cs:593-596`, currently 0.02) into a
real trap table: door opens → `AddAlertEnemies` (AWO 10017) wakes the next zone's sleepers,
or a 30s lights-cut, or a delayed spawn behind the team. Doors stop being unconditionally
safe transitions.
*Effort:* low — every primitive is on the existing per-door event surfaces.
*Guardrail:* only on zones with ordinary hibernation loads; cap 2 trapped doors per level;
a warden-intel tell after triggering so it's learnable, not arbitrary.

**Dropped (and why):**
- *The Immortal / flesh walls* — invincible persistent enemies risk soft-locks, and the
  required patching sits in known il2cpp ICF-fold trap territory.

(100% starting infection was originally dropped here over relief concerns, but was later
shipped as C2c — the maintainer chose to rely on the standing relief rolls only.)

## What NOT to do

1. **No length levers.** No extra zones, no extra bulkheads (do not "fix" the tier-blind
   `bulkheadChances` table at `RundownFactory.cs:560-566` — a short E Main is a *feature*),
   no added objectives, no higher alarm classes as a primary lever, no longer scans
   (`README.md:100`: "Reduce T-scan duration" is a standing community request).
2. **No scan-density or moving-scan escalation.** `README.md:41,45` are our own
   post-mortems ("Big security scans that move are too hard"; "Tone down higher density of
   scans"), and the CHANGELOG shows repeated alarm walk-backs.
3. **No raw HP/damage inflation** on `EnemyBalancing`. Composition, not spreadsheets.
4. **No resources below the D baseline.** Parity (A4), not austerity.
5. **No invincible persistent enemies.** Completability hazard + il2cpp ICF patch risk.
6. **No blind stacking.** The two known unplayable combos — FogFlood+LightsOff and
   pure-Shadows+LightsOff — must be encoded as exclusions, not left to low probability.

## Rollout & tuning strategy

- **Phase 1 — Parity:** ✅ A1-A4 shipped for 1.1.0 as one unit (A5/A6 rejected — no raw
  density ramps). Expected outcome: E measurably above D with zero new mechanics.
  Playtest gate below still applies before further tuning.
- **Phase 2 — Machinery:** B1, B3, B4, B5 first (data-only or stub-fill, low risk), then
  B2, B6, B7 (guardrails need exercising). Each behind its own E-only roll.
- **Phase 3 — Signatures:** `LevelSignature` pool — C2/C2b/C2c/C2d and C1 shipped; C4/C5
  next (low effort, proven primitives), then C3. One signature max per level.
- **Playtest gates between phases**, on fixed seeds pre/post:
  1. still completable;
  2. ammo economy intact (the A2+A4 interaction);
  3. no zone unwinnable on entry (B6 respawn + B7 blood door + alarm-stack coincidences);
  4. **time-to-complete did not materially increase** — the constraint is the metric. If a
     change makes E *longer*, it fails the gate even if it makes E harder.
- **When E overshoots, back off in order:** Group C roll weights → B5 stacking rate → A5
  points. Never re-add ammo above D parity as the fix.

## Key files

- `AutogenRundown/src/DataBlocks/Enemies/EnemyPopulation.cs` — tier rosters (A2)
- `AutogenRundown/src/DataBlocks/Enemies/EnemyGroup.cs` — hibernation group shapes (A1)
- `AutogenRundown/src/BuildDirector.cs` — points per zone (A5)
- `AutogenRundown/src/DataBlocks/LevelLayout.cs` — scouts, blood doors, group mixing,
  error alarms (A3, A6, B7)
- `AutogenRundown/src/DataBlocks/Zone.cs` — ammo, alarm modifiers, respawn knobs, door
  events (A4, B5, B6, C5)
- `AutogenRundown/src/DataBlocks/Level.cs` — starting-state overrides, wave settings,
  expedition balance selection (B2, B3, B4)
- `AutogenRundown/src/DataBlocks/LevelSettings.cs` — E modifier rolls, `LevelSignature`
  home (B2, Group C)
- `AutogenRundown/src/Extensions/WardenObjectiveEventCollections.cs` — `AddScriptedErrorAlarm`
  stub (B1, C2)
- `AutogenRundown/src/Extensions/WardenObjectiveEventCollections.cs` — event helpers
  (C1, C3, C5)
- `docs/game/rundowns/` — official E-level references (R2E1, R4E1, R5E1, R7E1, R8E1, R8E2)
