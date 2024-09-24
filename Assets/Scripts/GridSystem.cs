using MyUtils;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class GridSystem : MonoBehaviour {

    public Vector2Int _gridSize;
    public float _cellSize;
    public MeshCollider _col;
    public Mesh _mesh;
    public Material _material;
    public Material[] _materials;
    public Grid<GameObject> _grid;
    void Awake() {
        _grid = new(_gridSize, _cellSize, 16);
        CreateMesh();
    }
    public void CreateMesh() {
        _mesh = new Mesh();

        Vector3[] vertices = new Vector3[4 * _gridSize.x * _gridSize.y];
        Vector2[] uv = new Vector2[4 * _gridSize.x * _gridSize.y];
        int[] triangles = new int[6 * _gridSize.x * _gridSize.y];

        for (int x = 0; x < _gridSize.x; x++) {
            for (int z = 0; z < _gridSize.y; z++) {
                int index = x * _gridSize.y + z;
                Vector3 baseSize = new Vector3(1, 0, 1) * _cellSize;
                Vector2 cellUV = _grid.GetCellUvPos(x, z);
                MeshUtils.AddToMesh(vertices, uv, triangles, index, _grid.GetWorldPos(x, z), baseSize, cellUV, cellUV);
                // ChangeMaterialAtPosition(new Vector2Int(x, z));

            }
        }


        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = _mesh;
        GetComponent<MeshRenderer>().material = _material;
        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    public void ChangeMaterialAtPosition(Vector3 position) {
        Vector2Int gridPosition = _grid.GetGridCellPos(position);
        ChangeMaterialAtPosition(gridPosition);
    }
    public void ChangeMaterialAtPosition(Vector2Int gridPos) {
        int vertexIndex = gridPos.x * _gridSize.y + gridPos.y;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] uvs = mesh.uv;

        uvs[vertexIndex] = new Vector2(0.5f, 0.5f);

        mesh.uv = uvs;
    }

}

public class Grid<T> {
    public GridCell<T>[,] _cells;
    public float _cellSize;
    public Vector2Int _gridSize;

    public Grid(Vector2Int gSize, float cSize, int maxH) {
        _gridSize = gSize;
        _cellSize = cSize;

        _cells = new GridCell<T>[_gridSize.x, _gridSize.y];

        for (int x = 0; x < _gridSize.x; x++) {
            for (int z = 0; z < _gridSize.y; z++) {
                _cells[x, z] = new GridCell<T>() { _pos = GetWorldPos(x, z) + new Vector3(_cellSize, 0, _cellSize) * 0.5f, _h = (x) % 16 };
                _cells[x, z].CalculateUvPosition();
                Debug.DrawLine(GetWorldPos(x, z), GetWorldPos(x + 1, z), Color.white, 100f);
                Debug.DrawLine(GetWorldPos(x, z), GetWorldPos(x, z + 1), Color.white, 100f);
            }
        }
        Debug.DrawLine(new(0, 0, _gridSize.y), new(_gridSize.x, 0, _gridSize.y), Color.white, 100f);
        Debug.DrawLine(new(_gridSize.x, 0, 0), new(_gridSize.x, 0, _gridSize.y), Color.white, 100f);

    }
    public Vector2Int GetGridCellPos(Vector3 worldPos) {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / _cellSize), Mathf.FloorToInt(worldPos.z / _cellSize));
    }
    public Vector3 GetWorldPos(int x, int z) {
        return new Vector3(x, 0, z) * _cellSize;
    }
    public GridCell<T> GetGridCell(Vector3 pos) {
        return _cells[(int)pos.x, (int)pos.z];
    }
    public GridCell<T> GetGridCell(int x, int z) {
        return _cells[x, z];
    }
    public int GetCellH(int x, int z) {
        return _cells[x, z]._h;
    }
    public Vector2 GetCellUvPos(int x, int z) {
        Vector2 uvPos = new(Mathf.InverseLerp(0, 4, _cells[x, z]._uvPos.x), Mathf.InverseLerp(0, 4, _cells[x, z]._uvPos.y));
        uvPos += new Vector2(0.1f, 0.1f);
        Debug.Log($"uvPos for x = {x} and z = {z} = {uvPos}");
        return uvPos;
    }
}
public class GridCell<T> {
    public Vector3 _pos;
    public T _content;
    public int _h;
    public Vector2 _uvPos;
    public void CalculateUvPosition() {
        int hX = _h / 4;
        int hY = _h % 4;
        Debug.Log($"UV x = {hX}, y = {hY} for _h = {_h}");
        _uvPos = new Vector2(hX, hY);
    }
}
