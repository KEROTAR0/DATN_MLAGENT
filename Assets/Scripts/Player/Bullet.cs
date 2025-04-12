using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f;
    public float knockbackForce = 5f;
    public LayerMask agentLayer;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Nếu vận tốc khác 0, xoay viên đạn theo hướng di chuyển
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & agentLayer) != 0)
        {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            AgentController agent = collision.collider.GetComponent<AgentController>();
            if (agent != null)
            {
                agent.ApplyKnockback(direction * knockbackForce);
            }
        }
        Destroy(gameObject);
    }
}
