using System;
using UnityEngine;

[Serializable]
public class Target
{
    public string Name;
    public GameObject PositionObject;
    public int FloorNumber; // 1 = Ground Floor, 2 = First Floor, etc.
}
