using UnityEngine;

public class BulletImpactHandler : MonoBehaviour
{
    public float knockbackForceGrounded = 5f;
    public float knockbackForceAirborne = 10f;

    private Rigidbody2D rb;
    private MovementHandler movementHandler;
    private AgentController agentController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementHandler = GetComponent<MovementHandler>();
        agentController = GetComponent<AgentController>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            Vector2 bulletDirection = (transform.position - collision.transform.position).normalized;

            // ⚡️ Reset velocity trước khi đẩy
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            float force = movementHandler.IsGrounded ? knockbackForceGrounded : knockbackForceAirborne;
            rb.AddForce(bulletDirection * force, ForceMode2D.Impulse);

            // Phạt reward
            if (agentController != null)
            {
                agentController.OnHitByBullet();
            }

            Destroy(collision.gameObject);
        }
    }
}
