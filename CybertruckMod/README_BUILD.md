# 🤖⚡ Indian Bikes — Cybertruck Mod (Android)

Cybertruck outside-mod AssetBundle project for **com.Rohit.IndianBikes** (Indian Bikes Driving 3D).
Based on the exported Truck AssetBundle project, re-fitted with the **Tesla Cybertruck** body (Sketchfab) and EV powertrain.

Built/tested target: **Unity 2021.3.43f1** + **Android Build Support** module.

---

## 📦 What's inside

```
CybertruckMod/
├─ README_BUILD.md              ← you are here
├─ ASSET_ATTRIBUTION.md         ← required model credit (CC-BY-4.0)
├─ AuxiliaryFiles/path_id_map.json   ← reference: original bundle CAB (untouched)
└─ ExportedProject/             ← ⭐ THIS is the Unity project — open it in Unity
   ├─ Assets/
   │  ├─ outsidemods/prefab/rgs.prefab   ← THE mod (root renamed "Cybertruck", file kept as rgs on purpose — see below)
   │  ├─ Models/cybertruck.glb           ← Tesla Cybertruck (Sketchfab, CC-BY-4.0)
   │  ├─ AudioClip/EV_Engine_Loop.wav    ← synthesized EV cruise hum (seamless loop)
   │  ├─ AudioClip/EV_Engine_Load.wav    ← synthesized EV acceleration whine (seamless loop)
   │  ├─ Editor/CybertruckModBuilder.cs  ← one-click builder menu (Tools ▸ Cybertruck)
   │  ├─ Mesh/ Door_Right, Cargodoor_left, Steering_wheel, wheel_FL/RL … (from Truck export)
   │  └─ Scripts/Assembly-CSharp/*.cs    ← ⚠️ DUMMY stubs only — mirror of game scripts,
   │                                       script GUIDs must never change
   ├─ Packages/manifest.json    ← + com.unity.cloud.gltfast 6.14.1 (GLB import)
   └─ ProjectSettings/
```

> ⚠️ The dummy `.cs` files in `Assets/Scripts/Assembly-CSharp/` are **not** game code.
> They exist so the prefab's MonoScript references (GUIDs in the `.meta` files) resolve when
> Unity builds the bundle. At runtime the bundle binds them to the *real* classes inside the
> game's Assembly-CSharp. **Do not delete, rename or regenerate their `.meta` files.**

---

## 🏎️ Specs (as requested)

| Parameter | Stock Truck | **Cybertruck Mod** |
|---|---|---|
| `topSpeed` | 200 | **5000** 🚀 |
| `reverseSpeed` | 40 | **500** |
| `maxTorque` | 10 000 | **60 000** |
| Gears (speedometer) | 50/100/150/200 | **800/1600/2400/3200/4000/5000** |
| Chassis mass | 4000 | 4000 (battery weight, planted) |
| Wheel colliders spring/damper | 35000/4500 | **65000/7000** |
| Suspension travel | 0.30 m | **0.22 m** |
| Wheel mass / damping | 45 / 0.25 | **60 / 0.4** |
| Forward friction stiffness | 1.0 | **1.4** 🛞 |
| Sideways friction stiffness | 1.0 | **1.5** 🛞 |
| Center of mass | y = +0.15 | **y = −0.35** (low & planted) |
| Angular drag | 1.0 | **2.5** (resists high-speed flips) |
| Collision detection | Discrete | **Continuous** 🛡️ (no tunnelling at 5000!) |
| Force application point | 0 | **0.05** (high-speed stability) |
| Engine audio | Jet Engine | **EV hum / EV whine** 🔊⚡ |
| Exhaust smoke | on | **off** (it's an EV!) |
| Player Protect box | kept | **kept** ✅ (player stays in the car at speed) |

**"Not high speed player fall"** → handled with: low center of mass + stiff suspension + huge
tire grip + high angular drag + continuous collision + the in-prefab **Player Protect** box
collider (the game's own anti-eject volume) + `Interpolate` rigidbody. RidingCar/door hinges
unchanged. 🚪

**"EV instead of fuel"** → the JetEngine/sound_002 clips were swapped for synthesized EV
whine loops, and the exhaust particle system is disabled. There is no fuel gauge in this game's
vehicle class (`CarControl` has no fuel field — checked the dump), so no further EV/fuel UI
changes are needed. 🔋

---

## 🛠️ Build steps (5 minutes, on your PC)

1. Install **Unity 2021.3.43f1** (Unity Hub) with the **Android Build Support** module
   (+ OpenJDK + Android SDK/NDK tools it offers).
2. Open Unity Hub → **Add → Add project from disk** → select `ExportedProject/`.
3. Let it import. glTFast (`com.unity.cloud.gltfast`) will appear in Package Manager — wait
   for it to resolve (needs internet once).
4. Menu bar → **Tools ▸ Cybertruck ▸ Build Mod (Android)**. This will:
   - attach `Assets/Models/cybertruck.glb` under the prefab's **truck** node,
   - re-apply every spec in the table above,
   - assign the EV clips to `EngineSound` / `EngineSound 2`,
   - assign the AssetBundle name `rgs`,
   - build the bundle to `ExportedProject/AssetBundles/Android/rgs`.
5. Copy `AssetBundles/Android/rgs` (the file, not the `.manifest`) into the game's
   **mods folder** — the same folder your Truck mod lives in (the game's `rgs` loader scans
   it; pak file naming must keep the bundle named `rgs`, the asset inside is `rgs.prefab`).
6. Launch the game → mods menu / cheat phone 📞 → spawn it → enjoy 😼

> 💡 **Why the file is still called `rgs.prefab` / bundle `rgs`:** the game's mod loader
> (`rgs` class in Assembly-CSharp) carries an `assetNameToLoad` field, and the only asset it
> ever loaded in the original bundle was the `rgs` prefab. Keeping the name guarantees the
> loader finds it. You can rename it **only if** you discover (via the APK strings / IL2CPP
> metadata) that your game version uses a different `assetNameToLoad`.
> The prefab's *root GameObject* **is** already renamed to **Cybertruck**.

### Manual tweak knobs (in the prefab Inspector)
- Aligning the body: select `rgs.prefab → carzyCar → truck → Cybertruck (Body)`
  → adjust `localScale` / `localPosition` so the mesh sits between the 4 wheel colliders.
  GLB is ~4.7 m long, the rig wheelbase is ~7.5 m, hence the default `1.6` scale.
- Want drift mode? drop sideways stiffness to ~1.0. Want a bus? bump `rb.mass`.

### Notes / gotchas
- The GLB has **no animations** — doors/steering use the game's own rig objects, which is what we want.
- The original Truck export was **missing its body mesh** (only doors/wheels/steering exist as
  `.asset` meshes — the Cybertruck *is* the body now, which is why this works out so well).
- Headlight sounds, glass-impact FX, `ExplosionVehicle` destruction, `color_select` in-game
  color picker — all still wired and functional. 🎨💥
- Requires internet on first Unity open for glTFast; afterwards it's cached.

## 📜 License of the model
Cybertruck model: **"Tesla Cybertruck" by PolyDucky** —
https://sketchfab.com/3d-models/tesla-cybertruck-1ab0c3f248fa47019c4311abce9731a0
License: **CC-BY-4.0** (attribution required — see ASSET_ATTRIBUTION.md). Keep that credit with any build you share.
