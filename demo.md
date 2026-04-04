# Vertical Slice

A 15–30 minute playable experience that proves the core pillars: combat + Feral Sense + dialogue + faction tension.

---

## Goal

Prove only this: *"This core loop is fun and worth expanding."*

---

## 1. Scope — One Location

**Area: Abandoned Medical Sector**

Fits the lore (player origin). Naturally includes labs, corridors, quarantine zones. Justifies Feral enemies.

**Layout — keep it simple:**

```
Cryo Room (Start)
    ↓
Medical Wing (Exploration + first combat)
    ↓
Quarantine Zone (Main objective)
```

Optional: 1 locked shortcut, 1 hidden room (reward)

Total: ~5–8 small rooms max

---

## 2. Core Gameplay Loop

1. Wake up (tutorial — learn movement, interact)
2. Explore → find weapon
3. Feral Sense triggers (warning before first combat)
4. First combat encounter (Ferals)
5. Discover logs (story — parasite experiments, "unstable subject")
6. Meet Resistance scout (dialogue + trust choice)
7. Enter quarantine zone
8. Major encounter
9. Make faction decision

---

## 3. Combat

**Player:**
- Movement (WASD)
- 1 melee weapon (pipe, tool)
- Optional: dodge roll OR stamina bar

**Enemy — 1 type only:**
- Feral: moves erratically, attacks in short bursts
- Optional variant: slower but tankier (easy tweak, same AI)

---

## 4. Feral Sense

The player's core passive ability. When Ferals are within a detection radius, the player receives a subtle warning — a visual or audio cue indicating danger nearby before enemies are visible on screen.

This gives the player time to prepare (switch stance, choose approach) and communicates that the protagonist is *different* from a normal human.

Implementation: trigger a UI/audio cue when any Feral enters a defined range. No complex logic needed for the vertical slice.

---

## 5. Dialogue System (Light Version)

1 NPC is enough for the slice.

**Resistance Scout:**
- Finds player after first combat
- Suspicious of you
- 2–3 choices per interaction
- No complex stats needed yet

Example choices:
- "What happened here?"
- "I'm leaving."
- "I can help."

Branching outcome: Trust → gives access to shortcut / Distrust → withholds info

---

## 6. Story (Self-Contained)

**Setup:** You wake in cryo. Logs hint at experiments, failures, "unstable subject."

**Discovery:** Dead scientist, corrupted data logs. Parasite integration experiments happened here.

**Objective:** Retrieve a data shard from the quarantine zone.

**Final choice:**
- Option A: Give data to Resistance scout → "We must destroy this"
- Option B: Keep data → "This is valuable"

This sets the tone for the full game.

---

## 7. Systems — Must Have

| System | Notes |
|--------|-------|
| Player movement | WASD top-down |
| Basic melee combat | 1 weapon, hit detection |
| 1 enemy AI | Feral — erratic movement, burst attack |
| Simple health system | |
| Feral Sense | Proximity warning (visual/audio cue) |
| Basic dialogue UI | Choice list, 1 NPC |
| Interact system | E key — logs, doors, NPCs |
| Scene transitions | Doors between rooms |

## Not Building Yet

Skill tree, inventory, multiple weapons, complex AI, multiple factions, environmental hazards.

---

## 8. Art Style

- 2D pixel-art, top-down
- Dark sci-fi, strong lighting contrast
- Use placeholder shapes / asset store packs — don't overthink for the vertical slice

---

## 9. Audio (High Impact, Low Cost)

Focus on: ambient hum, distant metal sounds, creature noises, subtle distortion cue for Feral Sense activation.

Audio will carry atmosphere hard.

---

## 10. Realistic Timeline

| Week | Focus |
|------|-------|
| 1 | Movement + camera + basic combat |
| 2 | Enemy AI + level blockout |
| 3 | Feral Sense + UI basics |
| 4 | Dialogue system + story elements |
| 5–6 | Polish + audio + bug fixes |

---

## Success Criteria

The vertical slice is successful if:
- It's playable start to finish
- Player understands: *"I am different"* and *"There is something wrong here"*
- Combat feels okay (not perfect)
- Feral Sense feels interesting and useful
