using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cell {
    public Vector3 position;
    public CellTag zone;
    public CellSideTag side;
    public Matrix4x4 cell;

    public Cell(Vector3 position, CellTag zone, CellSideTag side) {
        this.position = position;
        this.zone = zone;
        this.side = side;
    }

    public override string ToString() {
        return position + " " + zone + " " + side;
    }

    private void OnDestroy() {
        Debug.Log("Deleted");
    }

}
