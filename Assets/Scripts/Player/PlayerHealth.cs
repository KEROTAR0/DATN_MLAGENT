using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI Settings")]
    public Image healthBarFill;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f;
    private bool isInvincible = false;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (sceneName == "Stage2" || sceneName == "Stage3")
        {
            currentHealth = gameManager.persistentHealth;
            UpdateHealthBar();
        }
        else
            currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthBar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Heal(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        animator.SetTrigger("hurt");
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        animator.SetTrigger("die");
        Debug.Log("Player died");
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GameManager.Instance.ShowGameOver();
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}
