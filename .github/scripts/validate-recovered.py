import sys, UnityPy
from collections import Counter
path, kind = sys.argv[1:]
e = UnityPy.load(path)
expected = 'assets/outsidemods/prefab/rgs.prefab'
print('Bundle asset names:', list(e.container.keys()))
if expected not in e.container:
    raise SystemExit('Expected prefab address is missing: ' + expected)
counts = Counter(o.type.name for o in e.objects)
print('Object counts:', dict(sorted(counts.items())))
required = {'Truck': {'GameObject': 60, 'Mesh': 5, 'MonoBehaviour': 28}, 'Prop': {'GameObject': 85, 'Mesh': 5, 'MonoBehaviour': 1}}
for name, minimum in required[kind].items():
    if counts[name] < minimum:
        raise SystemExit(f'{kind}: expected at least {minimum} {name}, got {counts[name]}')
print('Static bundle checks passed. Runtime and 1:1 fidelity not established.')
