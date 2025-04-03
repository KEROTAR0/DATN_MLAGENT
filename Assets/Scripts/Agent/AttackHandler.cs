using System.Collections;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 8f; // Phạm vi phát hiện Player
    public LayerMask playerLayer;     // Layer của Player để phát hiện

    [Header("Melee Attack Settings")]
    public Transform meleeAttackPoint;      // Vị trí tấn công melee (Empty Object đặt ngoài Agent)
    public float meleeAttackRadius = 1f;      // Phạm vi attack melee
    public float meleeKnockbackForce = 10f;   // Lực knockback khi melee
    public float meleeCooldown = 0.5f;        // Thời gian chờ giữa các melee attack
    private float meleeCooldownTimer = 0f;
    public string meleeAnimTrigger = "melee"; // Tên trigger trong Animator cho melee

    [Header("Range Attack Settings")]
    public GameObject bulletPrefab;      // Prefab đạn
    public Transform bulletSpawnPoint;   // Vị trí spawn đạn
    public float bulletSpeed = 10f;        // Vận tốc đạn
    public float rangeKnockbackForce = 5f; // Lực knockback khi range attack
    public float rangeCooldown = 3f;       // Thời gian chờ giữa các range attack
    private float rangeCooldownTimer = 0f;
    public string rangeAnimTrigger = "range"; // Tên trigger trong Animator cho range

    [Header("Animation")]
    public Animator animator;

    // Tham chiếu đến Player (tìm theo tag "Player" trong Start)
    private Transform playerTransform;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        // Cập nhật cooldown cho melee và range
        if (meleeCooldownTimer > 0)
            meleeCooldownTimer -= Time.deltaTime;
        if (rangeCooldownTimer > 0)
            rangeCooldownTimer -= Time.deltaTime;

        // Nếu có Player, tự động phát hiện và tấn công
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            // Kiểm tra nếu Player nằm trong detectionRange và ở phía trước Agent (dựa vào dot product)
            float dot = Vector2.Dot(transform.right, dirToPlayer);
            if (distanceToPlayer <= detectionRange && dot > 0.5f)
            {
                if (distanceToPlayer <= meleeAttackRadius)
                {
                    if (meleeCooldownTimer <= 0f)
                    {
                        MeleeAttack();
                        meleeCooldownTimer = meleeCooldown;
                    }
                }
                else
                {
                    if (rangeCooldownTimer <= 0f)
                    {
                        RangeAttack();
                        rangeCooldownTimer = rangeCooldown;
                    }
                }
            }
        }
    }

    // Phương thức này trả về các quan sát cho AgentController
    public float[] GetObservations()
    {
        // Quan sát gồm vị trí tương đối (x, y) và khoảng cách đến Player
        if (playerTransform != null)
        {
            Vector3 relativePos = playerTransform.position - transform.position;
            float distance = relativePos.magnitude;
            return new float[] { relativePos.x, relativePos.y, distance };
        }
        return new float[] { 0f, 0f, 0f };
    }

    // Phương thức này xử lý hành động được truyền từ AgentController
    // Giả sử: actions[0] = 1: melee, 2: range
    public void ProcessAction(float[] actions)
    {
        if (actions.Length > 0)
        {
            int attackType = Mathf.FloorToInt(actions[0]);
            if (attackType == 1 && meleeCooldownTimer <= 0f)
            {
                MeleeAttack();
                meleeCooldownTimer = meleeCooldown;
            }
            else if (attackType == 2 && rangeCooldownTimer <= 0f)
            {
                RangeAttack();
                rangeCooldownTimer = rangeCooldown;
            }
        }
    }

    // Thực hiện tấn công melee
    void MeleeAttack()
    {
        if (animator != null)
            animator.SetTrigger(meleeAnimTrigger);

        // Kiểm tra va chạm với Player trong vùng melee
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            // Tính vector knockback hướng từ Agent đến Player
            Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
            // Giả sử Player có component (ví dụ: PlayerMovement) với phương thức ApplyKnockback
            var player = hit.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.ApplyKnockback(knockbackDir * meleeKnockbackForce);
            }
        }
    }

    // Thực hiện tấn công range
    void RangeAttack()
    {
        if (animator != null)
            animator.SetTrigger(rangeAnimTrigger);

        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            // Giả sử Agent hướng phải khi scale.x dương, trái nếu âm
            float direction = transform.localScale.x;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(bulletSpeed * direction, 0f);
            }
            // Thiết lập lực knockback cho đạn (đạn đẩy nhẹ hơn so với melee)
            BulletAgent bulletScript = bullet.GetComponent<BulletAgent>();
            if (bulletScript != null)
            {
                bulletScript.knockbackForce = rangeKnockbackForce;
            }
        }
    }

    // Vẽ phạm vi melee trong Editor để dễ kiểm tra
    void OnDrawGizmosSelected()
    {
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRadius);
        }
    }
}
