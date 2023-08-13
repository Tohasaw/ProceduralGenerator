using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralRoom : MonoBehaviour {
    [SerializeField] private Mesh wallMesh;
    [SerializeField] private Mesh wallMeshBroken;
    [SerializeField] private Mesh wallMeshDoor;
    [SerializeField] private Material wallMaterial0;
    [SerializeField] private Material wallMaterial1;

    [SerializeField] private Mesh pillarMesh;
    [SerializeField] private Mesh floorTile;

    [SerializeField] private Mesh cellMesh;
    [SerializeField] private Material cellMaterialDefault;
    [SerializeField] private Material cellMaterialNorth;
    [SerializeField] private Material cellMaterialSouth;
    [SerializeField] private Material cellMaterialWest;
    [SerializeField] private Material cellMaterialEast;

    [SerializeField] private Vector2 RoomSize;
    [SerializeField] private int Seed;
    [SerializeField] private List<DecorationAsset> DecorationAssets;
    [SerializeField] private bool _Debug;

    private Vector2 roomSize = new Vector2(20f, 20f);
    private Vector2 wallSize = new Vector2(4f, 1.5f);
    private Vector2 cellSize = new Vector2(1f, 1f);
    private int seed = 11110;
    private bool debug = true;
    private List<Matrix4x4> walls;
    private List<Matrix4x4> wallsBroken;
    private List<Matrix4x4> wallsDoor;
    private List<Matrix4x4> pillars;
    private List<Cell> cells;
    private List<Matrix4x4> cellsVisual;
    private List<Matrix4x4> cellsNorthVisual;
    private List<Matrix4x4> cellsSouthVisual;
    private List<Matrix4x4> cellsWestVisual;
    private List<Matrix4x4> cellsEastVisual;

    private void Start() {
        //CreateCells();
        //CreateWalls();
        //CreatePillars();
        //CreateDecorations();
    }

    private void Update() {
        if (ValuesChanged()) {
            CreateCells();
            CreateWalls();
            CreatePillars();
            //CreateDecorations();
        }
        if (DebugChanged()) {
            debug = _Debug;
        }
        RenderWalls();
        RenderPillars();
        RenderCells();
        //RenderDecorations();
    }

    private void CreateCells() {
        Random.InitState(seed);

        cells = new List<Cell>();
        cellsVisual = new List<Matrix4x4>();
        cellsNorthVisual = new List<Matrix4x4>();
        cellsSouthVisual = new List<Matrix4x4>();
        cellsWestVisual = new List<Matrix4x4>();
        cellsEastVisual = new List<Matrix4x4>();

        int wallCountXx4 = Mathf.Max(1, (int)(roomSize.x / wallSize.x) * 4);
        int wallCountYx4 = Mathf.Max(1, (int)(roomSize.y / wallSize.x) * 4);

        for (int i = 0; i < wallCountXx4; i++) {
            for (int j = 0; j < wallCountYx4; j++) {
                var cellTag = CellTag.Inside;
                var cellSideTag = CellSideTag.None;
                var position = transform.position + new Vector3(-roomSize.x / 2 + cellSize.x * i + cellSize.y / 2, 0, roomSize.y / 2 + -cellSize.y * j - cellSize.y / 2);

                var r = transform.rotation;
                var s = new Vector3(1, 1, 1);

                var mat = Matrix4x4.TRS(position, r, s);

                if (i == 0 ) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.West;
                    cellsWestVisual.Add(mat);
                } else if (i == wallCountXx4 - 1) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.East;
                    cellsEastVisual.Add(mat);
                } else if (j == 0 && i != 0 && i != wallCountXx4) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.North;
                    cellsNorthVisual.Add(mat);
                } else if (j == wallCountYx4 - 1 && i != 0 && i != wallCountXx4) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.South;
                    cellsSouthVisual.Add(mat);
                } else if (i != 0 && i != wallCountXx4) {
                    cellsVisual.Add(mat);
                }

                cells.Add(new Cell(position, cellTag, cellSideTag));
            }
        }
    }

    void CreateWalls() {

        walls = new List<Matrix4x4>();
        wallsBroken = new List<Matrix4x4>();
        wallsDoor = new List<Matrix4x4>();

        int wallCountX = Mathf.Max(1, (int)(roomSize.x / wallSize.x));
        int wallCountY = Mathf.Max(1, (int)(roomSize.y / wallSize.x));
        float wallCornerCorrection = wallSize.y / 2;
        float XWallZPoint = roomSize.y / 2 + wallCornerCorrection;
        float YWallXpoint = wallCountX * wallSize.x / 2 + wallCornerCorrection;

        //WallsX
        for (int i = -1; i < 2; i += 2) {
            for (int j = 0; j < wallCountX; j++) {

                var t = transform.position + new Vector3(-roomSize.x / 2 + wallSize.x / 2 + j * wallSize.x, 0, i * XWallZPoint);
                var r = transform.rotation;
                var s = new Vector3(1, 1, 1);

                var mat = Matrix4x4.TRS(t, r, s);
                     
                var rand = Random.Range(0, 3);
                if (rand < 1) {
                    walls.Add(mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);

                    DeleteCellsWithVisualAtX(t, i);
                } else {
                    wallsDoor.Add(mat);

                    DeleteCellsWithVisualAtX(t, i);
                }
            }
        }

        //WallsY
        for (int i = -1; i < 2; i += 2) {
            for (int j = 0; j < wallCountY; j++) {

                var t = transform.position + new Vector3(i * YWallXpoint, 0, -roomSize.y / 2 + wallSize.x / 2 + j * wallSize.x);
                var r = Quaternion.Euler(0, 90, 0);
                var s = new Vector3(1, 1, 1);

                var mat = Matrix4x4.TRS(t, r, s);

                var rand = Random.Range(0, 3);
                if (rand < 1) {
                    walls.Add   (mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);

                    DeleteCellsWithVisualAtY(t, i);
                } else {
                    wallsDoor.Add(mat);

                    DeleteCellsWithVisualAtY(t, i);
                }
            }
        }
    }

    private void DeleteCellsWithVisualAtY(Vector3 t, int i) {
        for (int k = 0; k < 4; k++) {
            var cell = cells.FirstOrDefault(x => x.position == t + new Vector3(-i * 1.25f, 0, -1.5f + k));
            cells.Remove(cell);
            if (i == 1) {
                var cellVisual = cellsEastVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.25f, 0, -1.5f + k), transform.rotation, new Vector3(1, 1, 1)));
                cellsEastVisual.Remove(cellVisual);
            } else {
                var cellVisual = cellsWestVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(1.25f, 0, -1.5f + k), transform.rotation, new Vector3(1, 1, 1)));
                cellsWestVisual.Remove(cellVisual);
            }
        }
    }

    private void DeleteCellsWithVisualAtX(Vector3 t, int i) {
        for (int k = 0; k < 4; k++) {
            var cell = cells.FirstOrDefault(x => x.position == t + new Vector3(-1.5f + k, 0, -i * 1.25f));
            cells.Remove(cell);
            var r = transform.rotation;
            var s = new Vector3(1, 1, 1);
            if (i == 1) {
                if (cellsWestVisual.Exists(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, -1.25f), r, s))) {
                    var cellVisual = cellsWestVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, -1.25f), r, s));
                    cellsWestVisual.Remove(cellVisual);
                } else if (cellsEastVisual.Exists(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, -1.25f), r, s))) {
                    var cellVisual = cellsEastVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, -1.25f), r, s));
                    cellsEastVisual.Remove(cellVisual);
                } else {
                    var cellVisual = cellsNorthVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, -1.25f), r, s));
                    cellsNorthVisual.Remove(cellVisual);
                }
            } else {
                if (cellsWestVisual.Exists(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, 1.25f), r, s))) {
                    var cellVisual = cellsWestVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, 1.25f), r, s));
                    cellsWestVisual.Remove(cellVisual);

                } else if (cellsEastVisual.Exists(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, 1.25f), r, s))) {
                    var cellVisual = cellsEastVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, 1.25f), r, s));
                    cellsEastVisual.Remove(cellVisual);
                } else {
                    var cellVisual = cellsSouthVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t + new Vector3(-1.5f + k, 0, 1.25f), r, s));
                    cellsSouthVisual.Remove(cellVisual);
                }
            }
        }
    }

    private void CreatePillars() {
        pillars = new List<Matrix4x4>();

        int wallCountX = Mathf.Max(1, (int)(roomSize.x / wallSize.x));
        float wallCornerCorrection = wallSize.y / 2;
        float XWallZPoint = roomSize.y / 2 + wallCornerCorrection;
        float YWallXpoint = wallCountX * wallSize.x / 2 + wallCornerCorrection;

        //Pillars
        for (int i = -1; i < 2; i += 2) {
            for (int j = -1; j < 2; j += 2) {
                var t = transform.position + new Vector3(i * YWallXpoint, 0, j * XWallZPoint);
                var r = transform.rotation;
                var s = Vector3.one;

                var mat = Matrix4x4.TRS(t, r, s);
                pillars.Add(mat);
            }
        }
    }

    void CreateDecorations() {
        if (cells != null) {

            for (int i = 0; i < cells.Count; i++) {
                //var cell = cells.availablePosition
            }
        }
    }

    void CreateWallDecorations() {

    }

    void CreateInsideDecorations() {

    }

    void RenderWalls() {
        if (walls != null) {
            //Wall
            Graphics.DrawMeshInstanced(wallMesh, 0, wallMaterial1, walls.ToArray(), walls.Count);
            Graphics.DrawMeshInstanced(wallMesh, 1, wallMaterial0, walls.ToArray(), walls.Count);
            //Door Wall
            Graphics.DrawMeshInstanced(wallMeshDoor, 0, wallMaterial1, wallsDoor.ToArray(), wallsDoor.Count);
            Graphics.DrawMeshInstanced(wallMeshDoor, 1, wallMaterial0, wallsDoor.ToArray(), wallsDoor.Count);
            //Broken Wall
            Graphics.DrawMeshInstanced(wallMeshBroken, 0, wallMaterial1, wallsBroken.ToArray(), wallsBroken.Count);
            Graphics.DrawMeshInstanced(wallMeshBroken, 1, wallMaterial0, wallsBroken.ToArray(), wallsBroken.Count);
        }
    }

    void RenderPillars() {
        Graphics.DrawMeshInstanced(pillarMesh, 0, wallMaterial0, pillars.ToArray(), pillars.Count);
        Graphics.DrawMeshInstanced(pillarMesh, 1, wallMaterial1, pillars.ToArray(), pillars.Count);
    }

    private void RenderCells() {
        if (debug) {
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialDefault, cellsVisual.ToArray(), cellsVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialNorth, cellsNorthVisual.ToArray(), cellsNorthVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialSouth, cellsSouthVisual.ToArray(), cellsSouthVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialWest, cellsWestVisual.ToArray(), cellsWestVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialEast, cellsEastVisual.ToArray(), cellsEastVisual.Count);
        }
    }

    private void RenderDecorations() {
        throw new System.NotImplementedException();
    }

    private bool DebugChanged() {
        if (debug != _Debug) {
            return true;
        }
        return false;
    }

    private bool ValuesChanged() {
        bool changed = false;
        if (seed != Seed) {
            seed = Seed;
            changed = true;
        }
        if (roomSize != RoomSize) {
            if (RoomSize.x % wallSize.x == 0 && RoomSize.y % wallSize.x == 0) {
                roomSize = RoomSize;
                changed = true;
            } 
        }
        return changed;
    }
}
