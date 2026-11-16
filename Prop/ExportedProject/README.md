# PumpkinBoyMod — Indian Bikes Driving 3D character mod (rebuilt)

Unity project rebuilt from the mod bundle `pumpkinBoy.glb` (UnityFS, built with
**Unity 2021.3.43f1**). Open it in Unity 2021.3.43f1 or build headlessly from the
CLI — both regenerate the `rgs` asset bundle the game's mod loader expects.

## Bundle identity (preserved)

- AssetBundle name: `rgs` (set on `Assets/outsidemods/prefab/rgs.prefab.meta`;
  the build script also tags every asset under `Assets/` into `rgs`)
- Container path inside the bundle: `assets/outsidemods/prefab/rgs.prefab` (do not rename/move)
- File type the in-game picker expects: `.glb` (rename the built bundle, e.g. `PumpkinBoy.glb`)

## Contents

- `Assets/outsidemods/prefab/rgs.prefab` — character prefab ("pumbkin men",
  Sketchfab `b3c4971a…fbx`, pumpkin-headed boy), skinned mesh + `ModMe` script,
  used from the in-game character/player mod slot.
- `Assets/Mesh/Object_7/8/9/10/12.asset` — skinned meshes (`_rootJoint` / `Bip001` hierarchy).
- `Assets/AnimationClip/Object_4|Take 001_Object_4.anim` — the one animation clip.
- `Assets/AnimatorController/pumbkin men.controller` — animator referenced by the prefab.
- `Assets/Material/Image_0/3/10.mat` + `Assets/Texture2D/Image_0/3/10.png` — materials/textures.
- `Assets/Scripts/Assembly-CSharp/ModMe.cs` — the mod component (`public bool Scale;`),
  field-complete stub; MonoScript GUID preserved from the bundle.
- `Assets/Editor/BuildModBundle.cs` — menu **Modding → Build "rgs" Bundle (Android)**
  and the batch-mode entry `BuildModBundle.BuildFromCommandLine`.
- `ProjectSettings/` — Unity 2021.3.43f1; `TagManager.asset` (standard tags + `unknown_20004`).
  `Packages/manifest.json` lists only built-in modules.
- `PumpkinBoyMod-DCC/` (repo root, outside the project) — meshes as OBJ reference.

## Build (GUI)

Open in Unity → menu **Modding → Build "rgs" Bundle (Android)**.
Output: `<project>/Build/Android/rgs`.

## Build (CLI)

```bash
UNITY="$HOME/Unity/Hub/Editor/2021.3.43f1/Editor/Unity"   # adjust for your install
"$UNITY" -batchmode -quit -nographics \
  -projectPath /path/to/PumpkinBoyMod \
  -buildTarget Android \
  -executeMethod BuildModBundle.BuildFromCommandLine \
  -logFile /path/to/PumpkinBoyMod/build.log
```

Rename the built `rgs` to `PumpkinBoy.glb`, copy it to the device, and load it
in-game via the mod menu.

## Swap in your own character

1. Import your humanoid FBX (Humanoid rig recommended; the game maps hands/feet IK).
2. Replace the SkinnedMeshRenderer meshes on the prefab — keep the `ModMe`
   component and the animator setup.
3. Rebuild (commands above), rename to `*.glb`, install, load in-game.

Legal note: extracted assets are the intellectual property of the game developer;
keep this project personal — do not redistribute the extracted assets or mod bundles.
