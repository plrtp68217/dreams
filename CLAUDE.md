# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

DREAMS is a 2D pixel-art side-scroller platformer (Unity **6000.2.4f1**, URP 2D, C#). The heroine Eva chases a rabbit, hides from the searchlight of the "Eye" enemy, and reads notes/dialogs. Docs, TODO, and commit messages are in Russian; `TODO.md` holds the backlog and post-beta (v0.0.1) bug list (☑ = done).

## Build / run / test

There is no CLI build script, linter, or test suite (no test assemblies exist, though `com.unity.test-framework` is installed). Work happens in the Unity Editor:
- Open the project in Unity Hub with editor 6000.2.4f1; play from `Assets/Scenes/MainMenu.unity`.
- Build Settings scenes: `MainMenu` (index 0), `NightField` (index 1). `FastFood.unity` is a work-in-progress level not yet in the build list. `SceneController.StartScene(int)` loads by build index, and `SceneName` enum mirrors that order — keep them in sync when adding scenes.
- The `unity-cli` skill can drive a running Editor (compile, run C#, edit scenes) if needed.
- All gameplay scripts compile into the default `Assembly-CSharp` (no `.asmdef` files).

## Architecture

Scripts live in `Assets/Scripts/`. Wiring is done via `[SerializeField]` references set in the Inspector — not DI, not `FindObjectOfType`. Changing a serialized field name or a component's dependencies breaks references in scenes/prefabs (`Assets/Prefabs/Player.prefab`, `CAMERA.prefab`); use `[FormerlySerializedAs]` when renaming.

- **Services (`Services/`)** are plain `MonoBehaviour`s placed in the scene and injected into consumers: `InputService` (legacy `Input.GetKey*` polling, exposes per-frame flags like `EIsPressed`, `ControlIsHolding`; `Block()`/`Unblock()`/`BlockWithDelay()` suppress movement+jump input), `MovementService` (sets `Rigidbody2D.linearVelocity` X/Y), `CheckpointService` (list of reached checkpoint transforms), and fade helpers `AudioService`/`GraphicService`/`SpriteService`/`CanvasGroupService`. Player Settings uses "Both" input handling; the new Input System actions asset exists but gameplay input goes through `InputService`.
- **Entities**: `AEntity` (abstract, implements `IDamageable`) holds cached components (fetched in `Start`), `IsOnGround`/`IsAlive`/`IsInShelter`/`IsInvincible`, and the `Died` event. `Player` and `Rabbit` derive from it. `Player` does its own ground check (CircleCast) and tracks the `ActiveDialog` so it can be hidden on death.
- **Player flow**: `PlayerController` reads `InputService` → `MovementService`. `PlayerDeathHandler` subscribes to `Player.Died`, blocks input, hides the active dialog, waits, then respawns at `CheckpointService.GetLastCheckpoint()` with brief invincibility.
- **Units use enum state machines**, switched in `Update`/`FixedUpdate` (`RabbitState`, `EyeState`). State changes come from trigger components placed in the level (`RabbitTrigger` sets a configured `RabbitState` after a delay when a collider with a given `CustomTag` enters; `EyeTrigger` for the Eye). `Eye` raycasts along its beam and calls `Die()` on any `AEntity` not `IsInShelter`.
- **Triggers are the level-scripting layer**: `InteractionTrigger` (show `DialogUI` prompt on enter, `E` toggles `DialogCanvas` with localized text and blocks input), `ShelterTrigger` (hold Ctrl to hide: blocks input, sets `IsInShelter`, fades sprite), `CheckpointTrigger`, `CameraTrigger`, `SceneEndTrigger`, `JoinTrigger`, `TriggerWithSound`.
- **Dialogs/localization**: `Dialog` is the abstract base (`Enable(text)`/`Disable()`) for `DialogUI` and `DialogCanvas`. `LocalizedDialogueSystem` is a `DontDestroyOnLoad` singleton loading `Assets/Resources/dialogues.json` (`{dialogues:[{id, russian, english}]}`) and raises static `LanguageChanged`; UI text (menu TMP, buttons) also comes from these ids. Add new strings to that JSON with both languages.
- **Animator**: parameter names are referenced via the `AnimatorBool` / `AnimatorTrigger` enums using `.ToString()` — enum member names must exactly match Animator Controller parameter names. `UnitAnimatorController` / `PlayerAnimator` wrap the calls. Animations are imported from Aseprite.
- `CustomTag` enum values are compared against Unity tags (`CompareTag(_actor.ToString())`) and must match tags defined in Tag Manager.

## Conventions

- Private fields `_camelCase`, `[SerializeField] private`; explicit `== true/false` comparisons are common in existing code.
- **File encoding is mixed**: most `.cs` files are UTF-8 (with BOM), but `Player.cs`, `InputService.cs`, `JoinTrigger.cs`, `BrokenFlickerLight.cs`, `UI/ButtonChangeLanguage.cs` are Windows-1251 (Russian comments/headers). Preserve each file's encoding when editing so Cyrillic isn't corrupted; new files should be UTF-8.
- `Assets/OutlineEffect/` is third-party; `Assets/_Recovery/` is Unity scene-recovery output.
- Commit message style: a tag line (`[ADD]`, `[UPDATE]`, `[FIX]`) followed by a tab-indented hierarchical bullet list (area → item → details), often in Russian.
