using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    public Transform meleeAttackPoint;      // Vị trí tấn công melee (Empty Object đặt bên ngoài tay người chơi)
    public float meleeAttackRadius = 1f;      // Phạm vi tấn công melee
    public float meleeKnockbackForce = 10f;   // Lực đẩy khi tấn công melee
    public LayerMask agentLayer;            // Layer chứa các Agent cần bị tấn công
    public float meleeCooldown = 0.5f;        // Cooldown cho melee
    private float meleeCooldownTimer = 0f;    // Bộ đếm cooldown melee
    public string meleeAnimTrigger = "melee"; // Tên trigger trong Animator cho melee

    [Header("Range Attack Settings")]
    public GameObject bulletPrefab;         // Prefab của đạn
    public Transform bulletSpawnPoint;      // Vị trí spawn đạn (ví dụ: súng hoặc tay người chơi)
    public float bulletSpeed = 10f;         // Vận tốc của đạn
    public float rangeKnockbackForce = 5f;    // Lực đẩy khi đạn trúng Agent
    public float rangeCooldown = 3f;          // Cooldown cho range attack
    private float rangeCooldownTimer = 0f;    // Bộ đếm cooldown range
    public string rangeAnimTrigger = "range";// Tên trigger trong Animator cho range
    private float waitRangeAnimation = 0.2f;

    [Header("UI Settings")]
    public Text meleeCooldownText;          // UI Text hiển thị cooldown melee
    public Text rangeCooldownText;          // UI Text hiển thị cooldown range

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Cập nhật bộ đếm cooldown cho melee và range
        if (meleeCooldownTimer > 0)
            meleeCooldownTimer -= Time.deltaTime;
        if (rangeCooldownTimer > 0)
            rangeCooldownTimer -= Time.deltaTime;

        // Hiển thị cooldown trên UI (làm tròn đến 1 chữ số thập phân)
        if (meleeCooldownText != null)
            meleeCooldownText.text = meleeCooldownTimer > 0 ? meleeCooldownTimer.ToString("F1") : "Ready";
        if (rangeCooldownText != null)
            rangeCooldownText.text = rangeCooldownTimer > 0 ? rangeCooldownTimer.ToString("F1") : "Ready";

        // Xử lý input tấn công melee (ví dụ phím J)
        if (Input.GetMouseButtonDown(0) && meleeCooldownTimer <= 0)
        {
            StartCoroutine(MeleeAttack());
            meleeCooldownTimer = meleeCooldown;
        }
        
        // Xử lý input tấn công range (ví dụ phím K)
        if (Input.GetMouseButtonDown(1) && rangeCooldownTimer <= 0)
        {
            StartCoroutine(RangeAttack());
            rangeCooldownTimer = rangeCooldown;
        }
    }

    IEnumerator MeleeAttack()
    {
        // Kích hoạt animation melee
        animator.SetTrigger(meleeAnimTrigger);
        
        // Thêm delay nhỏ để đồng bộ với animation (nếu cần)
        yield return new WaitForSeconds(0.1f);
        
        // Kiểm tra các Agent trong phạm vi melee
        Collider2D[] hitAgents = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRadius, agentLayer);
        foreach (Collider2D agentCollider in hitAgents)
        {
            Vector2 direction = (agentCollider.transform.position - transform.position).normalized;
            AgentController agent = agentCollider.GetComponent<AgentController>();
            if (agent != null)
            {
                agent.ApplyKnockback(direction * meleeKnockbackForce);
            }
        }
    }

    IEnumerator RangeAttack()
    {
        // Kích hoạt animation range
        animator.SetTrigger(rangeAnimTrigger);

        yield return new WaitForSeconds(waitRangeAnimation);
        
        // Tạo đạn từ prefab tại vị trí spawn
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            
            // Xác định hướng bắn dựa trên scale X của người chơi (đối mặt trái/phải)
            float direction = transform.localScale.x;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(bulletSpeed * direction, 0f);
            }

            // Thiết lập lực knockback cho đạn (đạn đẩy lùi ít hơn)
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.knockbackForce = rangeKnockbackForce;
            }
        }
    }

    // Vẽ phạm vi tấn công melee trong Editor
    void OnDrawGizmosSelected()
    {
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRadius);
        }
    }
}
