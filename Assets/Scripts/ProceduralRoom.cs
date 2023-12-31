using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ProceduralRoom : MonoBehaviour {

    private const float CENTER_ANGLE = 0;
    private const float NORTH_ANGLE = 180;
    private const float SOUTH_ANGLE = 0;
    private const float WEST_ANGLE = 90;
    private const float EAST_ANGLE = 270;

    [SerializeField] private Mesh wallMesh;
    [SerializeField] private Mesh wallMeshBroken;
    [SerializeField] private Mesh wallMeshDoor;
    [SerializeField] private Material wallMaterial0;
    [SerializeField] private Material wallMaterial1;

    [SerializeField] private Mesh pillarMesh;

    [SerializeField] private List<Mesh> floorTileMeshList;
    [SerializeField] private Material floorTileMaterial;

    [SerializeField] private Mesh cellMesh;
    [SerializeField] private Material cellMaterialDefault;
    [SerializeField] private Material cellMaterialNorth;
    [SerializeField] private Material cellMaterialSouth;
    [SerializeField] private Material cellMaterialWest;
    [SerializeField] private Material cellMaterialEast;

    [SerializeField] private Vector2 RoomSize;
    [SerializeField] private int Seed;
    [SerializeField] private DecorationAssetSOList DecorationAssetsSOList;
    [SerializeField] private bool _Debug;
    [SerializeField] private float CellWallChances;
    [SerializeField] private float CellInsideChances;

    private Vector2 wallSize = new Vector2(4f, 1.5f);
    private Vector2 cellSize = new Vector2(1f, 1f);
    private float cellHeight = 0.15f;
    private Vector2 floorTileSize = new Vector2(2f, 2f);
    private Vector2 roomSize;
    private int seed;
    private bool _debug;
    private float cellWallChances;
    private float cellInsideChances;
    private List<Cell> cells;
    private List<Transform> decorations;

    private List<Matrix4x4> walls;
    private List<Matrix4x4> wallsBroken;
    private List<Matrix4x4> wallsDoor;

    private List<Matrix4x4> pillars;

    private List<Matrix4x4> floorTiles;
    private List<Matrix4x4> floorTilesLeft;
    private List<Matrix4x4> floorTilesRight;

    private List<Matrix4x4> cellsVisual;
    private List<Matrix4x4> cellsNorthVisual;
    private List<Matrix4x4> cellsSouthVisual;
    private List<Matrix4x4> cellsWestVisual;
    private List<Matrix4x4> cellsEastVisual;

    private void Update() {
        if (AnyChanges()) {
            RemoveDecorations();
            CreateCells();
            CreateFloor();
            CreateWalls();
            CreatePillars();
            CreateDecorations();
        }
        if (DebugChanged()) {
            _debug = _Debug;
        }
        RenderCells();
        RenderFloor();
        RenderWalls();
        RenderPillars();
        //RenderDecorations();
    }

    private void CreateCells() {
        UnityEngine.Random.InitState(seed);

        cells = new List<Cell>();
        cellsVisual = new List<Matrix4x4>();
        cellsNorthVisual = new List<Matrix4x4>();
        cellsSouthVisual = new List<Matrix4x4>();
        cellsWestVisual = new List<Matrix4x4>();
        cellsEastVisual = new List<Matrix4x4>();

        int cellCountX = Mathf.Max(1, (int)(roomSize.x / cellSize.x));
        int cellCountY = Mathf.Max(1, (int)(roomSize.y / cellSize.y));

        for (int i = 0; i < cellCountX; i++) {
            for (int j = 0; j < cellCountY; j++) {
                var cellTag = CellTag.Inside;
                var cellSideTag = CellSideTag.Center;
                var position = transform.position + new Vector3(-roomSize.x / 2 + cellSize.x * i + cellSize.x / 2, 0, -roomSize.y / 2 + cellSize.y * j - cellSize.y / 2 + 1);

                var r = transform.rotation;
                var s = new Vector3(1, 1, 1);

                var mat = Matrix4x4.TRS(position, r, s);
                var matVisual = Matrix4x4.TRS(position + new Vector3(0, cellHeight, 0), r, s);
                if (i == 0) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.West;
                    cellsWestVisual.Add(matVisual);
                } else if (i == cellCountX - 1) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.East;
                    cellsEastVisual.Add(matVisual);
                } else if (j == 0) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.South;
                    cellsSouthVisual.Add(matVisual);
                } else if (j == cellCountY - 1) {
                    cellTag = CellTag.Wall;
                    cellSideTag = CellSideTag.North;
                    cellsNorthVisual.Add(matVisual);
                } else {
                    cellsVisual.Add(matVisual);
                }

                cells.Add(new Cell(position, cellTag, cellSideTag));
            }
        }
    }

    private void CreateFloor() {
        floorTiles = new List<Matrix4x4>();
        floorTilesLeft = new List<Matrix4x4>();
        floorTilesRight = new List<Matrix4x4>();

        int floorTilesCountX = Mathf.Max(1, (int)(roomSize.x / floorTileSize.x));
        int floorTilesCountZ = Mathf.Max(1, (int)(roomSize.y / floorTileSize.y));

        for (int i = 0; i < floorTilesCountX; i++) {
            for (int j = 0; j < floorTilesCountZ; j++) {
                var position = transform.position + new Vector3(-roomSize.x / 2 + floorTileSize.x * i + floorTileSize.y / 2, 0, roomSize.y / 2 + -floorTileSize.y * j - floorTileSize.y / 2);
                var scale = new Vector3(1, 1, 1);

                var randScale = UnityEngine.Random.Range(0, 2);
                if (randScale < 1) {
                    scale = new Vector3(-1, 1, -1);
                }

                var mat = Matrix4x4.TRS(position, transform.rotation, scale);

                var rand = UnityEngine.Random.Range(0, 5);
                if (rand < 1) {
                    floorTilesLeft.Add(mat);
                } else if (rand < 2) {
                    floorTilesRight.Add(mat);
                } else {
                    floorTiles.Add(mat);
                }
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

                var rand = UnityEngine.Random.Range(0, 3);
                bool removeCells = false;

                if (rand < 1) {
                    walls.Add(mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);
                    removeCells = true;
                } else {
                    wallsDoor.Add(mat);
                    removeCells = true;
                }

                if (removeCells) {
                    for (int k = 0; k < 4; k++) {
                        var cellPos = t + new Vector3(-1.5f + k, 0, -i * 1.25f);
                        var cell = cells.FirstOrDefault(x => x.position == cellPos);

                        cells.Remove(cell);
                        RemoveCellVisual(cellPos);
                    }
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

                var rand = UnityEngine.Random.Range(0, 3);
                bool removeCells = false;

                if (rand < 1) {
                    walls.Add(mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);
                    removeCells = true;
                } else {
                    wallsDoor.Add(mat);
                    removeCells = true;
                }

                if (removeCells) {
                    for (int k = 0; k < 4; k++) {
                        var cellPos = t + new Vector3(-i * 1.25f, 0, -1.5f + k);
                        var cell = cells.FirstOrDefault(x => x.position == cellPos);

                        cells.Remove(cell);
                        RemoveCellVisual(cellPos);
                    }
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
            decorations = new List<Transform>();

            for (int i = 0; i < cells.Count; i++) {
                var cell = cells[i];
                var zoneChances = ZoneChances(cell.zone);
                var rand = UnityEngine.Random.Range(0, 1f);

                if (rand <= zoneChances) {
                    var possibleElements = DecorationAssetsSOList.list.Where(x => x.zone == cell.zone).ToList();

                    if (possibleElements.Any()) {
                        var assetSO = PickOneAssetSO(possibleElements);
                        var pos = cell.position;
                        var rot = GetRotation(cell.side);

                        var decPos = GetDecorationPos(pos, assetSO.area, rot);

                        if (IsInsideRoom(pos, rot, assetSO.area) && !IsOverLap(decPos, rot, assetSO.area)) {
                            RemoveArea(pos, assetSO.area, rot);

                            Transform DecorationObjectTransform = Instantiate(assetSO.prefab.transform, decPos, rot);
                            DecorationObjectTransform.GetComponent<Decoration>().decorationAssetSO = assetSO;
                            decorations.Add(DecorationObjectTransform);

                        }
                    }
                }
            }
        }
    }

    private Vector3 GetDecorationPos(Vector3 pos, Vector2 area, Quaternion rot) {

        if (area.x > 1 || area.y > 1) {

            float x, y;

            switch (rot.eulerAngles.y) {
                case SOUTH_ANGLE:
                    x = 1; y = 1;
                    pos = pos + new Vector3(area.x * 0.5f * x - 0.5f, 0, area.y * 0.5f * y - 0.5f);
                    break;
                case NORTH_ANGLE:
                    x = -1; y = -1;
                    pos = pos + new Vector3(area.x * 0.5f * x + 0.5f, 0, area.y * 0.5f * y + 0.5f); 
                    break;
                case WEST_ANGLE:
                    x = 1; y = 1;
                    pos = pos + new Vector3(area.y * 0.5f * x - 0.5f, 0, area.x * 0.5f * y - 0.5f);
                    break;
                case EAST_ANGLE:
                    x = -1; y = 1;
                    pos = pos + new Vector3(area.y * 0.5f * x + 0.5f, 0, area.x * 0.5f * y - 0.5f);
                    break;
                default: 
                    Debug.LogException(new Exception("Invalid rotation (GetDecorationPos): " + rot.eulerAngles.y)); break;
            }
        }
        return pos;
    }

    private bool IsInsideRoom(Vector3 pos, Quaternion rot, Vector2 area) {
        bool a = true, b = true, c = true, d = true;
        switch (rot.y) {
            case SOUTH_ANGLE:
                b = roomSize.x / 2 > pos.x + area.x / 2;
                break;
            case NORTH_ANGLE:
                a = -roomSize.x / 2 < pos.x - area.x / 2;
                break;
            case WEST_ANGLE: 
                d = roomSize.y / 2 > pos.z + area.y / 2;
                break;
            case EAST_ANGLE:
                d = roomSize.y / 2 > pos.z + area.y / 2;
                break;
        }
        if (a && b && c && d) {
            return true;
        }
        return false;
    }

    private bool IsOverLap(Vector3 pos, Quaternion rot, Vector2 area) {
        foreach (Transform decoration in decorations) {
            var areaD = decoration.GetComponent<Decoration>().decorationAssetSO.area;
            var posD = decoration.position;

            var oldAsset = GetAssetDiagonalPoint(posD, areaD);
            var newAsset = GetAssetDiagonalPoint(pos, area);

            if (oldAsset[1].z < newAsset[0].z || oldAsset[0].z > newAsset[1].z || oldAsset[1].x < newAsset[0].x || oldAsset[0].x > newAsset[1].x) {
                continue;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    ///  This method returns coordinats of low left and top right corners of area.
    /// </summary>
    private Vector3[] GetAssetDiagonalPoint(Vector3 position, Vector2 area) {

        Vector3 pointLowLeft = new Vector3(position.x - area.x / 2, 0, position.z - area.y / 2); 
        Vector3 pointTopRight = new Vector3(position.x + area.x / 2, 0, position.z + area.y / 2);

        return new Vector3[] { pointLowLeft, pointTopRight };
    }

    float ZoneChances(CellTag zone) {
        switch (zone) {
            case CellTag.Wall:
                return cellWallChances;
            case CellTag.Inside:
                return cellInsideChances;
        }

        return default;
    }

    private DecorationAssetSO PickOneAssetSO(List<DecorationAssetSO> possibleElements) {
        var rand = UnityEngine.Random.Range(0, possibleElements.Count);
        var id = (int) Mathf.Floor(rand);

        var decorationAssetSO = possibleElements[id];

        return decorationAssetSO;

    }

    private Quaternion GetRotation(CellSideTag side) {
        switch (side) {
            case CellSideTag.North:
                return Quaternion.Euler(0, NORTH_ANGLE, 0);
            case CellSideTag.South:
                return Quaternion.Euler(0, SOUTH_ANGLE, 0);
            case CellSideTag.West:
                return Quaternion.Euler(0, WEST_ANGLE, 0);
            case CellSideTag.East:
                return Quaternion.Euler(0, EAST_ANGLE, 0);
            case CellSideTag.Center:
                return Quaternion.Euler(0, SOUTH_ANGLE, 0);
        }
        return default;
    }

    private void RemoveArea(Vector3 pos, Vector2 area, Quaternion rot) {

        float iniX = 0, endX = 0, iniY = 0, endY = 0;

        if (area.x > 1 || area.y > 1) {
            switch (rot.eulerAngles.y) {
                case SOUTH_ANGLE:
                    iniX = pos.x; endX = pos.x + area.x;
                    iniY = pos.z; endY = pos.z + area.y;
                    break;
                case NORTH_ANGLE:
                    iniX = pos.x - area.x + 1; endX = pos.x + 1;
                    iniY = pos.z - area.y + 1; endY = pos.z + 1;
                    break;
                case WEST_ANGLE:
                    iniX = pos.x; endX = pos.x + area.y;
                    iniY = pos.z; endY = pos.z + area.x;   
                    break;
                case EAST_ANGLE:
                    iniX = pos.x - area.y + 1; endX = pos.x + 1;
                    iniY = pos.z; endY = pos.z + area.x;
                    break;
                default:
                    Debug.LogException(new Exception("Invalid rotation (GetDecorationPos): " + rot.eulerAngles.y)); break;
            }

            for (float i = iniX; i < endX; i++) {
                for (float j = iniY; j < endY; j++) {
                    var cell = cells.FirstOrDefault(x => x.position.x == i && x.position.z == j);
                    if (cell != null) {
                        cells.Remove(cell);
                        RemoveCellVisual(new Vector3(i, 0, j));
                    }
                }
            }

        } else {
            var cell = cells.FirstOrDefault(x => x.position.x == pos.x && x.position.z == pos.z);
            if (cell != null) {
                cells.Remove(cell);
                RemoveCellVisual(pos);
            }
        }
    }

    void RemoveDecorations() {
        if (decorations != null) {
            for (int i = 0; i < decorations.Count; i++) {
                decorations[i].GetComponent<Decoration>().DestroySelf();
            }
        }
        decorations = null;
    }

    private void RemoveCellVisual(Vector3 position) {
        var t = new Vector3(position.x, cellHeight, position.z);
        var r = transform.rotation;
        var s = new Vector3(1, 1, 1);

        if (cellsWestVisual.Exists(x => x == Matrix4x4.TRS(t, r, s))) {
            var cellVisual = cellsWestVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t, r, s));
            cellsWestVisual.Remove(cellVisual);
        }
        if (cellsEastVisual.Exists(x => x == Matrix4x4.TRS(t, r, s))) {
            var cellVisual = cellsEastVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t, r, s));
            cellsEastVisual.Remove(cellVisual);
        }
        if (cellsSouthVisual.Exists(x => x == Matrix4x4.TRS(t, r, s))) {
            var cellVisual = cellsSouthVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t, r, s));
            cellsSouthVisual.Remove(cellVisual);
        }
        if (cellsNorthVisual.Exists(x => x == Matrix4x4.TRS(t, r, s))) {
            var cellVisual = cellsNorthVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t, r, s));
            cellsNorthVisual.Remove(cellVisual);
        }
        if (cellsVisual.Exists(x => x == Matrix4x4.TRS(t, r, s))) {
            var cellVisual = cellsVisual.FirstOrDefault(x => x == Matrix4x4.TRS(t, r, s));
            cellsVisual.Remove(cellVisual);
        }
    }

    private bool DebugChanged() {
        if (_debug != _Debug) {
            return true;
        }
        return false;
    }

    private bool AnyChanges() {
        bool changed = false;
        if (seed != Seed || cellInsideChances != CellInsideChances || cellWallChances != CellWallChances) {
            seed = Seed;
            cellInsideChances = CellInsideChances;
            cellWallChances = CellWallChances;
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

    private void RenderCells() {
        if (_debug) {
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialDefault, cellsVisual.ToArray(), cellsVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialNorth, cellsNorthVisual.ToArray(), cellsNorthVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialSouth, cellsSouthVisual.ToArray(), cellsSouthVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialWest, cellsWestVisual.ToArray(), cellsWestVisual.Count);
            Graphics.DrawMeshInstanced(cellMesh, 0, cellMaterialEast, cellsEastVisual.ToArray(), cellsEastVisual.Count);
        }
    }

    private void RenderFloor() {
        if (floorTiles != null) {
            Graphics.DrawMeshInstanced(floorTileMeshList[0], 0, floorTileMaterial, floorTiles.ToArray(), floorTiles.Count);
            Graphics.DrawMeshInstanced(floorTileMeshList[1], 0, floorTileMaterial, floorTilesLeft.ToArray(), floorTilesLeft.Count);
            Graphics.DrawMeshInstanced(floorTileMeshList[2], 0, floorTileMaterial, floorTilesRight.ToArray(), floorTilesRight.Count);
        }
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
}
