# D-Lock Decipherer Log Summary

Reference document for all D-Lock Decipherer logs in GTFO. Use as a style guide for writing new logs and as a lore reference for generating contextually appropriate content.

Source: `DLockDecipherer.cs` entries mapped to `GameData_TextDataBlock_bin.json`, extracted to `docs/dlock-decipherer-logs/rundown-{1-8}/`.

## 1. Overview

188 logs across 8 rundowns:

| Rundown | Count | Primary Timeline | Dominant Themes                                              |
| ------- | ----- | ---------------- | ------------------------------------------------------------ |
| R1      | 9     | 2042-2054        | Facility setup, virus research, early KDS ops                |
| R2      | 12    | 2049-2063        | Construction, EBDT fragments, pandemic escalation            |
| R3      | 12    | 2050-2057        | Creature taxonomy, Schaeffer awakening, societal collapse    |
| R4      | 19    | 2049-2058        | Garganta lockdown, psychological horror, hydrostasis         |
| R5      | 43    | 2049-2055        | Corporate conspiracy, KDS power, facility politics           |
| R6      | 46    | 2044-2063        | Artifact research, Matter Wave Projector, dimensional travel |
| R7      | 21    | 2049-2055        | Quantum theory, pandemic geopolitics, Dr. Teale's collapse   |
| R8      | 26    | 2053-2058        | Final stand, Mr. Truth's grief, BIOCOM combat protocols      |

Timeline spans **2042-2063**, with the bulk of events concentrated in **2049-2055**.

R7 and R8 share many duplicate logs (approximately 12 files are reused across these rundowns), bringing unique content to roughly 170 distinct documents.

File naming convention: `R[RUNDOWN][TIER]_[CODE].txt` (e.g., `R5A2_ETY-4UJ-HHB.txt`).

---

## 2. Document Format Types

### D-Lock Encrypted Emails

The most common format. Corporate correspondence between Garganta personnel with encrypted headers.

**Header format:**

```
D-Lock Block Cipher

alias:int_server.1024_ciph.tier5.phys_ops/DLockwoodA074.flagged

From: James Durant B255
To: Dean Lockwood A074
Date: June 13th 2054
Subject: Signal
```

**Characteristics:**

- Formal salutation ("Mr. Lockwood,")
- Employee ID codes after names (letter + 3 digits: B255, A074, C067)
- Tiered clearance system (tier 2 through tier 5)
- Subject lines are terse, often single words
- Body text ranges from brief directives to multi-paragraph scientific analysis
- Some contain email chain replies (Re: Re: Re: format)

**Examples:** R1B1_834-786-872 (scientific analysis), R5A1_1JL-OYY-NBR (corporate directive), R4A3_DL4-39F-Q5Z (classified installation orders)

### EBDT Fragments (Encrypted Block Data Transfers)

Corrupted transmissions with significant data loss. Unique to R2 and scattered elsewhere.

**Header format:**

```
>>EBDT FRAGMENT 0BE204 START
```

**Characteristics:**

- `[corrupted]` placeholders for missing data
- Partial sentences that cut off mid-thought
- Fragment IDs (0BE201-0BE205)
- Enough readable content to hint at cover-ups, personnel decisions, and security breaches
- End with `>>EBDT FRAGMENT [ID] END`

**Examples:** R2A1_EBDT-0BE204 (contractor risk), R2B1_EBDT-0BE205 (loyalty defense), R2D1_EBDT-0BE203 (contractor vetting)

### Shortwave Radio Broadcasts (State of Truth AM)

Conspiracy radio show hosted by "Mr. Truth." Long-form monologues.

**Format:** No formal headers. Opens directly with the speaker's voice.

```
This is State of Truth AM. I want to talk about something... personal today.
You know, I can't very well call myself Mr. Truth if I'm not honest with
you, can I?
```

**Characteristics:**

- First person, conversational, spoken cadence with ellipses and dashes
- Addresses audience directly ("You know the number. Call.")
- Mixes personal vulnerability with apocalyptic observation
- Escalates from paranoid investigation to on-the-run survival narrative
- Sign-off: "Don't touch that dial. Mr. Truth out."
- References Mrs. Truth (wife), "Truth Seekers" (listeners), "Castle Truth" (safe house)
- Extended metaphors and parables (e.g., "Mr. Court" diner allegory in R8)
- Email contact: StateOfTruth@pm.me

**Examples:** R1D1_K94-6ER-ESF (false memories), R2E1_76Y-TTF-FWW (fleeing compound), R2E1_JN8-M96-M31 (apocalyptic road), R4B3_YTG-N60-439 (return broadcast), R8A1_VER-T96-3UN (grief monologue)

### BIOCOM / Warden System Logs

Machine-generated terminal output from the facility's AI systems.

**Format:**

```
ACCESS -BIOCOM -29$525GVTHR2%456FS
Instantiating BIOCOM.net(CLI)...
ready:_
authorized...
connecting...
\\Root\RETURN NAVDATA QUAD: Z30-Q2631A
```

**Characteristics:**

- CLI-style interface with backslash paths and function calls
- Asset tracking with percentage integrity (e.g., "Organs 27%", "Respiratory 82%")
- Quad team tracking with status codes (Combat, Evac, RESOURCE NOT FOUND)
- Dictionary/database queries with entry counts
- Timestamps in encoded format (e.g., `2063.04.14.03.24.37`)
- Cold, mechanical language with no emotional content
- WRDN (Warden) override sequences

**Examples:** R2D2_CRW-EUH-3YU (quad tracking), R6A1_PUT-THX-3DX (medical readout), R2B2_000-000-000 (membrane database), R8D1_WKH-N2E-TAZ (operative deployment)

### HearSay Audio Log Transcripts

Personal recordings captured by the facility's HearSay surveillance/recording system.

**Format:** Either raw first-person monologue or marked with transcript headers:

```
//HearSay auto-transcript begins//
```

**Characteristics:**

- Spoken cadence with hesitation markers ("I... I'm recording this so I...")
- Interruptions by internal voices or external sounds
- Stream-of-consciousness under duress
- Physical actions described through audio cues ("EXTRACT! RETREAT! COMPLY!")
- Some are intentional recordings, others are surveillance captures

**Examples:** R3C1_B94-E5W-52K (Schaeffer's first recording), R4B1_6J8-9NK-8YT (Schaeffer's investigation), R7B1_HBR-YH6-D88 (Teale's breakdown)

### Psychotherapy Session Transcripts

Dialogue-format transcripts of medical consultations.

**Format:**

```
TEALE
Mia?

MIA
What?

TEALE
I'm Dr. Teale.
```

**Characteristics:**

- Speaker names in caps on their own line, followed by dialogue
- Professional clinical tone from therapist, escalating distress from patient
- Physical actions described in prose between dialogue
- Sessions often end with violence or psychological break
- Dr. Teale appears in multiple sessions, deteriorating across the timeline

**Examples:** R4B3_XMN-GBB-8B5 (Mia - body horror), R6AX_QKL-RFH-4ZX (Edgar Soweta - hallucinations), R7B2_T0L-4Y4-R1H (Soweta - medication refusal)

### News Articles (Mambo Media Services)

Journalistic pieces from in-universe media outlets.

**Header format:**

```
Mambo Media Services news wire
Iridium, SMC, Price Skyrockets
Report from Torsten Kass, Financial Correspondent, WNA
March 24th 2049
```

**Characteristics:**

- Wire service format with publication name, headline, byline, date
- Multiple publications: Mambo Media Services, Liberty Sentinel, Politika Pensado, Science Weekly (The Knowledge Tree)
- Professional journalism tone, increasingly apocalyptic as timeline advances
- Some end with `[Encryption checksum mismatch. Contact your IT administrator]`
- Subject matter: financial markets, geopolitics, pandemic reports, scientific features

**Examples:** R4B2_276-908-7U8 (iridium market), R1D1_BNC-XUI-P59 (ESA Mars mission), R5A1_3RY-978-UGB (UN politics), R4D2_V94-ER1-120 (Liberty Sentinel farewell)

### Analyst Reports & Safety Memos

Internal facility documentation from medical/administrative staff.

**Header format:**

```
Analyst Report
Reported by: Boris Stanovich C066 (Report Analyst, Medical Dept.)
Date of Report: December 12th 2052
Report: #SAMD-C043-07
```

**Characteristics:**

- Formal structure with report IDs (SAMD-XXXX-XX format)
- Statistical data (percentages, man-hours, case counts)
- Recommendations section
- Professional tone struggling against systemic dysfunction
- Author signature at end

**Examples:** R4B2_KAL-P48-7FR (suicide rate report), R4C2_T87-OPE-NAW (lockdown procedures), R6C1_WYP-9KB-3VV (quarantine violation), R6D4_FTJ-8GE-T1R (incident suppression)

### Scientific Research Notes

Personal research documentation, often found in "vaults" (private digital storage).

**Header format (Hammerstein style):**

```
---
Category: Weekly Notes 12/08 - 12/14 2053
Reported by: Dr. Gallus Hammerstein B014
Created: December 14th 2053
Modified: December 16th 2053
---
```

**Characteristics:**

- Markdown formatting with headers and bullet points
- Mix of scientific observation and personal digression
- TODO lists and book title brainstorming
- Increasingly unhinged personal asides
- Species naming and classification notes

**Examples:** R1C2_23G-B8E-MGK (Hammerstein weekly notes), R3A1_7GY-HYP-SDW (creature taxonomy), R3A3_21M-CEW-49B (Stokes rebuttal draft), R7B3_QSW-EQW-S99 (Hammerstein progress report)

### Inventory / Delivery Confirmations

Brief logistical records.

**Format:**

```
28x neonatal HS units
12x cases N-33S_endocrine_stimulator
...
Delivery Confirmation:: B-528 Xavier Lightfoot
Note: Goods received by KDS representative #00118-G at Zone 44 Area C checkpoint
```

**Characteristics:**

- Item counts with "x" notation
- Technical item names (pharmaceutical/biological)
- Confirmation signatures with employee IDs
- Zone/Area location references
- Very brief, purely functional

**Examples:** R4C1_EPL-LPY-T7N (neonatal HSU shipment)

### WHO Official Statements & Pandemic Reports

Public health communications broadcast via shortwave.

**Format:** Varies from formal press releases to intercepted shortwave transmissions with garbling.

**Characteristics:**

- Official spokesperson attribution (Dr. Gloria Rutermayer)
- Epidemiological data (case counts, R-naught values, mortality rates)
- Symptom descriptions following clinical language
- Increasingly desperate tone as pandemic worsens
- Late-stage broadcasts are garbled with `[inaudible]` markers

**Examples:** R2B2_UJ7-OYT-REH (4.8 billion cases report), R6BX_J77-MNB-CXZ (phase 6 declaration), R8A1_56G-75H-Y78 (isolation recommendations), R6D4_226-CAQ-PLK (garbled Geneva broadcast)

### Draft Documents with Editorial Breakdowns

Unfinished documents revealing the author's psychological state through editing marks.

**Characteristics:**

- Strikethroughs and ALL CAPS emotional outbursts inline with formal text
- Self-directed commands ("Lie. Be a liar. LIE.", "SAY YOU'RE SORRY")
- Contradictory statements revealing true feelings
- Professional veneer cracking in real-time

**Examples:** R4D2_EYY-GBB-18P (Stokes' death announcement draft - "I AM A LIAR I AM A LIAR")

### Emergency Alert System Broadcasts

Government emergency communications.

**Characteristics:**

- Mechanical, official delivery
- Symptom lists (hemoptysis, muscular spasms, spinal deformation)
- Curfew orders, isolation protocols
- Repeated broadcast format

**Examples:** R6A1_W69-Y11-B3Z (nationwide curfew order)

### GAO / Government Investigation Reports

Formal government audit documents.

**Characteristics:**

- Report IDs (GAO-47-026-B)
- Financial discrepancy analysis
- Ethical concerns with formal recommendations
- Structured sections with findings and recommendations

**Examples:** R6B1_09U-TR6-RZX (KDS financial discrepancy - $432B vs $626B spent)

---

## 3. Writing Style Guide

### Tone Progression Across the Timeline

The logs collectively trace a descent from **bureaucratic normalcy** to **existential horror**:

1. **Pre-2050 (Corporate optimism):** Professional correspondence, financial excitement about iridium, construction progress reports. Tone is confident, forward-looking, occasionally conspiratorial but controlled.

2. **2050-2052 (Growing unease):** Mining accidents, anomalous readings, first hints of the "Garganta Flu." Documents maintain professional veneer but include more hedging language, more requests for secrecy.

3. **2052-2053 (Crisis escalation):** NAM-V outbreak, suicide spikes, creature encounters, lockdown procedures. Formal language strains against the weight of what it describes. Euphemisms proliferate ("recent events," "the situation").

4. **2053-2055 (Collapse):** Psychological breakdowns in real-time, desperate field reports, apocalyptic broadcasts. Professional format persists but content becomes raw, emotional, contradictory.

5. **2055-2058 (Aftermath):** Final newspaper editions, garbled shortwave transmissions, system logs tracking lost operatives. Documents become sparse, fragmented, elegiac.

### Corruption & Fragmentation Markers

Logs use several techniques to convey information loss and system degradation:

- **`[corrupted]`** - Missing text in EBDT fragments
- **`[inaudible]`** - Garbled audio in shortwave intercepts
- **`[Encryption checksum mismatch. Contact your IT administrator]`** - Abrupt termination of news articles
- **Trailing ellipses and dashes** - Thought interrupted or speaker losing coherence
- **ALL CAPS OUTBURSTS** - Author breaking through their own professional facade
- **Strikethrough editing marks** - Draft documents showing revision and emotional leakage
- **System errors** - "RESOURCE NOT FOUND", "resources exhausted", timeouts
- **Reversed/obfuscated text** - R6BX_20V-M6M-2X9 uses text that reads right-to-left

### Corporate Language Patterns

The facility's corporate voice uses specific rhetorical strategies:

- **Euphemism:** "recent events" (creature attack), "additional surface time" (cancelled leave compensation), "the situation" (NAM-V outbreak), "procured assets" (kidnapped operatives)
- **Narrative control:** "We cannot conclusively state that..." (we know but won't say), "Control the narrative" (explicit directive from Lockwood)
- **Manufactured morale:** Exclamation marks in announcements ("Spring Break!"), vague promises of compensation, appeals to team spirit while cancelling leave
- **Tiered secrecy:** References to clearance levels gate what can be discussed ("tier 4 clearance only," "need-to-know basis")
- **Plausible deniability:** Parenthetical clarifications denying involvement even while demonstrating it

### Technical Jargon Conventions

- **Employee IDs:** Letter prefix indicating role tier (A=executive/senior, B=specialist/scientist, C=engineer/operations, D=supervisor/systems, E=labor/mechanic) + 3-digit number
- **KSO designations:** O- (standard operative), GO- (ground operative), F- (field), T- (tactical)
- **Zone references:** Zone + number + Area + letter (e.g., "Zone 44 Area C")
- **Pharmaceutical:** IIx fast-twitch boosters, I2-LP (Injury Infection Liquid Pharmacon), N-33S endocrine stimulator, gambogic amide, NGF protein clusters
- **Geological:** Cretasium (unknown mineral/substance interfering with equipment), CMPS readings (air quality), platinoid/iridium ore processing
- **Systems:** BIOCOM (facility AI), WRDN/Warden (control system), HearSay (audio surveillance), D-Lock (encryption), DISC (Diurnal Illumination System Control), ELG (Emergency Lighting Grid), CMS (Containment Material Silos)
- **Projects:** Project Insight (alien research), Project RISE/Legion (clone soldier program), AlphaGen/Sigma (operative generations), PRDM (Personnel Record Demographic Materials)
- **Report IDs:** SAMD-XXXX-XX (medical), ITS-01 (security), PX-XXX (Project Insight classified)

### Emotional Voice Under Professional Constraint

The most distinctive stylistic feature of these logs is **professional people losing their composure within formal documents**:

- Boris Stanovich writes measured analyst reports while the world falls apart around him
- Dr. Stokes composes a death announcement while screaming "FUCK ME" and "I hope those fucking creatures ripped the flesh from his bones" between paragraphs
- Andrew Clinton sends cheerful all-staff emails about "Spring Break!" while burying the lead about a viral outbreak
- Dr. Teale maintains clinical observation notes while his own sanity unravels

This tension between **format and content** is the core dramatic engine of the logs.

---

## 4. Key Characters with Voice Profiles

### Mr. Truth (State of Truth AM Host)

- **Role:** Conspiracy radio host, survivalist, apocalyptic prophet
- **Voice:** Conversational and intimate, shifts between vulnerable confession and thundering rhetoric. Uses ellipses for pauses, dashes for sudden thoughts. Addresses audience as peers. Extended metaphors and parables. Biblical/mythological allusions.
- **Arc:** Curious investigator (false memories) → hunted fugitive (government/KDS pursuit) → apocalyptic witness (mass death on highways) → grieving widower (loss of Mrs. Truth in Nevada)
- **Key phrases:** "Don't touch that dial," "Mr. Truth out," "Truth Seekers," "Castle Truth," "Babylon is burning"
- **Logs:** R1D1_K94-6ER-ESF, R2E1_76Y-TTF-FWW, R2E1_JN8-M96-M31, R3C1_21W-SOL-9NY, R4B3_YTG-N60-439, R7C1_2W1-EW2-1PT, R7C2_56G-75H-Y78, R8A1_VER-T96-3UN, R8A2_VAB-FF6-NKK

### John Schaeffer (C067 / T-3894)

- **Role:** Engineer turned KSO operative, hydrostasis survivor, eventual rebel
- **Voice:** Initially confused and fragmented, interrupted by Warden commands in ALL CAPS. Gradually becomes more coherent, analytical, determined. Noir-detective introspection. Uses questions to build understanding.
- **Arc:** Implant-controlled operative → confused awakening → systematic investigation → discovery of hydrostasis crypt → resolve to escape and rescue others
- **Key phrases:** "My name is John Schaeffer," "EXTRACT! RETREAT! COMPLY!", "I'm getting the fuck out of here"
- **Logs:** R3C1_B94-E5W-52K, R3D1_HW5-BI7-120, R3D1_POI-M7Y-THE, R4B1_6J8-9NK-8YT, R4C1_4HB-6UR-YTE, R4E1_M7B-NHG-F27

### Dr. Gallus Hammerstein (B014)

- **Role:** Geologist/lead researcher on Project Insight, creature taxonomist
- **Voice:** Grandiose, egotistical, paranoid. Uses markdown formatting in personal notes. Book title brainstorming. Names species after himself ("Parasitidae Hammerstein," "Gallus Reptus"). Dismissive of colleagues, especially women. Latin motto: "Non ducor, duco" (I am not led; I lead).
- **Arc:** Ambitious researcher → power consolidation ("culling" rivals) → killed by creatures (possibly welcome news to colleagues)
- **Physical state:** "Head is heavy," "brain feels damp" — possibly early infection symptoms
- **Logs:** R1C2_23G-B8E-MGK, R3A1_7GY-HYP-SDW, R6C1_CVB-SK5-F71, R6C3_EEA-2UN-577, R7B3_QSW-EQW-S99

### Andrew Clinton (B035)

- **Role:** Project Manager, corporate operator
- **Voice:** Shifts between forced cheerfulness (all-staff emails) and cold corporate maneuvering (private correspondence). Uses exclamation marks performatively. Maintains relationships through favors and implied threats. Personal warmth occasionally surfaces (mentions daughter Ariana).
- **Arc:** Construction overseer → Garganta project manager → narrative controller → dies in "October 13th catastrophic event" (energy surge, 2053)
- **Logs:** R2B1_EBDT-0BE205, R2B4_EBDT-0BE201, R3A2_DTE-110-111, R4A3_DL4-39F-Q5Z, R4A3_HZW-8JK-LTT, R4C3_6YH-KKN-B11, R5A1_1JL-OYY-NBR, R6B1_BKS-BTP-FD9

### Dr. Rebecca Stokes (B162/B193)

- **Role:** Project Insight lead (after Hammerstein's death), Matter Wave Projector researcher
- **Voice:** Confrontational and evidence-driven in rebuttals. Audio logs shift between scientific precision and emotional exhaustion. Growing paranoia about surveillance ("I can hear you out there"). Draft documents reveal buried rage and self-loathing ("I AM A LIAR").
- **Arc:** Scientific challenger to Hammerstein → reluctant project lead → quantum theory breakthrough → paranoid isolation → emotional deterioration
- **Logs:** R3A3_21M-CEW-49B, R4D2_EYY-GBB-18P, R6BX_D34-4HG-8AE, R6C1_2LZ-SAA-6NY, R6C1_YCQ-8K3-388, R6C2_H9K-HYR-1TE, R6CX_7JY-T12-ESQ, R7B3_2CB-5U8-DBK, R7D1_2MD-N3H-SYH

### Dr. August Teale (B023)

- **Role:** Psychotherapist / facility psychiatrist
- **Voice:** Clinical and measured in early sessions. Progressively fragmented, paranoid, stream-of-consciousness. Experiences the same symptoms he diagnoses in patients. Audio glitches suggest infection spreading to the recordings themselves.
- **Arc:** Professional clinician → patient encounters escalate (Mia, Edgar Soweta) → hearing noises from walls → paranoia about being a test subject → personal files altered → "I have to get out of here"
- **Logs:** R4B3_XMN-GBB-8B5, R6AX_QKL-RFH-4ZX, R6C1_WT7-RH8-WFD, R7B1_HBR-YH6-D88, R7B2_T0L-4Y4-R1H, R7C1_U5E-GGY-J89, R7C2_D3T-INI-FNI

### Anders Johanson (A029)

- **Role:** KDS leader, strategic decision-maker
- **Voice:** Formal, business-like, results-oriented. Discusses human beings as resources and products. Impatient with delays. References to "Johanson Papers" (leaked documents revealing hydrostasis memory loss).
- **Logs:** R6AX_R5L-NLL-023, R6BX_LWA-519-3VX, R6D3_HAS-I5T-DAY, R7B2_SVN-QAS-XPO

### Dean Lockwood (A074)

- **Role:** Senior facility authority, tier 5 clearance
- **Voice:** Formal, authoritative, controlled. Issues directives with precision. Concerned with narrative management and information containment. Occasionally shows ethical conflict but prioritizes operational security.
- **Arc:** Authority figure → NAM-V terminology enforcer → assumes project management after Clinton's death → increasingly isolated decision-maker
- **Logs:** R1B2_3NY-798-65Y, R4D1_7JH-PPP-SAD, R6B2_34R-OKJ-992, R6BX_20V-M6M-2X9, R6BX_LWA-519-3VX, R6C2_DTR-298-4VL, R6D1_D1H-J34-5XX

### Nolan Connor (B229)

- **Role:** Communications/investigations, Project Insight member
- **Voice:** Investigative, factual, emotionally restrained. Delivers evidence without editorializing. Final journal entry is resigned but determined.
- **Arc:** Forensic investigator → Project Insight team member → expedition artifact recovery → final stand at the Inner (2056)
- **Logs:** R1B1_834-786-873, R6CX_W35-9BV-EUV, R8C1_AKK-3XW-EEB

### Andre Piros (A134)

- **Role:** KDS operational commander at Garganta
- **Voice:** Military brevity, authoritative, refuses elaboration. Issues search authorizations and containment orders. Blocks information flow.
- **Logs:** R1B2_3NY-798-65Y, R1C2_LB2-4BK-902, R6B2_34R-OKJ-992, R6D4_SK8-G25-G65

### Janson Davies (A001)

- **Role:** Dreyfus Industries executive, facility director
- **Voice:** Executive distance, plausible deniability, careful disclaimers. Emphasizes corporate hierarchy. Refers to himself in careful third-party framing.
- **Logs:** R2B4_EBDT-0BE201, R2C1_EBDT-0BE202, R5A1_1JL-OYY-NBR, R6D4_SK8-G25-G65

### Boris Stanovich (C066)

- **Role:** Report analyst, Medical Department
- **Voice:** Measured, data-driven, professional. Presents alarming findings with clinical detachment. Recommendations are practical and increasingly urgent. A voice of institutional sanity in a collapsing system.
- **Logs:** R4B2_KAL-P48-7FR, R4C2_T87-OPE-NAW, R6C1_WYP-9KB-3VV, R6D4_FTJ-8GE-T1R

### James Durant (B255)

- **Role:** Scientist, virus researcher, EF-01 expedition leader
- **Voice:** Formal scientific correspondence. Precise, analytical, measured concern. Romantic correspondence with Susan Nesbitt shows a warmer, excited side.
- **Arc:** Virus researcher → EF-01 expedition through Matter Wave Projector → stranded on alien world → field reports from jungle environment
- **Logs:** R1B1_834-786-872, R6CX_W35-9BV-EUV, R6D1_4AK-KPG-6BE, R7A1_CJ9-GVT-ERB

### Angela Klein (A106)

- **Role:** KDS personnel/behavioral science
- **Voice:** Administrative, technical. Discusses clone operative quality, behavioral assessment, implant specifications. Treats engineered humans as products.
- **Logs:** R1D1_TAB-GTG-YUM, R6C2_FFW-HYY-6FL, R6C2_UFR-710-NHA, R7D2_98R-T5B-N57, R8C1_LUM-3SS-6ZN

### Michel LeFavre (B028)

- **Role:** Medical Director
- **Voice:** Official, slightly condescending. Includes dictionary definitions in memos. Asserts medical authority.
- **Logs:** R7D1_7TR-VST-RBY, R7B1_SM1-X918-XWM

### Dr. Abeo Dauda (A153)

- **Role:** Virus researcher
- **Voice:** Technical medical documentation. Characterizes NAM-V as "perfectly designed."
- **Logs:** R7E1_60F-056-HNK

### Eric Chapman (C368)

- **Role:** Medical records clerk, self-identified autistic
- **Voice:** Direct, literal, socially unconventional. Initiates friendship based on logical compatibility assessment. Includes personal observations others would omit.
- **Logs:** R4D1_XDL-9YD-1QA

### Matthew Aaron

- **Role:** Daylight Mission operations lead (WHO)
- **Voice:** Military field report cadence. Professional, narrative detail, cautious optimism.
- **Logs:** R3B1_57H-K78-EVR, R4A2_FCL-K5E-2BG

### Howard Stokes (B341)

- **Role:** Geologist, off-site researcher (Dr. Rebecca Stokes' relative)
- **Voice:** Collaborative, scientific. Provides spectroscopy analysis of alien materials.
- **Logs:** R6C2_THB-G61-UWU, R6D1_8AA-FEE-EDD

### Igor Rostok (C122)

- **Role:** Engineer, physical operations
- **Voice:** Casual technical correspondence, workplace complaints, defiant humor.
- **Arc:** Worker → reports surveillance concerns → killed by KSO for entering restricted area
- **Logs:** R4C1_G62-988-G2Q, R6C3_MQQ-792-B8L, R6D4_SK8-G25-G65, R7B1_B47-EH6-REG

### Vitori Molina (C158)

- **Role:** Contractor foreman
- **Voice:** Incident reporting, procedural documentation.
- **Logs:** R8D1_B5R-T76-QLP

### Ellis Carnegie

- **Role:** Kovac Research, cybernetics division
- **Voice:** Technical specifications, detached. Discusses operative chip specifications and behavioral classification.
- **Logs:** R6C2_UFR-710-NHA, R6D3_HAS-I5T-DAY

---

## 5. Organizations & Locations

### Organizations

| Name                               | Role                        | Key Details                                                                                                                                     |
| ---------------------------------- | --------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| **Santonian Mining Company (SMC)** | Mining operator             | Runs Garganta facility. Parent: Dreyfus Industries. Iridium mining cover for alien artifact excavation.                                         |
| **Dreyfus Industries**             | Corporate parent            | Funds SMC, provides infrastructure investment. Mr. Dreyfus referenced but never directly appears.                                               |
| **Kovac Defense Services (KDS)**   | Private military contractor | Security at Garganta. Operates Project RISE (clone soldiers). GAO flagged $194B spending discrepancy. Recruits from prisons.                    |
| **Project Insight**                | Research division           | Studies alien creatures and artifacts within Garganta. Led by Hammerstein, then Stokes.                                                         |
| **Project RISE / Legion**          | Clone soldier program       | KDS black ops. AlphaGen → Sigma generations. Grows operatives with behavioral modification via Sec-B-M chips. 60-day accelerated maturity.      |
| **BIOCOM**                         | Facility AI system          | Manages all facility systems: medical, environmental, security, navigation. 100% uptime, EMP-resistant. Eventually hijacked/merged with Warden. |
| **Warden (WRDN)**                  | Control AI                  | Hijacked BIOCOM. Controls implanted operatives. Issues commands (EXTRACT, RETREAT, COMPLY). Deploys "Seek and Destroy" missions.                |
| **WHO**                            | International health body   | Tracks NAM-V pandemic. Dr. Gloria Rutermayer leads. Sends Daylight Mission to Garganta (2057).                                                  |
| **Mambo Media Services**           | News agency                 | In-universe wire service. Publishes multiple outlets.                                                                                           |
| **State of Truth AM**              | Independent radio           | Mr. Truth's conspiracy broadcast. Covers earthquakes, pandemic, government collapse.                                                            |
| **ESA**                            | Space agency                | Red Alpha Mars mission (2042). Severed ties with Kovac after Johanson Papers leak.                                                              |
| **Nyxos**                          | Equipment contractor        | Supplies HSU units and spectroscopy equipment. Favored over McCallister.                                                                        |
| **McCallister**                    | Contractor (dismissed)      | Dropped for asking too many questions in restricted areas.                                                                                      |

### Locations

| Name                       | Description                                                                                                                                                                                                                                                    |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Garganta**               | Underground facility in Yucatan Peninsula, Mexico. Built over Chicxulub impact site. Massive drill shaft (100ft diameter) reaching 4,200m+ depth. Contains mining, research, habitation, and military zones. ~1.8M+ sq/m total space (185K sq/m unregistered). |
| **The Inner / The Fossil** | Ancient alien spacecraft buried in the crater. Contains artifacts, data blocks, the Matter Wave Projector. Sections include "The Gallery," "The Bridge." Built by "the Collectors." Always called "the Fossil" in official communications (never "vessel").    |
| **KDS-Deep**               | Classified KDS area deep in facility. Contains thousands of hydrostasis units holding operatives. ~4M cubic meters before access rescinded.                                                                                                                    |
| **Chicxulub**              | Impact crater site. Cover story for mining operations. "Chicxulub Operations Center" is the public-facing name.                                                                                                                                                |
| **Valiant Chamber**        | Largest natural void in the facility: 7.3M cubic meters.                                                                                                                                                                                                       |
| **Shaft 17b**              | Deepest point: 4,200m. Location of Intrepid Chamber where Hammerstein's team died.                                                                                                                                                                             |
| **Translocation Chamber**  | Where Matter Wave Projector is operated. Launch point for EF-01 expedition.                                                                                                                                                                                    |
| **Progreso**               | Port town in Yucatan. Daylight Mission base camp. Completely evacuated.                                                                                                                                                                                        |
| **Merida**                 | Mexican city. 9.8 earthquake in 2049 (suspicious — zone has no fault line). Evacuation staging area.                                                                                                                                                           |
| **Kvitoya DNA Vault**      | Svalbard. Sealed permanently in 2054 with 3.4M animal DNA samples and 58 volunteers.                                                                                                                                                                           |
| **Pahrump, Nevada**        | Ghost town where Mr. Truth loses Mrs. Truth.                                                                                                                                                                                                                   |

---

## 6. Core Lore Threads by Rundown

### Rundown 1: Foundations

- James Durant's definitive analysis: NAM-V and parasites are **extraterrestrial**, likely share origin
- Virus-parasite symbiosis: virus extends parasite lifespan, parasite provides transmission vector
- **KSO Bishop anomaly:** Two different masks with his DNA after EF-01 return (implies dimensional duplication)
- Unknown signal broadcasting from within facility
- Andre Piros authorizes all-points search for escaped target carrying "extremely hazardous material" (Dec 2052)
- Full Santonian Mining Company personnel manifest
- State of Truth introduces **false memory phenomenon** (Merida quake date discrepancy)
- Red Alpha Mars mission background (2042)

### Rundown 2: Fragments and Scale

- EBDT fragments reveal contractor politics, cover-ups, and "Problem #12"
- **Project RISE revealed:** KDS black ops program for genetically engineered soldiers (AlphaGen prototypes)
- WHO pandemic report: **4.8 billion cases, 2.27 billion deaths** by September 2055
- Patient Zero traced to Naples, Florida (December 2052)
- KDS quad team (including Schaeffer T-3894) lost in Zone 32 — all status: "Resource lost" (2063)
- BIOCOM membrane database processing (1 billion+ entries)
- State of Truth: Mr. Truth flees compound, documents mass death on highways

### Rundown 3: Awakening

- **Hammerstein's creature taxonomy:** Gallus Reptus, Jaculus, Capreolatus, Horridus. Multiple evolutionary origins ("trees, not branches")
- Stokes challenges Hammerstein: creatures contain **human DNA**, may be modified Garganta staff
- Pharmaceutical development: IIx fast-twitch boosters, syringe side effects
- **John Schaeffer's awakening sequence:** Implant control → confused recording → implant removal → defiant destruction
- Daylight Mission reaches Progreso: abandoned city, mass cremation sites, "chichiricu" creature sightings
- WHO official statement on Daylight Mission progress
- Mining operations memo: anomalous readings below 750m
- State of Truth: "Death rattle of our species," promise to rise from ashes

### Rundown 4: Descent

- **Daylight Mission at Garganta:** Massive drill hole, abandoned infrastructure, Chinook helicopter, BIOCOM system encountered
- HearSay surveillance system installation (classified, tier 4 only)
- Spring Break announcement buries NAM-V outbreak disclosure, leave cancelled
- **Boris Stanovich reports:** 34.4% workforce with psychological disturbances, "Garganta Flu" (no physical cause)
- **Dr. Teale & Mia session:** Body horror escalation, "It's in all of us"
- Iridium financial context: $2,868/oz, SMC stock surge, Dreyfus backing
- Facility space audit: 1.8M+ sq/m total, 185K unregistered, deepest point 4,200m
- **Lockdown procedures:** Reactor containment, Faraday locks, classified protocols (RISE-XFIL, PX-048)
- NAM-V terminology crackdown: "Garganta Flu"/"The Stoops" forbidden
- Eric Chapman's autistic observation: shared unexplained noises with Stanovich
- **Stokes' breakdown draft:** Death announcement for Hammerstein's team, inner rage exposed
- **Liberty Sentinel farewell:** U.S. government collapses (January 2058)
- **Schaeffer discovers KDS-Deep:** Thousands in hydrostasis, AI extracting memories as resources. He was frozen for 7 years.
- State of Truth returns after 70 days: "Babylon is burning and we survived"

### Rundown 5: Corporate Machinery

- Davies establishes D-Lock communications, clarifies corporate hierarchy
- UN General Assembly: KDS recruiting from prisons, "Armies for Rent" controversy
- Extensive corporate correspondence detailing facility politics, personnel conflicts
- KDS operational expansion, security tightening
- Deepening cover-up infrastructure, information compartmentalization
- Multiple document types showcase bureaucratic machinery enabling horror

### Rundown 6: The Artifacts

- **Matter Wave Projector discovery:** Silicon resin coating prevents "accidents" (people vanish on contact)
- **EF-01 expedition:** Translocation jump → contact lost → Durant's team stranded on alien jungle world
- Artifacts (data blocks) resemble digital storage devices. Cretasium substance may regulate behavior.
- **Artifact analysis:** Unknown material composition (24% unidentified), extra-terrestrial origin confirmed via Chicxulub meteorite correlation
- Clock anomalies on level L37 (230-second deviation per 24 hours) correlated with artifact location
- BIOCOM network interference below 500m from crystalline rock (Cretasium)
- KDS operative chips: Sec-B-M 2.7.4, 120,000 units, 1300MB/s upload speed
- **Andrew Clinton dies:** October 13th catastrophic energy surge creates blast tunnels
- Igor Rostok executed by KSO for entering restricted area
- Boris Stanovich reports 114+ unreported incidents, KDS denying creature existence
- **Iridium deposit discovery:** 1.5-2km wide vein, 4km deep, ~4,000 tons worth ~$300 billion
- Merida earthquake analysis: 9.8 in seismically inactive zone, suspicious timing
- Manchester NAM-V overflow: emergency rooms overrun, "The Stoops"
- Kvitoya DNA Vault preparations (referenced)
- Stokes grows paranoid about Kovac surveillance: "I can hear you out there"
- BIOCOM unauthorized access sequence (2063): 29M+ GLM entries compiled
- Rise operative behavioral issues: Sigma generation showing unexpected creativity, memory persistence

### Rundown 7: Unraveling

- **Quantum frequency discovery:** EF-01 team experienced time 115x faster (10 months for them = hours at Garganta)
- Dr. Stokes' deep theoretical analysis of dimensional mechanics and superposition
- **Dr. Teale's complete psychological collapse** across multiple sessions: hearing noises → paranoia → personal files altered → "I have to get out of here"
- Edgar Soweta: treatment-resistant psychosis, visions vs hallucinations, released by KDS
- **NATO dissolution** due to NAM-V: US population crashes from 474M to 302M in 3 years
- Journalistic investigation: bodies moved from exclusion zone, secret exclusion zones near coast
- NAM-V biological profile: 85% infection rate across all exposure methods, survives 4+ weeks outside host
- Sigma generation operatives developing autonomous behavior, spontaneous tool improvisation
- State of Truth: Merida earthquake conspiracy (not on fault line, iridium discovery timing)
- Durant-Nesbitt romance: protein sequencer breakthrough (Gramineae plants), Sigma generation research
- Igor reports surveillance concerns, KDS "thugs," plea for Clinton's intervention

### Rundown 8: Endgame

- **Nolan Connor's final journal (November 2056):** Family separated, sealed chamber with massive biomass, final expedition tomorrow
- **BIOCOM combat deployment:** Quad B452 assigned (Bishop GO-1395, Woods F-2056, Hackett O-4711, Dauda T-3701) — the four playable characters
- Mr. Truth in Pahrump, Nevada: Mrs. Truth is dead, "Mr. Truth is not here," existential collapse
- Mr. Truth's parable: "Mr. Court" diner allegory about exploitation and recognizing predators
- WHO operatives intercepted by BIOCOM defense system during unauthorized database access
- Disturbing personal recordings: pregnancy crisis, infection, possible infanticide
- **Kvitoya DNA Vault seals permanently** (December 2054): 3.4M samples, 58 volunteers, permanent isolation
- NAM-V incubation analysis: 46% of cases have extended asymptomatic period (18 days to 12 months)
- Angela Klein: Rise "seed" operative quality concerns, 60-day maturity insufficient
- Multiple duplicated logs from R7, suggesting these logs surface across multiple expeditions

---

## 7. Thematic Categories

### Corporate Conspiracy & Cover-ups

Documents reveal a layered system of information suppression: tiered clearance, forbidden terminology, contractor dismissals for asking questions, execution for trespassing, narrative control directives. The corporate machine continues generating cheerful memos while people die.

**Key logs:** R4A3_HZW-8JK-LTT (Spring Break/virus buried), R4D1_7JH-PPP-SAD (terminology enforcement), R2C1_EBDT-0BE202 (Problem #12), R2D1_EBDT-0BE203 (McCallister dismissed), R6D4_SK8-G25-G65 (Igor executed)

### NAM-V Pandemic Escalation

The virus progression is documented from first case (Naples, FL, Dec 2052) through global catastrophe (4.8B cases, 2.27B deaths by Sept 2055) to societal collapse (NATO dissolved, US government fallen by 2058). Documents trace the epidemic from localized "Garganta Flu" euphemism through WHO pandemic declaration to garbled final shortwave broadcasts.

**Key logs:** R4B2_KAL-P48-7FR (Garganta Flu), R6BX_J77-MNB-CXZ (phase 6), R2B2_UJ7-OYT-REH (4.8B cases), R7C2_EWF-R6U-RTH (NATO collapse), R4D2_V94-ER1-120 (US government collapse), R8E2_T5V-HW4-8PW (46% asymptomatic carriers)

### Alien Artifact Discovery

The Inner/Fossil is an ancient alien spacecraft buried in the Chicxulub crater. Artifacts include data blocks, the Matter Wave Projector, and materials with 24% unknown composition. Contact with artifacts causes disappearance. Cretasium substance interferes with electronics and may regulate artifact behavior.

**Key logs:** R6C1_CVB-SK5-F71 (unbreakable artifact), R6BX_D34-4HG-8AE (Matter Wave Projector), R6C2_THB-G61-UWU (spectroscopy analysis), R6C1_YCQ-8K3-388 (data blocks), R6C3_EEA-2UN-577 (the Inner exploration)

### Dimensional Travel & Temporal Anomalies

The Matter Wave Projector enables translocation to other dimensions/worlds. EF-01 expedition landed in an alien jungle where time runs 115x faster. Clock anomalies near artifacts suggest localized temporal effects. Mr. Truth's false memories may indicate timeline alterations.

**Key logs:** R6D1_295-H65-V3N (launch sequence), R6CX_W35-9BV-EUV (Durant's jungle), R7B3_2CB-5U8-DBK (115x time dilation), R7D1_2MD-N3H-SYH (quantum theory), R1D1_K94-6ER-ESF (false memories)

### Mind Control & Identity

The Warden system controls operatives through neural implants. Project RISE creates clone soldiers with behavioral modification chips. Schaeffer's arc from controlled KSO to self-aware rebel is the central personal narrative. Hydrostasis inhibits long-term memory retention.

**Key logs:** R3C1_B94-E5W-52K (implant control), R3D1_HW5-BI7-120 (implant removal), R2B4_FER-WI9-053 (Project RISE briefing), R6C2_UFR-710-NHA (Sec-B-M chips), R6C3_TJQ-WCG-JWX (hydrostasis memory loss), R7D2_98R-T5B-N57 (Sigma autonomy)

### Creature Biology & Taxonomy

Creatures are classified under Hammerstein's "Gallus" genus: Reptus, Jaculus, Capreolatus, Horridus. Each species has unique evolutionary origin ("trees, not branches"). They contain human DNA and may be modified facility staff. Darker pigmentation = more active hunters. Environmental factors at depth drive mutations.

**Key logs:** R1B1_834-786-872 (virus-parasite symbiosis), R3A1_7GY-HYP-SDW (Gallus taxonomy), R3A3_21M-CEW-49B (human DNA in creatures), R7E1_60F-056-HNK (virus biological profile)

### Psychological Horror & Institutional Decay

The logs document mental deterioration across the facility population: 34.4% reporting psychological disturbances, treatment-resistant psychosis, therapists becoming patients, unexplained noises from walls, "Garganta Flu" with no physical cause. The medical system itself breaks down as KDS suppresses incident reporting.

**Key logs:** R4B2_KAL-P48-7FR (suicide rates), R4B3_XMN-GBB-8B5 (Mia session), R7B1_HBR-YH6-D88 (Teale breakdown), R6D4_FTJ-8GE-T1R (114 unreported incidents), R4D1_XDL-9YD-1QA (Chapman's noises)

### Societal Collapse

Documents trace the end of civilization: NATO dissolves, US population drops by 172M in 3 years, Liberty Sentinel publishes its final issue, the internet faces shutdown, DNA vaults seal permanently. State of Truth broadcasts serve as the voice of the dying world.

**Key logs:** R4D2_V94-ER1-120 (Liberty Sentinel farewell), R7C2_EWF-R6U-RTH (NATO dissolution), R8E1_7TR-VST-RBY (Kvitoya vault sealing), R2E1_JN8-M96-M31 (mass death highways)

### Worker Exploitation & Safety

Garganta workers face cancelled leave, mandatory underground existence, suppressed health data, and execution for entering restricted areas. Contractors are dismissed for curiosity. KDS recruits from prisons involuntarily. Rise operatives are manufactured products with quality control issues.

**Key logs:** R4A3_HZW-8JK-LTT (leave cancelled), R6D4_SK8-G25-G65 (Igor executed), R5A1_3RY-978-UGB (prison recruitment), R6D3_HAS-I5T-DAY (operatives as products), R8C1_LUM-3SS-6ZN (accelerated maturity concerns)

---

## 8. Master Timeline

| Year     | Key Events                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **2037** | Pithovirus Klauvas discovered in Svalbard permafrost                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| **2042** | Red Alpha Mars mission launches from Kourou (ESA). Budget crisis forces reduced scope.                                                                                                                                                                                                                                                                                                                                                                                                                                |
| **2044** | ESA severs ties with Kovac after Johanson Papers reveal hydrostasis memory loss. Red Alpha crash landing on Mars.                                                                                                                                                                                                                                                                                                                                                                                                     |
| **2047** | GAO investigation flags KDS: $194B spending discrepancy, "Martial Trafficking" concerns, BIOCOM deployment.                                                                                                                                                                                                                                                                                                                                                                                                           |
| **2048** | Merida 9.8 earthquake in seismically inactive zone. Yucatan evacuation begins.                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| **2049** | Garganta blueprints finalized (Sept). Massive iridium deposit discovered near Chicxulub (~$300B). Iridium price surges to $2,868/oz. Clinton contacts Peres about Yucatan "situation."                                                                                                                                                                                                                                                                                                                                |
| **2050** | Garganta drilling begins (July). UN session: KDS prison recruitment controversy. Journalism investigation into Yucatan exclusion zones. Mining accidents begin. Anomalous readings below 750m.                                                                                                                                                                                                                                                                                                                        |
| **2051** | KDS Deep excavation delays. BIOCOM equipment installation. 120,000 Sec-B-M chips delivered.                                                                                                                                                                                                                                                                                                                                                                                                                           |
| **2052** | HearSay system installation (March). NAM-V outbreak at Garganta — Spring leave cancelled, virus downplayed (April). Suicide rate spikes. "Garganta Flu" coined. Artifact L18-AT06 proves unbreakable — extra-terrestrial origin confirmed. Unknown signal intercepted from facility (July). Patient Zero documented in Naples, FL (December). Target escapes facility with hazardous material (Dec 28). Boris Stanovich reports 34.4% psychological disturbance rate. Dr. Richtofer eye injury from artifact testing. |
| **2053** | NAM-V phase 6 pandemic declared (March). Manchester hospitals overflow. Nationwide US curfew. Dr. Teale's psychological deterioration begins. Andrew Clinton dies in energy surge (Oct 13). Lockwood assumes project management. Boris Stanovich reports 114+ unreported incidents. Igor Rostok executed. KDS-Deep sealed. Edgar Soweta treatment-resistant psychosis. Lockdown procedures enacted.                                                                                                                   |
| **2054** | Hammerstein's creature taxonomy published. Durant's virus research. Rise/HSU shipments to private vaults. Kvitoya DNA Vault seals permanently (Dec). Field promotions for KDS operatives. Stokes' quantum frequency discovery. NATO dissolution. NAM-V incubation analysis: 46% asymptomatic up to 12 months. US population: 474M → 302M.                                                                                                                                                                             |
| **2055** | WHO reports 4.8B cases, 2.27B deaths (Sept). EF-01 expedition through Matter Wave Projector — team stranded on alien world. Stokes' quantum dimensional theory. State of Truth broadcasts from isolation. KDS Sigma generation shows autonomous behavior.                                                                                                                                                                                                                                                             |
| **2056** | Nolan Connor's final journal: sealed chamber with massive biomass, final expedition planned.                                                                                                                                                                                                                                                                                                                                                                                                                          |
| **2057** | Daylight Mission reaches Progreso (April). Abandoned city, mass cremation sites. WHO garbled broadcast from Geneva.                                                                                                                                                                                                                                                                                                                                                                                                   |
| **2058** | Liberty Sentinel publishes final issue (Jan 13). US government collapsed.                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| **2063** | BIOCOM system operations continue. Schaeffer's quad team lost in Zone 32. Operative deployment: Bishop, Woods, Hackett, Dauda (the player characters).                                                                                                                                                                                                                                                                                                                                                                |
