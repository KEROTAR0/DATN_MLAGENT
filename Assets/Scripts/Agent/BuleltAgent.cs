using UnityEngine;

public class BulletAgent : MonoBehaviour
{
    public LayerMask playerLayer;
    public float lifetime = 5f;
    public float knockbackForce = 5f;
    public float damage = 7f;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Vector2 direction = (other.transform.position - transform.position).normalized;
            PlayerMovement playerMovement = other.collider.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplyKnockback(direction * knockbackForce);
            }
            PlayerHealth playerHealth = other.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
