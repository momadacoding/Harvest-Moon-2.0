# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Harvest Moon 2.0 — an open-source farming simulation game, recently converted from Godot 3 to **Godot 4.6**. Written entirely in GDScript.

## Running the Project

- Open with **Godot 4.6** editor and press F5
- Main scene: `res://menus/main/MainMenu.tscn`
- No build step, test framework, or linter — all development is done through the Godot editor

## Architecture

**Single root scene pattern:** `Game.tscn` is the master scene containing all areas, player, UI, sounds, shaders, and menus as children. The player node is reparented between areas on transition.

```
Game.gd (orchestrator)
├── Farm/  — main gameplay area with Crops/Dirt/Junk tilemaps
├── House/ — indoor area, bed/sleep mechanics
├── Town/  — shop access, largest tilemap
├── Farm/Player/ — CharacterBody2D, reparented between areas
│   └── UI/ — Inventory, Hotbar, Dashboard, Energy Bar
├── Shaders/ — 4 ColorRect overlays for time-of-day (morning/afternoon/evening/night)
├── Sound/ — music + effects manager
└── Menus/ — Shop, Pause
```

**Autoload:** `GameManager` (`save_load/GameManager.gd`) — singleton for save/load, accessible globally.

### Key Design Patterns

- **Player reparenting:** On area transitions, `Game.gd` removes the Player from one area and adds it to another (e.g. `farm_to_house()`, `house_to_farm()`)
- **Grid-based movement:** 32x32 pixel tiles. Player snaps to tile centers. Movement validated via `Area.is_cell_vacant()`
- **TileMap IDs as game state:** Crop growth stages, dirt states, and junk types are all numeric tile IDs. This makes serialization straightforward
- **Hotbar mirrors inventory:** The hotbar duplicates the last row of the inventory and keeps both in sync
- **JSON save system:** GameManager serializes everything (world tiles, inventory, player state, time, weather, shaders) into a single JSON file per save

### Crop Growth (Farm.gd)

Crops advance one stage per day if watered. Tile IDs:
- Turnips: 0→1→2→3→4→5
- Strawberries: 30→31→32→33→34→35
- Eggplants: 12→13→14→15→16→17

Dirt states: -1 (untilled), 0 (tilled/dry), 2 (watered)

### Time System (TimeManager.gd)

60-day cycle across 4 seasons (15 days each). Hours run 6AM→11PM. Shader tweens handle visual transitions. At 11PM, a sleep signal forces the player to bed.

## Godot 3→4 Conversion Issues

This project was recently converted from Godot 3. Several scene files still use `format=2` (Godot 3 format):
- `menus/controls/Controls.tscn`
- `menus/graphics/Graphics.tscn`
- `menus/main/back to main menu button/Back to Main Menu.tscn`
- `menus/dialogue/Dialogue.tscn`
- `Game.tscn`

**Common conversion pitfall:** When a `format=2` scene is instanced inside a `format=3` scene, the Godot 4 editor may override anchors to 0, causing panels to have zero size. Fix by converting the source scene to `format=3` or setting size/anchors in `_ready()`.

## Key Files

| File | Lines | Role |
|------|-------|------|
| `Game.gd` | ~115 | Area transitions, global constants (`tile_size = Vector2(32,32)`) |
| `player/Player.gd` | ~1160 | Movement, tools, animations, power moves |
| `areas/Farm.gd` | Large | Crop growth, tilling, harvesting, junk spawning |
| `save_load/GameManager.gd` | ~530 | Save/load all game state to JSON |
| `ui/inventory/Inventory.gd` | — | Drag-drop, stacking, shift-click swap |
| `shaders/Shaders.gd` | — | Time-of-day tween system |

## Editing Guidelines

- **Never edit `.tscn` files blindly** — understand the `format` version (2 vs 3) and `layout_mode` (0=Position, 1=Anchors) before changing layout properties
- **Tile IDs are meaningful** — changing tileset IDs breaks crop growth logic and save compatibility
- **Player node path changes break saves** — GameManager uses hardcoded paths like `get_node("/root/Game").player`
- **Area transition functions must stay symmetric** — every `X_to_Y()` must have a matching `Y_to_X()` that undoes shader/sound/weather changes
