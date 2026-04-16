# Repository Guidelines

## Project Structure & Module Organization
This repository is a Godot 4.6 farming game. `project.godot` defines the project entry, and the playable entry scene is `res://menus/main/MainMenu.tscn`. `Game.tscn` is the master gameplay scene orchestrated by `Game.gd`. Core gameplay scripts live in `areas/`, `player/`, `save_load/`, `sound/`, `shaders/`, and `grid/`. UI and menu scenes are split between `ui/` and `menus/`. Art, tiles, and audio assets live alongside their related scenes in folders such as `tilesets/`, `sound/`, and `Boot Splash and Icon/`.

## Build, Test, and Development Commands
Use Godot 4.6 for all work.

- `godot4.6 --editor project.godot` opens the project in the editor.
- `godot4.6 --path .` runs the game from the configured main scene.
- `godot4.6 --headless --path . --quit` is a quick import/config sanity check for CI or local verification.
- `godot4.6 --headless --path . --export-release "Windows Desktop" build/HarvestMoon.exe` exports using `export_presets.cfg`.

There is no separate build system, package manager, linter, or scripted test runner in this repository.

## Coding Style & Naming Conventions
Write GDScript with tabs or 4-space-equivalent indentation, matching the file you edit. Use `PascalCase` for scene and script filenames (`Player.gd`, `MainMenu.tscn`), `snake_case` for functions and variables, and keep node paths stable because save/load code depends on them. Prefer small, scene-local changes over cross-scene rewrites. Do not change TileMap IDs casually; crop growth, dirt state, and save data rely on those numeric values.

## Testing Guidelines
This project currently has no dedicated automated test suite. Validate changes in the Godot editor and in a playable run. For gameplay changes, verify area transitions, inventory/hotbar sync, save/load behavior, and affected UI scenes. If you add tests later, place them in a top-level `tests/` folder and name files after the covered feature, such as `tests/test_save_load.gd`.

## Commit & Pull Request Guidelines
Recent history uses short English summaries such as `convert to godot4` and `Updated README`. Keep commits short, imperative, and specific, for example `Fix farm crop hydration reset`. Avoid vague date-only messages. PRs should include a brief gameplay summary, affected scenes/scripts, linked issues, and screenshots or GIFs for UI or visual changes. Call out save compatibility risks and any `.tscn` format conversion.

## Contributor Notes
Prefer editing scenes in the Godot editor instead of hand-editing `.tscn` files. `GameManager` is an autoload singleton from `save_load/GameManager.gd`; treat path changes and serialization changes as high risk.
