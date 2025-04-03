using UnityEngine;

public class BulletAgent : MonoBehaviour
{
    public LayerMask playerLayer;
    public float lifetime = 5f;
    public float knockbackForce = 5f;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Vector2 direction = (other.transform.position - transform.position).normalized;
            PlayerMovement player = other.collider.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.ApplyKnockback(direction * knockbackForce);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
