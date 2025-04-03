using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementHandler : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public int maxJumpCount = 2;

    [Header("Cooldown Settings")]
    public float jumpCooldown = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    [Header("Path follow")]
    public float pathPointThreshold = 0.1f;

    private Rigidbody2D rb;
    private int currentJumpCount = 0;
    private float lastJumpTime = -10f;
    private bool isGrounded;
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            currentJumpCount = 0;
        }
    }

    // Di chuyển trái/phải
    public void Move(float direction)
    {
        if (!canMove)
        {
            Debug.Log("Agent canMove = false.");
            return;
        }
        Vector2 velocity = rb.linearVelocity;
        velocity.x = direction * moveSpeed;
        rb.linearVelocity = velocity;
    }
    public void DisableMovement()
    {
        canMove = false;
    }

    public void EnableMovement()
    {
        canMove = true;
    }
    public void MoveRandom()
    {
        // Di chuyển trái hoặc phải ngẫu nhiên
        float randomDirection = Random.Range(-1f, 1f);
        Move(randomDirection);
        Debug.Log("Di chuyển ngẫu nhiên");
    }
    // Nhảy thẳng đứng hoặc né đạn
    public void PerformJump()
    {
        if (Time.time - lastJumpTime >= jumpCooldown && currentJumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentJumpCount++;
            lastJumpTime = Time.time;
        }
    }

    // Nhảy qua phía trước (vừa nhảy vừa di chuyển)
    public void JumpForward(float direction)
    {
        if (Time.time - lastJumpTime >= jumpCooldown && currentJumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            Vector2 jumpDirection = new Vector2(direction, 1f).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
            currentJumpCount++;
            lastJumpTime = Time.time;
        }
    }
    public void SetPath(List<Vector3> newPath)
    {
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("Đường đi rỗng, agent không thể di chuyển.");
            return;
        }
        currentPath.Clear(); 
        currentPath.AddRange(newPath); 
        currentPathIndex = 0;
        Debug.Log($"Agent đã nhận đường đi, tổng điểm: {currentPath.Count}");
    }


    public void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) 
        {
            Debug.LogWarning("Không có đường đi để theo!");
            return;
        }

        Vector3 targetPos = currentPath[currentPathIndex];
        Vector2 direction = (targetPos - transform.position).normalized;

        // Di chuyển trái/phải
        Move(direction.x);

        // Nếu khoảng cách nhỏ hơn ngưỡng, sang điểm tiếp theo
        if (Vector2.Distance(transform.position, targetPos) < pathPointThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                currentPath.Clear();  // Hết path
            }
        }
    }

    public void ResetMovement()
    {
        rb.linearVelocity = Vector2.zero;
        currentJumpCount = 0;
        lastJumpTime = -10f;
        canMove = true;
    }

    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public bool IsGrounded => isGrounded;

    public void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.y);
        sensor.AddObservation(isGrounded ? 1f : 0f);
        sensor.AddObservation(currentJumpCount);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
