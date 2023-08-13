using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DecorationAssetSO : ScriptableObject {
    public GameObject prefab;
    public Vector2 area;
    public CellTag zone;

    [Range(0f, 1f)] 
    public float chances;
}
