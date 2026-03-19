# The Conduit

> A short-lived humanoid clone built by the Warden from RISE technology -- the Collectors' fragile, desperate attempt to speak to humanity face to face.

## Overview

| Field | Value |
|-------|-------|
| **Designation** | Conduit (BIOCOM subroutine: `conduit_init`, `conduit_create`, `conduit_assimilate`) |
| **Nature** | Artificially grown humanoid clone; biological communication interface |
| **Origin** | Created by the Warden using Project RISE cloning technology and genome data |
| **Function** | Bridge between the Collectors and humans; capable of surviving among Sleepers and communicating the Collectors' message |
| **Status** | Destroyed (multiple iterations killed by mercenaries or creatures; final iteration dies during conversation with ALT Schaeffer) |

## Description

The Conduit is a humanoid organism created by the Warden using technology and biological data from Project RISE, the KDS cloning program originally designed to mass-produce soldiers. Unlike RISE's military clones, the Conduit was purpose-built as a communication bridge -- a biological interface through which the Collectors could speak directly to humans. The Warden spent considerable effort preparing for each Conduit's creation: extracting genome data from the RISE database, compiling millions of GLM entries for the Conduit to assimilate, and running simulation after simulation to improve conformity and comprehension metrics.

Each Conduit was grown through an accelerated embryonic development process. BIOCOM logs document the procedure in clinical detail: preparing a synthetic ovum, isolating a nucleus, fusing genetic material, transporting the zygote, forming a blastocyst, implanting it, and initiating accelerated growth. The resulting organisms were approximate humans -- humanoid in form but physiologically fragile, with system integrity readings showing severe deficiencies (organs at 27%, digestive system at 14%, reproductive at 2%, immunity at 0%). Their expected lifespan was roughly four days (345,600 seconds). What they excelled at was communication: translation aptitude of 94% and comprehension ability of 83%.

The Conduit possessed a remarkable trait that no human could replicate: it could exist among Sleepers without being attacked. Whether through engineered pheromones, temperature regulation, or some behavioral characteristic the Collectors understood from their millennia of studying the Collection, the creatures recognized the Conduit as non-threatening. ALT Schaeffer observed one standing "in the middle of a mass of creatures, and they weren't attacking it." This immunity to the Sleepers was essential to the Conduit's function -- it needed to survive long enough in the hostile Complex to reach and communicate with humans.

The Conduit's speech was halting, associative, and occasionally incoherent. It mixed lucid statements of cosmic significance ("The virus is the primary life force of membranes in which it exists") with non-sequiturs ("Do you like dogs?") and deteriorating syntax as its brief lifespan ran out. Its voice was that of the Collectors filtered through an imperfect vessel -- alien intelligence straining against the limitations of a body that was already dying.

## Role in the Narrative

**Pre-Creation -- Research Phase:** Long before any Conduit is grown, the Warden begins preparations. It accesses the RISE project database through BIOCOM, extracting genome data and cloning protocols. It runs a subroutine called `conduit_informatics`, processing RISE data and simulating Conduit initialization parameters to improve conformity. Separately, it compiles over 29 million GLM database entries and feeds them into a `conduit_assimilate` function, building the knowledge base the Conduit will need to communicate meaningfully with humans.

**Repeated Failures:** Schaeffer's analysis reveals that the Warden's "conduit" subroutine had been active for some time before any successful communication occurred. The program would create a Conduit, attempt to have it reach a squad of mercenaries, and the Conduit would be "killed every time" -- either by the mercenaries who saw it as a threat, or by the creatures during the chaos of combat. "Then the program runs again, and it gets killed again." Each death prompted the Warden to iterate: improving the design, updating the simulation parameters, and trying again.

**ALT R6 -- Schaeffer's First Sighting (R6C2_FFW-HYY-6FL):** ALT Schaeffer witnesses a Conduit in the field for the first time. He describes seeing "someone -- approximately human" standing amid a mass of creatures that ignored its presence. When a squad of mercenaries entered through a security door, the Conduit reached out a hand to them, clearly attempting communication. The creatures attacked the mercenaries, and in the chaos, the Conduit was killed along with two of the squad. Schaeffer recognized the Conduit's intent was non-hostile: "It was trying to communicate. It was weak and looked like it was dying." He resolved that if he encountered another, he would try to talk to it.

**ALT R6 -- Creation of a New Conduit (R6A1_PUT-THX-3DX):** BIOCOM logs document the Warden creating a fresh Conduit after the previous one's death. The system gathers biological materials (cerebrospinal fluid, X-gen compound catalyst, N-33S endocrine stimulator, NGF protein, Nyxos HSU unit), evaluates the previous Conduit's system integrity data, and initiates `conduit_create(2)` -- growing a new organism from synthetic ovum through accelerated embryonic development.

**ALT R6 -- The Conversation (R6D1_8AA-FEE-EDD):** The pivotal moment of the GTFO narrative. ALT Schaeffer meets the new Conduit and, true to his word, speaks to it instead of killing it. The Conduit identifies itself as "a biological conduit for humans" that speaks on behalf of "us" -- the Collectors. It delivers the essential revelation: the Warden is an extension of the Collectors, the virus cannot be stopped, every membrane is impure, and humanity must project to a clean membrane to survive. The Conduit's coherence degrades as the conversation progresses -- it begins speaking of teaching humans "to regulate the Matter birthday" (likely the Matter Wave Projector) before lapsing into fragmentary speech: "Do you like dogs?" Its final words acknowledge its own failure: "Again meetings more meetings. This unit has failed." It dies shortly after.

**OG Timeline -- Killed by Schaeffer:** In the original timeline, OG Schaeffer encounters a Conduit but, viewing the Warden as an adversary, assumes it is a trap designed to deceive him. He ignores its attempts at communication and kills it. This act of distrust is the key divergence between the two timelines. Without the Conduit's message, OG Schaeffer proceeds to wage war against the Warden rather than cooperating with the Collectors' plan.

## Key Connections

- **The Warden (WRDN)** -- Creator and controller. The Warden builds each Conduit, feeds it knowledge through `conduit_assimilate`, and deploys it into the Complex to reach humans.
- **The Collectors** -- The intelligence that speaks through the Conduit. "This unit is of us."
- **Project RISE** -- The KDS cloning technology the Warden co-opts to grow each Conduit. Without RISE's accelerated development capabilities, the Conduit could not exist.
- **John Schaeffer (ALT)** -- The human who finally listens. His decision to talk to the Conduit instead of killing it is the turning point that enables the escape plan.
- **John Schaeffer (OG)** -- The human who kills the Conduit, believing it to be a Warden deception. This closes off the Collectors' communication channel in the original timeline.
- **The Sleepers** -- The Conduit can exist among them unharmed, a trait engineered by the Warden using the Collectors' understanding of the Collection's biology.
- **NAM-V** -- The virus whose omnipresence across membranes is the message the Conduit was built to deliver.

## Appearances

| Rundown | Log ID | Description |
|---------|--------|-------------|
| R2 | R2D2_CRW-EUH-3YU | Warden runs `conduit_informatics` subroutine, processing RISE data and simulating Conduit initialization. Reports 8.59% conformity improvement. |
| R5 | R5B3_F12-KLU-YN2 | Schaeffer identifies the "conduit" subroutine in Warden logs: "There is a subroutine in the Warden logs called 'conduit', which has been trying to communicate with the mercenaries, but it's been killed every time." |
| R5 | R5E1_8EL-71W-3HW | Schaeffer notes repeated Warden terminology including "conduit" and "specimen" -- evidence of the ongoing Conduit development program. |
| R6 | R6A1_PUT-THX-3DX | BIOCOM log: the Warden creates a new Conduit. Documents full biological creation process from synthetic ovum through accelerated embryonic development. System integrity report: organs 27%, immunity 0%, expected lifespan ~4 days, translation aptitude 94%. |
| R6 | R6B1_CUJ-655-1HB | Warden compiles 29+ million database entries and runs `conduit_assimilate` -- feeding accumulated knowledge into the Conduit's learning framework. |
| R6 | R6C2_FFW-HYY-6FL | ALT Schaeffer witnesses a Conduit in the field: a humanoid clone standing among creatures, reaching out to mercenaries, killed in the ensuing chaos. "It was trying to communicate." |
| R6 | R6D1_8AA-FEE-EDD | The Conduit conversation with ALT Schaeffer. The Collectors speak through the Conduit, revealing the truth about the Warden, the virus, the membranes, and humanity's need to escape. The Conduit dies during the exchange. |

## Notable References

> "A biological conduit for humans. This unit will speak for us."
> -- The Conduit, identifying its purpose to Schaeffer (R6D1_8AA-FEE-EDD)

> "It was a human, approximately. It was standing in the middle of a mass of creatures, and they weren't attacking it. [...] The clone clearly didn't want to hurt the mercenaries. It was trying to communicate. It was weak and looked like it was dying."
> -- ALT Schaeffer, witnessing the Conduit for the first time (R6C2_FFW-HYY-6FL)

> "There is a subroutine in the Warden logs called 'conduit', which has been trying to communicate with the mercenaries, but it's been killed every time. Then the program runs again, and it gets killed again, either by the mercenaries or the creatures. But some intelligence is driving this. It is not AI."
> -- Schaeffer, analyzing the Warden's repeated attempts to build and deploy Conduits (R5B3_F12-KLU-YN2)
