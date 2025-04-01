using UnityEngine;

public class BulletAgent : MonoBehaviour
{
    private float knockbackForce = 5f;  // Giá trị mặc định

    public void SetKnockback(float force)
    {
        knockbackForce = force;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<AgentController>()?.AddReward(0.2f);
            Vector2 knockback = new Vector2(transform.right.x * knockbackForce, knockbackForce / 3);
            //other.GetComponent<AgentController>()?.ApplyKnockback(knockback);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
