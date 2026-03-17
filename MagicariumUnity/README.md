# Magicarium – Unity Project

This folder contains the **Unity 2022.3 LTS** project for Magicarium, a turn-based RTS game.

## How to Open

1. Install **Unity 2022.3 LTS** (any 2022.3.x patch release) via Unity Hub.
2. In Unity Hub click **Open → Add project from disk** and select this `MagicariumUnity/` folder.
3. Unity will import all assets and compile the scripts (first import can take a minute).
4. Open `Assets/Scenes/Game.unity` in the Project window if it is not already open.
5. Press **▶ Play** to start the game.

> The scene contains only a Main Camera.  
> All game objects (canvas, map tiles, UI panels) are created at runtime by
> `GameBootstrapper` using `[RuntimeInitializeOnLoadMethod]` — no prefabs or
> manual scene setup needed.

---

## Controls

| Action | How |
|---|---|
| **Select a unit / building** | Click its tile on the map |
| **Move a unit** | Select it → click **Move** → click destination tile |
| **Attack** | Select a military unit → click **Attack** → click enemy tile |
| **Gather resources (worker)** | Select worker → click **Gather / Deposit** while on a resource tile |
| **Deposit resources (worker)** | Move worker to your HQ → click **Gather / Deposit** |
| **Build a building** | Click the desired **Build** button → click a passable tile |
| **Train a unit** | Click the desired **Train** button (requires Barracks for soldiers) |
| **End your turn** | Click **End Turn** (top-right) |

---

## Terrain Legend (map tiles)

| Colour | Label | Terrain |
|---|---|---|
| Dark green | *(empty)* | Grass |
| Darker green | `Fo` | Forest — contains **Wood** |
| Gray | `^^` | Mountain — impassable |
| Blue | `~~` | Water — impassable |
| Gold/yellow | `Au` / `Mo` | Mine — contains **Gold** or **Magic Ore** |
| Brown | `Rd` | Road — +50% unit movement speed |

---

## Entity Labels (map)

| Label | Entity |
|---|---|
| `W` (blue) | Your Worker |
| `S` / `A` / `K` (blue) | Your Soldier / Archer / Knight |
| `HQ` (blue) | Your Main Base |
| `Br` / `Fm` / … (blue) | Your other buildings |
| `w` / `s` / `a` / `k` (red) | Enemy units |
| `hq` / `br` / … (red) | Enemy buildings |

---

## Building Costs

| Building | Gold | Wood | Magic Ore |
|---|---|---|---|
| Farm | 50 | 30 | — |
| Barracks | 150 | 100 | — |
| Blacksmith Hall | 200 | 150 | 50 |
| Mill | 75 | 50 | — |
| Church | 100 | 80 | 20 |
| Well | 40 | 20 | — |
| Shrine | 80 | — | 30 |
| Field | 30 | 10 | — |

## Training Costs

| Unit | Gold | Magic Ore | Requires |
|---|---|---|---|
| Worker | 50 | — | Main Base |
| Soldier | 100 | — | Barracks |
| Archer | 80 | 20 | Barracks |
| Knight | 200 | 50 | Barracks |

---

## Passive Income (per turn)

- **Shrine** → +5 Magic Ore / turn each
- **Mill + Farm** → +2 Gold × (mill count × farm count) / turn

---

## Win Condition

Destroy the enemy's **Main Base** and all enemy units to win.  
The game declares a winner automatically and shows a Game Over screen with a **Restart** button.
