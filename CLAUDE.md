# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Status

This project is in **early production**. Design documents and initial scripts exist:
- `overview.md` — full game concept and design document
- `demo.md` — vertical slice (MVP) specification
- `Scripts/` — Unity C# scripts, pulled from Unity after each session; always read before planning

Scripts are the source of truth for current implementation state.

## Game Concept

**Genre:** 2D true top-down (bird's eye) pixel-art RPG (combat + stealth + dialogue) — reference: Hotline Miami, Darkwood  
**Setting:** A centuries-old colony ship en route back to Mars, society fractured into three factions  
**Player:** A cryo human partially integrated with a parasitic organism — created by the Resistance as an experiment gone wrong

**Signature Passive — Feral Sense:** Player senses nearby Ferals before they're visible (proximity warning cue). Foundation of the player's unique survivability.

**Progression — Dual Skill Tree (future, not vertical slice):**
- Human side: utility, dialogue, exploration abilities; affects how factions perceive you
- Parasite side: combat-focused; more investment = more "feral" appearance and NPC reactions
- Future: environmental immunity (radiation, cold, heat) from parasitic symbiosis

**Three Factions (no clear good/evil):**
- **Devoted** — controls infrastructure/life support; wants to weaponize the parasite
- **Opportunists** — small elite with original parasite samples and combat robots; leverage-based power
- **Resistance** — saboteurs who hate the parasite but used it to create the player

## Vertical Slice Scope (demo.md)

The MVP targets a 15–30 minute playable experience in one location: **Abandoned Medical Sector**

**Layout:** Cryo Room → Medical Wing → Quarantine Zone (~5–8 rooms total)

**Must-have systems:**
- Player movement (WASD + mouse-aimed rotation, 360°)
- Field of View cone (light/shadow, drives stealth + atmosphere)
- Basic melee combat (1 weapon)
- 1 enemy type (Feral — erratic, burst attacks)
- Simple health system
- Feral Sense (proximity warning cue — fires regardless of FOV)
- Basic dialogue UI (2–3 choices, 1 NPC: Resistance scout)
- Interact system (E key)
- Scene transitions

**Explicitly out of scope for MVP:** skill tree, inventory, multiple weapons, complex AI, multiple factions, environmental hazards

## Architecture (When Implementation Begins)

**Engine:** Unity 6.3 LTS, 2D true top-down (bird's eye) pixel-art, real-time combat.  
**Player facing:** Mouse cursor, 360° rotation. No 4-directional sprite limitation — sprite is top-down view, rotates with the GameObject.

Core systems — keep them decoupled:

1. **PlayerController** — WASD movement, mouse-aimed rotation, melee attack
2. **FieldOfViewSystem** — raycasting FOV cone; controls a shadow/darkness layer outside the cone; core to stealth and atmosphere
3. **FeralSenseSystem** — passive proximity detection; triggers warning cue (visual/audio) when Ferals enter range, regardless of FOV
4. **EnemyAI** — Feral behavior (erratic movement, burst attack pattern); designed to be variant-extensible (stalker/brute/signaler)
5. **DialogueSystem** — choice tree with binary trust outcome affecting world state (shortcut access)
6. **InteractSystem** — E-key proximity interactions (logs, doors, NPCs)
7. **UIManager** — health bar, Feral Sense cue, dialogue box