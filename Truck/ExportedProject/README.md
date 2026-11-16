# TruckMod — Indian Bikes Driving 3D vehicle mod (rebuilt)

Unity project rebuilt from the mod bundle `Truck.glb` (UnityFS, built with
**Unity 2021.3.43f1**). Open it in Unity 2021.3.43f1 (Android Build Support) or
build headlessly from the CLI — both regenerate the `rgs` asset bundle the game's
mod loader expects.

## Bundle identity (preserved)

- AssetBundle name: `rgs` (set on `Assets/outsidemods/prefab/rgs.prefab.meta`;
  the build script also tags every asset under `Assets/` into `rgs`)
- Container path inside the bundle: `assets/outsidemods/prefab/rgs.prefab` (do not rename/move)
- File type the in-game picker expects: `.glb` (rename the built bundle, e.g. `Truck.glb`)

## Contents

- `Assets/outsidemods/prefab/rgs.prefab` — vehicle prefab: truck with
  `CarControl`, `WheelCollider`s, steering wheel, doors with HingeJoints,
  exhaust smoke/sparks particles, headlights, engine/crash/door audio and an
  explosion rig (`modexplosionPrefab` + `ExplosionVehicle`).
- `Assets/Mesh/*.asset` — truck meshes as Unity-native Mesh assets (1:1 from bundle).
- `Assets/Material/`, `Assets/Texture2D/`, `Assets/AudioClip/` — materials, textures, OGG audio.
- `Assets/Shader/` — 3 shaders recovered from the bundle (`Custom_SuperBlurPostEffect`,
  `Projector_Light`, `UnityLibrary_2D_Effects_ColorCycle`).
- `Assets/GameObject/` — particle/impact prefabs (Spark, MetalJunk*, GlassImpact).
- `Assets/Scripts/` — 16 IL2CPP-derived field-complete stubs exported from the bundle
  (MonoScript GUIDs preserved, e.g. `CarControl.cs.meta` = `c6918fd37d0f8b51664a692e8cae4c1a`)
  plus `DummyTypes.cs` (placeholder classes/enums the stubs reference, e.g. `ControlType`)
  — method bodies are empty by design; the game supplies the real implementations at runtime.
- `Assets/Editor/BuildModBundle.cs` — menu **Modding → Build "rgs" Bundle (Android)**
  and the batch-mode entry `BuildModBundle.BuildFromCommandLine`.
- `ProjectSettings/` — pinned to Unity 2021.3.43f1; `TagManager.asset` defines the
  `unknown_20004` tag the prefab references. `Packages/manifest.json` lists only
  built-in modules (no ugui), matching the stub `UnityEngine.UI.Text` used by `DummyTypes`.
- `TruckMod-DCC/` (repo root, outside the project) — meshes as OBJ for Blender/etc.

## Build (GUI)

Open the project in Unity → menu **Modding → Build "rgs" Bundle (Android)**.
Output: `<project>/Build/Android/rgs`.

## Build (CLI)

```bash
UNITY="$HOME/Unity/Hub/Editor/2021.3.43f1/Editor/Unity"   # adjust for your install
"$UNITY" -batchmode -quit -nographics \
  -projectPath /path/to/TruckMod \
  -buildTarget Android \
  -executeMethod BuildModBundle.BuildFromCommandLine \
  -logFile /path/to/TruckMod/build.log
```

Rename the built `rgs` to `Truck.glb`, copy it to the device, and load it
in-game via the mod menu (the picker filters on `.glb`).

## Swap in your own truck

1. Model your truck (keep the root at the vehicle center; wheels need WheelColliders).
2. Replace the vehicle visuals under the prefab's vehicle node, or re-parent your
   own mesh, keeping the WheelCollider GameObjects, `centerOfMass`, camera nodes
   (`Cam`, `InteriorCam`), `SitPosL` and the script components.
3. Keep or replace the tires (`wheel_FL/FR/RL/RR` meshes + WheelColliders).
4. Rebuild (commands above), rename to `*.glb`, install, load in-game.

Legal note: extracted assets are the intellectual property of the game developer;
keep this project personal — do not redistribute the extracted assets or mod bundles.
