using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralRoom : MonoBehaviour
{
    [SerializeField] private Mesh wallMesh;
    [SerializeField] private Mesh wallMeshBroken;
    [SerializeField] private Mesh wallMeshDoor;
    [SerializeField] private Material wallMaterial0;
    [SerializeField] private Material wallMaterial1;
    [SerializeField] private Mesh pillarMesh;
    [SerializeField] private Mesh floorTile;
    [SerializeField] private Vector2 RoomSize;
    [SerializeField] private int Seed;
    [SerializeField] private List<DecorationAsset> DecorationAssets;

    private Vector2 roomSize = new Vector2(20f, 20f);
    private Vector2 wallSize = new Vector2 (4f, 1.5f);
    private int seed = 11110;
    private List<Matrix4x4> walls;
    private List<Matrix4x4> wallsBroken;
    private List<Matrix4x4> wallsDoor;
    private List<Matrix4x4> pillars;
    private List<Cell> cells;

    private void Start() {
        Random.InitState(seed);
        CreateWalls();
        CreateCells();
        CreateDecorations();
    }

    private void Update() {
        if (ValuesChanged()) {
            CreateWalls();
            CreateCells();
            CreateDecorations();
        }
        RenderWalls();
        RenderDecorations();
    }

    void CreateWalls() {

        walls = new List<Matrix4x4>();
        wallsBroken = new List<Matrix4x4>();
        wallsDoor = new List<Matrix4x4>();
        pillars = new List<Matrix4x4>();

        int wallCountX = Mathf.Max(1, (int)(roomSize.x / wallSize.x));
        int wallCountY = Mathf.Max(1, (int)(roomSize.y / wallSize.x));
        float scaleX = (roomSize.x / wallCountX) / wallSize.x;
        float scaleY = (roomSize.y / wallCountY) / wallSize.x;
        float wallCornerCorrection = wallSize.y / 2;
        float XWallZPoint = roomSize.y / 2 + wallCornerCorrection;
        float YWallXpoint = wallCountX * wallSize.x / 2 * scaleX + wallCornerCorrection;

        //Walls
        for (int i = -1; i < 2; i += 2) {
            for (int j = 0; j < wallCountX; j++) {

                var t = transform.position + new Vector3(-roomSize.x / 2 + wallSize.x * scaleX / 2 + j * scaleX * wallSize.x, 0, i * XWallZPoint);
                var r = transform.rotation;
                var s = new Vector3(scaleX, 1, 1);

                var mat = Matrix4x4.TRS(t, r, s);

                var rand = Random.Range(0, 3);
                if (rand < 1) {
                    walls.Add(mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);
                } else {
                    wallsDoor.Add(mat);
                }
            }
        }

        //Walls
        for (int i = -1; i < 2; i += 2) {
            for (int j = 0; j < wallCountY; j++) {

                var t = transform.position + new Vector3(i * YWallXpoint, 0, -roomSize.y / 2 + wallSize.x * scaleY / 2 + j * scaleY * wallSize.x);
                var r = Quaternion.Euler(0, 90, 0);
                var s = new Vector3(scaleY, 1, 1);

                var mat = Matrix4x4.TRS(t, r, s);

                var rand = Random.Range(0, 3);
                if (rand < 1) {
                    walls.Add(mat);
                } else if (rand < 2) {
                    wallsBroken.Add(mat);
                } else {
                    wallsDoor.Add(mat);
                }
            }
        }

        //Pillars
        for (int i = -1; i < 2; i+=2) {
            for (int j = -1; j < 2; j+=2) {
                var t = transform.position + new Vector3(i * YWallXpoint, 0, j * XWallZPoint);
                var r = transform.rotation;
                var s = Vector3.one;

                var mat = Matrix4x4.TRS(t, r, s);
                pillars.Add(mat);
            }
        }

        //Floor 
        
    }

    private void CreateCells() {
        if (walls != null) {
            cells = new List<Cell>();
            int wallCountX = Mathf.Max(1, (int)(roomSize.x / wallSize.x));
            int wallCountY = Mathf.Max(1, (int)(roomSize.y / wallSize.x));
            float scaleX = (roomSize.x / wallCountX) / wallSize.x;
            float scaleY = (roomSize.y / wallCountY) / wallSize.x;
            float wallCornerCorrection = wallSize.y / 2;
            float XWallZPoint = roomSize.y / 2 + wallCornerCorrection;
            float YWallXpoint = wallCountX * wallSize.x / 2 * scaleX + wallCornerCorrection;

            for (int i = 0; i < wallCountX * 4; i++) {
                for (int j = 0; j < wallCountY * 4; j++) {
                    var t = transform.position + new Vector3(i * YWallXpoint, 0, -roomSize.y / 2 + wallSize.x * scaleY / 2 + j * scaleY * wallSize.x);
                    if (j == 0) { }
                    if (j == wallCountY - 1) { }
                    if (i == 0) { }
                    if (i == wallCountX - 1) { }
                }
            }
        }
    }


    void CreateDecorations() {
        if (cells != null) {
            int wallCountX = Mathf.Max(1, (int)(roomSize.x / wallSize.x));
            int wallCountY = Mathf.Max(1, (int)(roomSize.y / wallSize.x));

            for (int i = 0; i < wallCountX * 4 * wallCountY * 4; i++) {

            }
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
            //Pillar
            Graphics.DrawMeshInstanced(pillarMesh, 0, wallMaterial0, pillars.ToArray(), pillars.Count);
            Graphics.DrawMeshInstanced(pillarMesh, 1, wallMaterial1, pillars.ToArray(), pillars.Count);
        }
    }

    private void RenderDecorations() {
        throw new System.NotImplementedException();
    }

    bool ValuesChanged() {
        if (seed != Seed || roomSize != RoomSize) {
            seed = Seed;
            if (RoomSize.x % wallSize.x == 0 && RoomSize.y % wallSize.y == 0)
                roomSize = RoomSize;
            return true;
        }
        return false;
    }
}
