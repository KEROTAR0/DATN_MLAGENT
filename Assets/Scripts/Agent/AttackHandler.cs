using System.Collections;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class AttackHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float meleeRange = 1.5f;
    public float attackCooldown = 1f;
    public float attackRange = 8f;

    [Header("Forces")]
    public float meleeForce = 10f;
    public float rangeForce = 5f;

    private bool canAttack = true;
    private Transform target; // Đối tượng Player cần tấn công

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void CollectObservations(VectorSensor sensor)
    {
        if (target == null)
        {
            sensor.AddObservation(Vector2.zero); // Không có player
            return;
        }

        Vector2 directionToTarget = target.position - transform.position;
        sensor.AddObservation(directionToTarget.normalized);
        sensor.AddObservation(Vector2.Distance(transform.position, target.position));
    }

    public void TryAttack()
    {
        if (!canAttack || target == null) return;

        Vector2 direction = transform.right;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, attackRange, LayerMask.GetMask("Player"));

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            float distance = Vector2.Distance(transform.position, hit.collider.transform.position);

            if (distance > 3f) 
                StartCoroutine(PerformRangeAttack(hit.collider));
            else 
                StartCoroutine(PerformMeleeAttack(hit.collider));
        }
    }

    private IEnumerator PerformRangeAttack(Collider2D player)
    {
        canAttack = false;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * bulletSpeed;

        BulletAgent bulletScript = bullet.GetComponent<BulletAgent>();
        bulletScript.SetKnockback(rangeForce);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator PerformMeleeAttack(Collider2D player)
    {
        canAttack = false;
        AgentController agent = player.GetComponent<AgentController>();
        if (agent != null)
        {
            Vector2 knockback = new Vector2(transform.right.x * meleeForce, meleeForce / 2);
            //agent.ApplyKnockback(knockback);
            agent.AddReward(0.3f);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, firePoint.position + transform.right * 6f);
    }
}
