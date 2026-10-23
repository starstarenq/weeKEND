# SkillIndicatorLab

## Folder layout and export

```text
SkillIndicatorLab/
  Editor/       Lab window and Shader Graph generator (editor-only)
  Runtime/      Indicator, demo, damage target, DOTween damage UI
  Generated/    Graphs, materials, prefabs, scene, preview
  README.md
```

Copy this entire folder and its `.meta` files into another project's `Assets`, or select the folder in Unity and use **Assets > Export Package**. Review **Include dependencies** before exporting: shared third-party assets may also be selected. Existing script, folder, prefab, material, and scene GUIDs were preserved during relocation.

Destination requirements:
- Unity 6; source project version is 6000.4.2f1.
- Universal Render Pipeline and Shader Graph; source uses URP 17.4.0. Configure an active URP pipeline in the destination project.
- Unity UI (uGUI) and TextMesh Pro, including TMP Essential Resources/default font.
- DOTween installed and set up. The shared `Assets/Plugins/Demigiant` installation is not copied into this feature folder.

Open `Generated/Scenes/SkillIndicatorLab.unity` and enter Play mode to preview all four shapes and damage text. Use **Tools > Skill Indicators > Shader Graph Lab** to generate a new lab. Generation paths follow this folder even if it is moved under another folder in `Assets`; keep the editor script's `.meta` file. Existing generated content is preserved; subsequent generations use `Generated 1`, `Generated 2`, etc.

The Shader Graph generator uses internal Shader Graph APIs; compatibility with other package versions must be checked in the destination Unity editor. No card battle scripts are required.

## Skill Indicator damage

- Add `SkillDamageTarget` to the target root and a 3D Collider to that root or a child. Configure `maxHealth` and `armor` before play.
- Set `damage`, `hitDuration`, `hitHeight`, `targetLayers`, and optionally `owner` on `SkillIndicator`. Set `damageCamera` for multiple-camera scenes.
- Call `Trigger()` when the attack activates. It hits immediately and checks again during the hit window. Each target receives damage only once per activation, even with multiple colliders. `EndHitWindow()` cancels the window.
- Detection requires both a Collider overlapping the attack volume and the target root pivot inside the ground shape and local height range `[0, hitHeight]`. Donut centers and positions behind sectors are excluded. This is a pivot-based rule, not exact mesh intersection.
- Damage is `min(current HP, max(0, damage - armor))`. UI displays the actual HP removed; fully blocked attacks display `BLOCK`. Dead targets are ignored. `HealthChanged`, `Died`, and `DamageApplied` expose results to other systems.
- A screen overlay Canvas is created automatically. Text pops in, rises, and fades over one second, following the projected hit position as the camera moves. Popups clean up their tweens when destroyed.
- Existing Lab scenes work on Play: `SkillIndicatorDemo.createDamageTarget` creates a capsule target and restores it after death on the next attack. Disable that option for your own targets; disable the demo component when combat code drives the indicator.

This target component provides standalone HP for the indicator demo; it does not automatically map to the separate card battle `Combatant` data model.
