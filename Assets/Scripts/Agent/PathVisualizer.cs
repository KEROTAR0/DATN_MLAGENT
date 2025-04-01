using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour
{
    public Transform agentTransform;
    public Transform targetTransform;
    public PathfindingManager pathfindingManager;
    public float updateInterval = 0.5f;
    
    private LineRenderer lineRenderer;
    private float timer = 0f;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= updateInterval)
        {
            UpdatePath();
            timer = 0f;
        }
    }
    
    void UpdatePath()
    {
        if(agentTransform == null || targetTransform == null || pathfindingManager == null) return;
        
        List<Vector3> path = pathfindingManager.FindPath(agentTransform.position, targetTransform.position);
        if(path != null && path.Count > 1)
        {
            lineRenderer.positionCount = path.Count;
            for(int i = 0; i < path.Count; i++)
            {
                lineRenderer.SetPosition(i, path[i]);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
