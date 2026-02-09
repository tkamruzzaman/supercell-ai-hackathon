# 🎮 LOCAL MULTIPLAYER ZONE CONTROL GAME (UNITY)

**This is a local multiplayer game.**  
Designed for **couch play**, shared screen, and fast competitive matches.

---

## 🧠 Project Overview

This project is a **top-down local multiplayer strategy game** built in **Unity** where two players compete to control zones using followers.

Players move around the map, deploy followers into capture zones, and fight for territory.  
Combat, capture, and scoring all happen **in real time on a single screen** — no networking involved.

---

## 🕹️ Core Features

- **Local Multiplayer (2 Players)**
  - Same screen
  - No online networking
  - Designed for controllers or keyboard input

- **Zone Control Gameplay**
  - Neutral → Capturing → Contested → Locked states
  - Zones generate points over time
  - Zones visually react to combat, capture, and locking

- **Follower-Based Combat**
  - Followers have health, capture speed, and combat stats
  - Combat is tick-based (Clash Royale–style)
  - One unit cannot be killed multiple times in the same tick
  - Deterministic combat resolution

- **Visual Feedback**
  - Capture progress bars
  - Combat animations when zones are contested
  - Clear visual cue when a zone is locked
  - Floating player indicators above characters

- **Match Flow System**
  - Match starts only when both players join
  - Match timer with TextMeshPro UI
  - Match ends when:
    - Time runs out, or
    - All zones are locked
  - Winner determined by:
    1. Number of locked zones
    2. Capture points as tie-breaker

---


---


---

## 🚧 Project Status

This project is currently a **prototype / gameplay-focused build**.
Art, polish, and sound are intentionally minimal to prioritize mechanics.

---

## 📌 Notes

- Built for **local multiplayer first**


