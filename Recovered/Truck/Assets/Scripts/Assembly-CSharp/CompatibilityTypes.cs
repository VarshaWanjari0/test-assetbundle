// Schema-only references needed to compile the recovered prefab. No game logic.
using UnityEngine;
public enum ControlType { Touch = 0, Steering = 1, Joystick = 2 }
public enum explosionType { ExpCar = 0, ExpBike = 1, ExpTraffic = 2, ExpAirPlane = 3, ATV = 4, Tank = 5, JCB = 6, Boat = 7, Null = 8 }
public class GlassGlobal : MonoBehaviour { public MeshFilter meshFilter; public Mesh replacedGlass; }
public class PlayerControl : MonoBehaviour { }
public class wheelModified : MonoBehaviour { public GameObject[] Wheels; }
