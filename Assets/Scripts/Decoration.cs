using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Decoration : MonoBehaviour
{
    public DecorationAssetSO decorationAssetSO;

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
