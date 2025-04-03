using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f;
    public float knockbackForce = 5f;
    public LayerMask agentLayer;

    void Start()
    {
        Destroy(gameObject, lifetime);
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
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
