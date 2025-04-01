using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("Tilemap và cấu hình spawn")]
    public Tilemap groundTilemap;
    public float positionOffset = 0.3f;
    public LayerMask obstacleLayer;

    [Header("Hiển thị Gizmos")]
    public bool showGizmos = true;
    public Color walkableColor = Color.green;
    public Color nonWalkableColor = Color.red;
    public float gizmoSize = 0.4f;

    public List<Vector3> validPositions { get; private set; } = new List<Vector3>();
    private List<Vector3> nonWalkablePositions = new List<Vector3>();

    private Vector3 gridOrigin;
    private float cellSize;

    void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
{
    validPositions.Clear();
    nonWalkablePositions.Clear();

    if (groundTilemap == null)
    {
        Debug.LogError("Ground Tilemap chưa được gán!");
        return;
    }

    BoundsInt bounds = groundTilemap.cellBounds;
    cellSize = groundTilemap.cellSize.x; // Ô vuông
    gridOrigin = groundTilemap.CellToWorld(bounds.position);

    for (int x = bounds.xMin; x < bounds.xMax; x++)
    {
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            if (groundTilemap.HasTile(cellPos))
            {
                Vector3Int cellAbove = new Vector3Int(x, y + 1, 0);
                // Lấy vị trí world của ô và cộng thêm offset để ra chính giữa ô
                Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(cellSize * 0.5f, groundTilemap.cellSize.y * 0.5f+1, 0f);
                // Nếu vẫn cần một offset theo trục y (ví dụ, để điều chỉnh vị trí spawn)
                worldPos.y += positionOffset;

                if (!groundTilemap.HasTile(cellAbove))
                {
                    validPositions.Add(worldPos);
                }
                else
                {
                    nonWalkablePositions.Add(worldPos);
                }
            }
        }
    }
}


    public Vector3 GetRandomPosition()
    {
        if (validPositions.Count == 0)
        {
            Debug.LogWarning("Không có vị trí hợp lệ nào trong lưới!");
            return Vector3.zero;
        }
        int randomIndex = Random.Range(0, validPositions.Count);
        return validPositions[randomIndex];
    }

    // 🔸 Check vị trí bất kỳ có nằm trong validPositions
    public bool IsWalkableAtWorldPos(Vector2 worldPos)
    {
        // Xác định cell gần nhất từ worldPos
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        Vector3 worldCellPos = groundTilemap.CellToWorld(cell);
        worldCellPos.y += groundTilemap.cellSize.y + positionOffset;

        return validPositions.Contains(worldCellPos);
    }

    // 🔸 Trả về mảng 3x3 (float[9]) vùng quanh worldPos (1f = đi được, 0f = không)
    public float[] GetWalkableAreaAround(Vector2 worldPos)
    {
        float[] area = new float[25];
        int index = 0;

        Vector2 bottomLeft = worldPos - new Vector2(cellSize, cellSize);

        for (int dx = 0; dx < 5; dx++)
        {
            for (int dy = 0; dy < 5; dy++)
            {
                Vector2 checkPos = bottomLeft + new Vector2(dx * cellSize, dy * cellSize);
                area[index] = IsWalkableAtWorldPos(checkPos) ? 1f : 0f;
                index++;
            }
        }

        return area;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || groundTilemap == null) return;

        Gizmos.color = walkableColor;
        foreach (Vector3 pos in validPositions)
        {
            Gizmos.DrawCube(pos, Vector3.one * gizmoSize);
        }

        Gizmos.color = nonWalkableColor;
        foreach (Vector3 pos in nonWalkablePositions)
        {
            Gizmos.DrawWireCube(pos, Vector3.one * gizmoSize);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && groundTilemap != null)
        {
            GenerateGrid();
        }
    }
#endif
}
