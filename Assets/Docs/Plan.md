# DOTween UI Transition Package Plan

## Current Goal
- Migrate the legacy `EasyTween` UI controller runtime from custom frame tweening to DOTween while preserving the existing inspector, serialized scene data, and drag-and-drop workflow.
- Use this project as the integration test bed before extracting the system into an embedded UPM package.

## Task Board
- [x] Create persistent task log at `Assets/Docs/Plan.md`.
- [x] Replace custom `CurrentAnimation` runtime with DOTween sequences.
- [x] Add DOTween ease selection while keeping animation curve compatibility.
- [ ] Preserve editor buttons and current inspector workflow.
- [ ] Add reusable package scaffold and package documentation.
- [ ] Verify compile/runtime behavior in Unity.

## Working Instructions
- Complete one task-board step at a time.
- After each step is completed, stop and report the exact files changed, what to check in Unity, and any known risk.
- Wait for user confirmation before starting the next step.
- Keep this file updated after each completed step so a later session can resume without losing context.

## Session Log
- 2026-04-25: Started implementation on branch `ui-controller-update`.
- 2026-04-25: Confirmed DOTween exists under `Assets/Plugins/Demigiant/DOTween` and project UI usage is currently driven by `EasyTween`, `UITween`, `UIScreen`, and `EditorUITween`.
- 2026-04-25: Added checkpoint workflow: pause for user confirmation after every completed task-board step.
- 2026-04-25: Migrated `CurrentAnimation` execution from manual frame-counter interpolation to DOTween sequences while keeping `EasyTween` and serialized data names intact.
- 2026-04-28: Added inspector controls for choosing AnimationCurve or DOTween Ease per enter/exit tween on fade, position, scale, and rotation.
- 2026-04-28: Investigating runtime snap-to-end issue reported during panel testing; sequence construction now pauses until all child tweens are joined and each tween receives an explicit start value.
- 2026-04-28: Temporarily tested a single `Animation Delay (Sec)`, then disabled it. Later option is direction-specific `Start Delay` for intro/open and `Exit Delay` for close/exit.
- 2026-04-28: Panel testing confirmed the snap-to-end issue is no longer happening after switching DOTween to drive normalized progress while legacy interpolation applies values.

## Decisions
- Keep the `EasyTween` component name during the first migration pass to avoid breaking scene and prefab script references.
- Keep existing serialized field names and animation data classes wherever possible.
- Default new easing controls to animation curves for backward compatibility.
- Do not require Coplay/Unity MCP for this migration.

## Known Issues
- Legacy fade runtime ignored configured start/end fade values and directly mapped tween percentage to alpha. DOTween runtime now uses configured start/end fade values; Unity validation still needed.
- `UIScreen.CanvasOff` uses `Task.Delay`, which is wall-clock based and not Unity time-scale aware.
- Embedded package extraction should happen after the migrated behavior is validated in this project.
- Later option: add `Start Delay (Sec)` before intro/open tween and `Exit Delay (Sec)` before close/exit tween. Keep defaults at `0`.

## Next Session Resume Point
- Validate the inspector ease controls in Unity. If clean, commit this checkpoint and continue with editor workflow verification and package scaffolding.
