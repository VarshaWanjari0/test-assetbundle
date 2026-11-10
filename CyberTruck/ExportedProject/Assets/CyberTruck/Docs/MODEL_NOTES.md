# CyberTruck model notes

Source: the uploaded `CyberTruck.fbx` — one welded mesh, 6 337 verts / 3 623 triangles,
6 material slots, no textures (flat Phong colours), authored `−X` forward, `+Y` up,
`+Z` right, about 4× real-world scale.

**Nothing about the shape was changed.** This pass only makes the asset usable by the
game's vehicle rig.

## 1. Axes and scale

| | source | shipped |
|---|---|---|
| forward | −X | **+Z** |
| up | +Y | +Y |
| right | +Z | **+X** |
| scale | 22.74 units long | **×0.2459 → metres** |
| length / width / height | 22.74 / 8.96 / 7.29 units | **5.591 / 2.177 / 1.792 m** |
| wheelbase | 14.63 units | **3.598 m** |
| wheel radius | 1.83 units | **0.468 – 0.477 m** |
| tyres | — | **sit on y = 0** |
| origin | model space | **axle midpoint, on the ground** |

The front was identified from the mesh itself: the full-width **white** light bar is at
−X, the full-width **red** bar at +X, and the overhangs come out 0.90 m front / 1.09 m
rear — exactly a Cybertruck's, only if the front is at −X.

Vertex data is written **mirrored in X** on purpose: Unity's FBX importer converts from
right-handed (FBX) to left-handed (Unity) by negating X, so the file is authored so that
after that conversion the car faces +Z with its right side at +X. (If your importer has
*Bake Axis Conversion* enabled, turn it off or rotate the prefab 180° on Y.)

## 2. Wheels

The source is a single welded mesh, so the four wheels were cut out by a cylinder test
around each measured hub (radius 1.95 units, outboard of the arch liner) and re-centred on
their own origins, axle along local X:

| mesh | tris | hub (m) | radius |
|---|---|---|---|
| `Wheel_FR` | 362 | (0.831, 0.487, 1.799) | 0.476 |
| `Wheel_FL` | 393 | (−0.869, 0.463, 1.799) | 0.468 |
| `Wheel_RR` | 393 | (0.899, 0.463, −1.800) | 0.468 |
| `Wheel_RL` | 393 | (−0.860, 0.473, −1.799) | 0.477 |
| `CyberTruck_Body` | 2 082 | — | — |
| **total** | **3 623** | | same as the source |

No wheel geometry was added or removed; the only per-triangle change is the tyre/rim
material split (inner 62 % of the radius = rim).

## 3. Materials

The source slots had no useful names (`…untitledglass…`, `…sid001…`, `…bodyshell001…`,
`…indicatorrf1…`, `Part1Mtl`, `…headlightl001…`) and no textures. They were renamed and
re-tuned; the geometry that used each slot is unchanged:

| slot | used on | shipped material | look |
|---|---|---|---|
| big outer panels (314 tris) | body shell | `CyberTruck_Body` | brushed stainless, metallic 0.85, smooth 0.72 |
| remaining details (3 239) | sills, arches, bumper parts, interior, roof panel | `CyberTruck_Trim` | dark charcoal, 0.048 / smooth 0.35 |
| front light bar (12) | + indicators (4) | `CyberTruck_Light` | emissive white (2.6, 2.35, 1.70) |
| rear light bar (36) | — | `CyberTruck_Tail` | emissive red (2.2, 0.15, 0.10) |
| wheels | outer 62 % | `CyberTruck_Tire` | near-black rubber |
| wheels | inner 38 % | `CyberTruck_Rim` | gunmetal, metallic 0.9 |

No colour is baked into the mesh — repaint by editing `CyberTruck_Body.mat` (or the eight
preset colours in `color_select` on the prefab).

## 4. Landmarks measured from the mesh

Roof peak at `z ≈ 0.10`, roof height 1.79 m, greenhouse from `z ≈ −1.0` to `+0.6`, front
light bar at `z ≈ 2.40` / 1.02 m high, rear bar at `z ≈ −2.75`.

Seat `SitPosL` (−0.42, 0.88, 0.20) · eye `InteriorCam` (−0.42, 1.32, 0.42) · steering
(−0.42, 1.04, 0.82) · door `DoorPos` (1.03, 0.95, 0.20) · 3rd-person pivot `Cam`
(0, 1.45, 0.30) with `CamDis` 7.

Rebuild with `work/make_model_cb.py` (model) → `work/make_project.py` (skeleton) →
`work/make_prefab.py` (prefab) → `work/docs_render_cb.py` (these images); then run
`CyberTruck ▸ 1` in Unity so the mesh assets and materials are re-wired.
