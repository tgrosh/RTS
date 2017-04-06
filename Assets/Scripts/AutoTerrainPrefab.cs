using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Auto Terrain/Prefab")]
public class AutoTerrainPrefab: ScriptableObject
{
    public GameObject prefab;
    [Range(0,1)]
    public float probability; //probability the prefab will appear
    [Range(0, 1)]
    public float groupingProbability; //probability the prefab will cause another identical prefab to be generated
}

