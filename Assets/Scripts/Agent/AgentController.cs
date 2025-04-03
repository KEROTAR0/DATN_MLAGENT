using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.IO.Compression;
using System;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MovementHandler))]
public class AgentController : Agent
{
    private Vector2 initialPosition;

    public Transform itemTransform;
    public PathfindingManager pathfindingManager;
    public Tilemap tilemap; // Gán Tilemap trong Inspector
    public LayerMask tilemapLayer;
    public float stuckCheckInterval = 1f; // Kiểm tra mỗi 1 giây
    private float stuckCheckTimer = 0f;
    private MovementHandler movementHandler;
    private BulletDetector bulletDetector;
    private AttackHandler attackHandler;
    private bool isAttacking = false;
    private float attackCooldownTimer = 0f;
    public float attackCooldown = 0.5f;
    private float facingDirection = 1f;

    [Header("Rewards & Penalties")]
    public float rewardPerItem = 1f;
    public float fallPenalty = -1f;
    public float bulletPenalty = -0.25f;

    public float pathUpdateInterval = 0.5f;
    private float pathUpdateTimer = 0f;

    private bool hasPath = false; 
    Animator animator;

    [SerializeField] private bool isKnockback = false;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private float knockbackTimer = 0f;

    public override void Initialize()
    {
        movementHandler = GetComponent<MovementHandler>();
        bulletDetector = GetComponentInChildren<BulletDetector>();
        attackHandler = GetComponent<AttackHandler>();
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
    }
    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        movementHandler.ResetMovement();
        bulletDetector.ResetDetection();

        UpdatePath();
    }
    void Update()
    {
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            stuckCheckTimer = 0f;
            CheckIfStuck();
        }
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
                movementHandler.EnableMovement();
                Debug.Log("Hết Knockback, đã bật lại di chuyển.");
            }
            return;
        }

        if (isAttacking)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                isAttacking = false;
                movementHandler.EnableMovement();
                Debug.Log("Hết thời gian tấn công, đã bật lại di chuyển.");
            }
            return;
        }
        PerformAttack();

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 toItem = itemTransform.position - transform.position;
        sensor.AddObservation(toItem.x);
        sensor.AddObservation(toItem.y);

        movementHandler.CollectObservations(sensor);
        bulletDetector.CollectObservations(sensor);
        attackHandler.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        movementHandler.Move(moveInput);
        animator.SetFloat("moving", Mathf.Abs(moveInput)); 
        pathUpdateTimer += Time.deltaTime;
        
        int discreteAction = actions.DiscreteActions[0];
        switch (discreteAction)
        {
            case 1:
                facingDirection *= -1f;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                transform.localScale = scale;
                break;
            case 2:
                if (bulletDetector.IsBulletDetected())
                {
                    Debug.Log("action 2");
                    movementHandler.PerformJump();
                }
                break;
            case 3:
                if (movementHandler.IsGrounded)
                {
                    Debug.Log("action 3");
                    movementHandler.Move(facingDirection);
                    movementHandler.PerformJump();
                }
                break;
            case 4:
                PerformAttack();
                Debug.Log("action 4");
                break;
        }

        if (pathUpdateTimer >= pathUpdateInterval)
        {
            UpdatePath();
            pathUpdateTimer = 0f;
        }
        if (hasPath)
        {
            movementHandler.FollowPath();
        }
        else
        {
            movementHandler.MoveRandom();
        }

        if (transform.position.y < -10f)
        {
            AddReward(fallPenalty);
            EndEpisode();
        }
        AddReward(-0.001f); // Phạt mỗi bước đi
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        //continuousActions[0] = Input.GetAxis("Horizontal");
        discreteActions[0] = 0; // Không có hành động nào mặc định

        if (Input.GetKeyDown(KeyCode.F))
        {
            discreteActions[0] = 4; // Tấn công
        }
    }
    void UpdatePath()
    {
        if (pathfindingManager == null || itemTransform == null) 
        {
            Debug.LogWarning("pathfindingManager hoặc itemTransform bị null!");
            return;
        }

        List<Vector3> path = pathfindingManager.FindPath(transform.position, itemTransform.position);
        if (path != null && path.Count > 1)
        {
            Debug.Log($"Tìm thấy đường đi đến item! Số điểm: {path.Count}");
            movementHandler.SetPath(path);
            hasPath = true;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy đường đi!");
            hasPath = false;  // Không có đường
        }
    }
    public void OnHitByBullet()
    {
        AddReward(bulletPenalty);
    }
    public void ApplyKnockback(Vector2 force)
    {
        isKnockback = true;
        knockbackTimer = knockbackDuration;
        movementHandler.DisableMovement();

        float verticalMultiplier = 2.5f; 
        Vector2 adjustedForce = new Vector2(force.x, force.y * verticalMultiplier);

        GetComponent<Rigidbody2D>().AddForce(adjustedForce, ForceMode2D.Impulse);
    }
    private void PerformAttack()
    {
        attackHandler.TryAttack();
        isAttacking = true;
        attackCooldownTimer = attackCooldown;
        movementHandler.DisableMovement();

        animator.SetTrigger("attack");
    }
    public void PickUpItem()
    {
        AddReward(1f);
        Debug.Log("Item picked up, reward added!");
        EndEpisode(); 
    }
    void CheckIfStuck()
    {
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        if (IsTileOccupied(cellPos))
        {
            Debug.LogWarning("Agent bị kẹt trong Tilemap! Dịch chuyển về vị trí ban đầu.");
            transform.position = initialPosition;
        }
    }
    bool IsTileOccupied(Vector3Int cellPos)
    {
        TileBase tile = tilemap.GetTile(cellPos);
        return tile != null; // Nếu có tile ở vị trí này, tức là bị kẹt
    }
}
