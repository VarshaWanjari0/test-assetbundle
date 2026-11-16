# Indian Bikes Driving 3D v91 — recovered mod projects

These are **best-effort recovered projects, not a verified 1:1 decompilation**. The two `.glb` inputs are UnityFS AssetBundles, not glTF files. The linked APK was inspected statically; it was **not installed or executed**.

## What changed after inspecting the APK

* The APK identifies itself as `com.Rohit.IndianBikes`, version name/code **91**, and reports **Unity 6000.4.9f1**, IL2CPP, Android. Both original mod bundles report **Unity 2021.3.43f1**, Android. This mismatch matters; whether a bundle built with either editor works in this game must be tested on-device.
* All 17 MonoScript identities used by the two bundles (16 truck classes and `ModMe` for pumpkin) have matching class names, namespaces and `Assembly-CSharp` identity in v91. This is an identity check, **not proof** of matching behavior, serialized type hashes, or successful loading.
* The APK's TagManager maps tag ID **20004 to `Vehicle`** and layer **9 to `Car`**. The truck prefab's AssetRipper placeholder `unknown_20004` has been replaced with `Vehicle`, and both projects have the v91 tag/layer names. This was cross-checked against an in-game truck object with tag ID 20004 and `Vehicle`. The full v91 list is included in `Reference/Game_v91_metadata/TagManager_export_v91.yaml.txt`. The project TagManager is adapted to Unity 2021's format, not an unmodified original project file.
* Cpp2IL extracted v91 type metadata. The editable stubs now contain the public field names and types found for the 16 truck scripts and `ModMe`, including `CarControl.speedText` and `RandomAudioClip.onlyClipChange`, which were present in game metadata but absent from the respective original bundle type trees. The latter two default to null/false unless edited. `com.unity.ugui` was added for `speedText`.
* The APK contains `.glb`, `rgs`, `rgscm`, `rgsp` and related text literals; low-level loader analysis identifies UnityWebRequest AssetBundle APIs. This is consistent with the sample bundle address but does not establish every install path or loader rule.

## Choose a project pair

* `TruckProject` and `PumpkinProject`: source-version projects, pinned to **2021.3.43f1**. These preserve the original export version and are the comparison baseline.
* `TruckProject_Unity6_EXPERIMENTAL` and `PumpkinProject_Unity6_EXPERIMENTAL`: copies with `ProjectVersion.txt` set to **6000.4.9f1** to match APK v91. They have **not been imported or migrated by Unity**. The truck copy requests `com.unity.ugui` 2.0.0. Open a copy in the matching Unity editor and let Unity migrate the serialized assets; review all import errors and changed assets. Merely changing the project version does not prove successful migration.

Keep the truck and pumpkin projects separate: both contain the prefab at `Assets/outsidemods/prefab/rgs.prefab` and use the same address in their respective bundles. Keep the unchanged example bundles for rollback.

## Critical limitations

1. The APK uses IL2CPP. Cpp2IL's `diffable-cs` output here reconstructs **type and field metadata with empty method bodies**; it did not recover the original C# implementations. The Unity projects intentionally contain schema-only placeholder MonoBehaviours, not real car/game logic. Their Play Mode cannot reproduce the game. The game may or may not resolve a rebuilt bundle to its own compiled game scripts: **not tested**.
2. The supplied dump and APK metadata permit a stronger field comparison but do not recreate original editor sources, original full project settings, stripped data, material/shader source, or guarantee matching serialization layouts. Reference metadata and low-level Cpp2IL ISIL are provided for inspection only, not as working C#.
3. Some custom shaders from AssetRipper are dummy shaders. Appearance may differ. The pumpkin animation, bone references, mesh compression, material behavior and compatibility need visual and runtime testing. A zero *uncompressed* vertex count in the bundle is not sufficient to conclude a compressed mesh is empty.
4. **No Unity Editor or Android device was available in this environment.** None of the four projects has been opened/compiled/built in Unity; neither original nor rebuilt bundle has been loaded into game v91. The build scripts and Unity 6 migration remain unverified. GUID checks and metadata comparisons cannot substitute for those tests.
5. This is a third-party modified APK from the supplied link. Its origin and safety are not verified; no claims are made about it matching the official store release. Use your own judgment before installing it.

## Open, edit and build in the GUI

1. Install the matching project editor version via Unity Hub, including **Android Build Support**. Extract this entire archive. Add one project folder as an existing project. For the Unity 6 experiment, first make a separate backup; Unity will migrate files on import. Allow import to finish and inspect the Console.
2. Open `Assets/outsidemods/prefab/rgs.prefab` in Prefab Mode. Preserve the root `rgs`, prefab path, needed component types and references. Project `Assets/Mesh` contains the recovered native Unity meshes; textures and materials are in their corresponding folders. Avoid deleting colliders, wheel colliders, rigidbodies, scripts or attachment transforms when replacing only visuals.
3. **Truck:** import your FBX/mesh and textures. Replace meshes/materials on the existing hierarchy, adjust scale and pivot, and reconnect wheel meshes, wheel colliders, doors, seats, triggers and references as needed. The `Cargodoor_left` mesh is large and has five submeshes; inspect it before assuming it is just a door. Game behavior cannot be checked in this project's Play Mode.
4. **Pumpkin / prop:** replace or add visual children under the root and keep the `ModMe` component and suitable collider. The example is skinned and animated: an animated replacement needs compatible bones, skinned mesh references, controller and animations; a static prop should be set up appropriately instead of blindly reusing the rig.
5. Choose **Mods → Build Android bundle…** and save outside `Assets` with a `.glb` filename. The builder uses Android, bundle name `rgs`, LZ4 compression, and address `assets/outsidemods/prefab/rgs.prefab`, matching the example bundles' exported address. This is an AssetBundle even with the `.glb` extension. Test with a backup on the exact game version; actual loader behavior is not fully established.

## Build from the Unity CLI

Install the relevant exact editor version and Android module. Close the GUI for the project before invoking the CLI. Example using the **2021 baseline**; substitute the Unity 6 executable and corresponding `_Unity6_EXPERIMENTAL` folders to test that variant:

```sh
UNITY=/absolute/path/to/Unity
ROOT=/absolute/path/to/extracted/folder
"$UNITY" -batchmode -nographics -quit -projectPath "$ROOT/TruckProject" -buildTarget Android -executeMethod ModBundleBuilder.BuildFromCommandLine -modOutput "$ROOT/output/Truck.glb" -logFile "$ROOT/truck-build.log"
"$UNITY" -batchmode -nographics -quit -projectPath "$ROOT/PumpkinProject" -buildTarget Android -executeMethod ModBundleBuilder.BuildFromCommandLine -modOutput "$ROOT/output/pumpkinBoy.glb" -logFile "$ROOT/pumpkin-build.log"
```

On Windows, use the `Unity.exe` path and absolute Windows paths. Check process exit code and Editor log. The builder rejects missing Android support, missing prefab or missing prefab scripts, and copies the result to the requested output filename, overwriting it if it exists. This does not detect all invalid references or establish game compatibility. Intermediate builds go under the project's `Library/RecoveredModBundleBuild`.

## Reference files

* `Reference/Truck.glb`, `Reference/pumpkinBoy.glb`: unchanged originals. `game_type_dump.cs.txt`: previously supplied game type dump, not source code.
* `Reference/Game_v91_metadata/*.cs.txt`: selected Cpp2IL diffable type metadata, with **empty method bodies**, not compilable restored game code. `*.ISIL.txt` is automated low-level analysis; it can have unresolved calls or translation errors. The JSON records MonoScript identity comparisons.
* `Reference/Truck_OBJ/`: per-mesh OBJ geometry exported with UnityPy and sanity-checked with trimesh. These are not complete prefab/model exports: they omit original hierarchy, prefab transforms, material assignments and animation, and UnityPy changes handedness. Use the recovered Unity prefab as the authoritative reference.
* `AUDIT.txt`: source hashes, object counts and checks. APK itself and its binaries are not included.
