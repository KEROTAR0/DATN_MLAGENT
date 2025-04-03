using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; 
    public Animator animator;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    public bool IsGrounded => isGrounded; 
    private bool isFalling;
    private int jumpCount;
    private Vector2 defaultPos;
    private bool isKnockback;
    public float knockbackTimer = 0;
    public float knockbackDuration = 0.5f;
    private bool canMove;

    // Start is called before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        jumpCount = maxJumps;
        defaultPos = transform.position;
        animator.SetBool("isFalling", false);
        animator.SetBool("isGrounded", true);
        animator.SetBool("isJumping", false);
        isKnockback = false;
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.W) && jumpCount > 0)
            Jump();

        // Kiểm tra nếu người chơi rơi xuống dưới y = -10 thì reset vị trí
        if (transform.position.y < -10)
            ResetPosition();

        // Cập nhật animation
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isFalling", isFalling);
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
                EnableMovement();
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }
    }
    
    void Move()
    {
        if (!canMove) return; // Nếu không thể di chuyển, thoát khỏi hàm
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Cập nhật animation trạng thái di chuyển
        animator.SetFloat("move", Mathf.Abs(moveInput));
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount--;
        isFalling = false;
        isGrounded = false;
    }
    
    void FixedUpdate()
    {
        CheckGrounded();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
            isFalling = false;
        }
        else if (rb.linearVelocity.y < 0)
        {
            isFalling = true;
            isGrounded = false;
        }
    }
    
    void ResetPosition()
    {
        transform.position = defaultPos;
        rb.linearVelocity = Vector2.zero; // Đặt lại vận tốc để tránh bị trôi
        jumpCount = maxJumps; // Reset số lần nhảy
        isGrounded = true;
        isFalling = false;
        animator.SetBool("isFalling", false);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
            isFalling = false;
        }
        else if (rb.linearVelocity.y < 0)
        {
            isFalling = true;
            isGrounded = false;
        }
    }
    public void ApplyKnockback(Vector2 force)
    {
        isKnockback = true;
        knockbackTimer = knockbackDuration;
        DisableMovement();

        float verticalMultiplier = 2.5f;
        Vector2 adjustedForce = new Vector2(force.x, force.y * verticalMultiplier);

        rb.linearVelocity = adjustedForce;
    }
    void DisableMovement()
    {
        canMove = false;
    }
    void EnableMovement()
    {
        canMove = true;
    }
}
