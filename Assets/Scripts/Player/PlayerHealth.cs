using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI Settings")]
    // Image fill của thanh máu (đảm bảo Image có kiểu Fill Amount)
    public Image healthBarFill;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthBar();

    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10f);
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            Heal(10f);
        }
    }
    /// <summary>
    /// Hàm nhận sát thương.
    /// </summary>
    /// <param name="damage">Lượng sát thương nhận vào.</param>
    public void TakeDamage(float damage)
    {
        animator.SetTrigger("hurt");
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
    }
    /// <summary>
    /// Cập nhật UI thanh máu dựa trên tỉ lệ máu còn lại.
    /// </summary>
    public void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    /// <summary>
    /// Xử lý khi người chơi chết.
    /// </summary>
    void Die()
    {
        animator.SetTrigger("die");
        Debug.Log("Player died");
        // Thực hiện các xử lý khi người chơi chết, ví dụ: reload scene hoặc hiển thị game over.
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}
