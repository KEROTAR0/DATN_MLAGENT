using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explosionRadius = 2f;
    public float explosionForce = 10f;
    public float damage = 9f;
    public float explosionTimeAnimation = 0.25f;
    public float delayBeforeExplosion = 2f;
    private Rigidbody2D rb;
    private Animator animator;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("isGrounded", false);
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        if (rb.linearVelocity.y < 0) // Khi bom rơi xuống
        {
            animator.SetBool("isFalling", true);
            animator.SetBool("isGrounded", false);

        }
        else
            animator.SetBool("isFalling", false);

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        animator.SetBool("isFalling", false);
        animator.SetBool("isGrounded", true);
        
        if (!hasExploded)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(DelayBeforeExplosion());
        }
    }

    IEnumerator DelayBeforeExplosion()
    {
        yield return new WaitForSeconds(delayBeforeExplosion);
        StartCoroutine(Explode());
    }
    
    IEnumerator Explode()
    {
        hasExploded = true;
        animator.SetTrigger("kaboom"); // Chạy animation nổ

        yield return new WaitForSeconds(0.3f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D nearbyObject in colliders)
        {
            AgentController agent = nearbyObject.GetComponent<AgentController>();
            PlayerMovement player = nearbyObject.GetComponent<PlayerMovement>();
            PlayerHealth playerHealth = nearbyObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            if (player != null)
            {
                Vector2 forceDir = nearbyObject.transform.position - transform.position;
                player.ApplyKnockback(forceDir.normalized * explosionForce);
            }
            if (agent != null)
            {
                Vector2 forceDir = nearbyObject.transform.position - transform.position;
                agent.ApplyKnockback(forceDir.normalized * explosionForce);
            }
        }
        
        Destroy(gameObject, explosionTimeAnimation);
    }
}
