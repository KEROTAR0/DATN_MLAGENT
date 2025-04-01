using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingManager : MonoBehaviour
{
    [Header("Grid Data")]
    public GridManager gridManager;

    [Header("Jump Constraints")]
    public float maxJumpHeight = 4f;
    public float maxJumpDistance = 4f;

    [Header("Gizmos Settings")]
    public bool drawPathGizmos = true;
    public Color pathColor = Color.cyan;
    
    [Header("Editor Preview Path")]
    [Tooltip("Gán transform của Agent để xem đường đi trong Editor")]
    public Transform agentTransform;
    [Tooltip("Gán transform của Item (mục tiêu) để xem đường đi trong Editor")]
    public Transform targetTransform;

    private List<PathNode> nodes = new List<PathNode>();

    void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager chưa được gán!");
            return;
        }
        GenerateNodesFromGrid();
    }

    void GenerateNodesFromGrid()
    {
        nodes.Clear();
        // Tạo node từ các điểm an toàn trong grid
        foreach (Vector3 pos in gridManager.validPositions)
        {
            nodes.Add(new PathNode(pos));
        }

        // Xác định hàng xóm cho từng node dựa trên khoảng cách và nhảy cho phép
        foreach (PathNode node in nodes)
        {
            foreach (PathNode other in nodes)
            {
                if (node == other)
                    continue;

                Vector2 diff = other.worldPos - node.worldPos;
                float dx = Mathf.Abs(diff.x);
                float dy = other.worldPos.y - node.worldPos.y;

                // Kiểm tra giới hạn nhảy (cho cả nhảy lên và cho phép chênh lệch nhỏ xuống)
                if (dx <= maxJumpDistance && dy <= maxJumpHeight && dy >= -maxJumpHeight)
                {
                    // Kiểm tra đường đi không có vật cản: 
                    // (Có thể cần điều chỉnh vị trí bắt đầu của raycast nếu cần)
                    RaycastHit2D hit = Physics2D.Linecast(node.worldPos, other.worldPos, gridManager.obstacleLayer);
                    if (!hit.collider)
                    {
                        node.neighbors.Add(other);
                    }
                }
            }
        }
    }

    // Tìm đường đi bằng thuật toán A* từ vị trí start đến end
    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        // Tìm node gần nhất với vị trí start và end
        PathNode startNode = FindClosestNode(startPos);
        PathNode endNode = FindClosestNode(endPos);

        if (startNode == null || endNode == null)
        {
            //Debug.LogWarning("Không tìm được node bắt đầu hoặc kết thúc!");
            return null;
        }

        // Reset chi phí của các node
        foreach (PathNode node in nodes)
        {
            node.gCost = Mathf.Infinity;
            node.hCost = 0;
            node.parent = null;
        }

        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startNode, endNode);

        while (openList.Count > 0)
        {
            PathNode current = GetLowestFCostNode(openList);

            if (current == endNode)
            {
                return ReconstructPath(current);
            }

            openList.Remove(current);
            closedSet.Add(current);

            foreach (PathNode neighbor in current.neighbors)
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeG = current.gCost + Vector2.Distance(current.worldPos, neighbor.worldPos);
                if (tentativeG < neighbor.gCost)
                {
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = GetHeuristic(neighbor, endNode);
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
            //Debug.Log($"Số lượng openList: {openList.Count}, closedSet: {closedSet.Count}");
        }

        //Debug.LogWarning("Không tìm được đường đi!");
        return null;
    }

    // Tìm node gần nhất với vị trí được cho
    PathNode FindClosestNode(Vector3 pos)
    {
        PathNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (PathNode node in nodes)
        {
            float dist = Vector2.Distance(pos, node.worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }
        return closest;
    }

    float GetHeuristic(PathNode a, PathNode b)
    {
        float baseCost = Vector2.Distance(a.worldPos, b.worldPos);
        float heightDiff = b.worldPos.y - a.worldPos.y;

        if (heightDiff < 0) // Nếu phải đi xuống, ưu tiên hơn
            baseCost *= 0.8f; // Giảm chi phí để ưu tiên đi xuống

        return baseCost;
    }


    PathNode GetLowestFCostNode(List<PathNode> nodeList)
    {
        PathNode best = nodeList[0];
        foreach (PathNode node in nodeList)
        {
            if (node.FCost < best.FCost)
                best = node;
        }
        return best;
    }

    List<Vector3> ReconstructPath(PathNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        PathNode current = endNode;
        while (current != null)
        {
            path.Add(current.worldPos);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    // Vẽ các đường nối giữa các node và đường đi từ agent đến mục tiêu (nếu có) trong Editor
    void OnDrawGizmos()
    {
        if (!drawPathGizmos || nodes == null)
            return;

        // Vẽ các kết nối giữa các node
        Gizmos.color = Color.gray;
        foreach (PathNode node in nodes)
        {
            foreach (PathNode neighbor in node.neighbors)
            {
                Gizmos.DrawLine(node.worldPos, neighbor.worldPos);
            }
        }

        // Nếu có agent và mục tiêu, tính và vẽ đường đi an toàn
        if (agentTransform != null && targetTransform != null)
        {
            List<Vector3> path = FindPath(agentTransform.position, targetTransform.position);
            if (path != null && path.Count > 1)
            {
                Gizmos.color = pathColor;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
    public Vector3 GetRandomNodePosition()
    {
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogWarning("Không có node nào để chọn!");
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, nodes.Count);
        return nodes[randomIndex].worldPos;
    }

    // Nội class Node
    class PathNode
    {
        public Vector3 worldPos;
        public List<PathNode> neighbors = new List<PathNode>();

        public float gCost = Mathf.Infinity;
        public float hCost;
        public float FCost => gCost + hCost;

        public PathNode parent;

        public PathNode(Vector3 pos)
        {
            worldPos = pos;
        }
    }
}
