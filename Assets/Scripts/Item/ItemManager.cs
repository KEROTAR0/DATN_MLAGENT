using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    [Header("Tham chiếu và cấu hình")]
    public GridManager gridManager;
    public float respawnDelay = 1f;
    public ItemType itemType;
    
    // UI để hiển thị số Treasure còn lại (chỉ áp dụng với item Treasure)
    public Text treasureRemainingText;

    // Sử dụng biến static để đếm số Treasure đã nhặt được trong stage hiện tại
    public static int collectedTreasureCount = 0;
    // Giới hạn cho Stage1 và Stage2
    public int treasureLimit = 15;

    private Collider2D itemCollider;
    private SpriteRenderer spriteRenderer;

    public enum ItemType
    {
        Treasure,
        Health
    }

    void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (gridManager == null)
        {
            Debug.LogError("Chưa gán GridManager cho ItemManager!");
            return;
        }

        // Reset đếm Treasure khi bắt đầu Stage1 hoặc Stage2
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Stage1" || sceneName == "Stage2")
        {
            collectedTreasureCount = 0;
            UpdateTreasureUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Xử lý khi Agent nhặt item
        if (other.CompareTag("Agent"))
        {
            Debug.Log("Nhặt item! Thêm phần thưởng +1");
            AgentController agentController = other.GetComponent<AgentController>();

            if (agentController != null)
            {
                agentController.PickUpItem();

                if (itemType == ItemType.Treasure)
                {
                    if (sceneName == "Stage3")
                    {
                        Debug.Log("Agent nhặt Treasure (Stage3)");
                        StartCoroutine(RespawnItem());
                    }
                    else
                    {
                        if (collectedTreasureCount < treasureLimit)
                        {
                            collectedTreasureCount++;
                            Debug.Log("Agent nhặt Treasure! Số Treasure còn lại: " + (treasureLimit - collectedTreasureCount));
                            UpdateTreasureUI();
                            StartCoroutine(RespawnItem());

                            // Nếu hết Treasure, chuyển màn
                            if (collectedTreasureCount >= treasureLimit)
                            {
                                GameManager gameManager = FindFirstObjectByType<GameManager>();
                                if (gameManager != null)
                                {
                                    gameManager.SavePlayerDataForScene();
                                    Debug.Log("Agent góp phần nhặt đủ Treasure. Chuyển màn.");
                                }

                                if (sceneName == "Stage1")
                                    SceneManager.LoadScene("Stage2");
                                else if (sceneName == "Stage2")
                                    SceneManager.LoadScene("Stage3");
                            }
                        }
                        else
                        {
                            Debug.Log("Đã đạt giới hạn Treasure (Agent).");
                            StartCoroutine(RespawnItem());
                        }
                    }
                }
                else if (itemType == ItemType.Health)
                {
                    Debug.Log("Agent không cần máu. Bỏ qua item Health.");
                    StartCoroutine(RespawnItem());
                }
            }
        }

        // Xử lý khi Player nhặt item
        if (other.CompareTag("Player"))
        {
            PlayerScore playerScore = other.GetComponent<PlayerScore>();
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (itemType == ItemType.Treasure)
            {
                if (sceneName == "Stage3")
                {
                    Debug.Log("Nhặt Treasure (Stage3)! +100 điểm");
                    playerScore?.AddScore(100);
                    StartCoroutine(RespawnItem());
                }
                else
                {
                    if (collectedTreasureCount < treasureLimit)
                    {
                        collectedTreasureCount++;
                        Debug.Log("Nhặt Treasure! +100 điểm. Số Treasure còn lại: " + (treasureLimit - collectedTreasureCount));
                        playerScore?.AddScore(100);
                        UpdateTreasureUI();
                        StartCoroutine(RespawnItem());

                        if (collectedTreasureCount >= treasureLimit)
                        {
                            GameManager gameManager = FindFirstObjectByType<GameManager>();
                            if (gameManager != null)
                            {
                                gameManager.SavePlayerDataForScene();
                                Debug.Log("Đã lưu player data.");
                            }

                            Debug.Log("Đã nhặt đủ Treasure. Chuyển sang màn tiếp theo.");
                            if (sceneName == "Stage1")
                                SceneManager.LoadScene("Stage2");
                            else if (sceneName == "Stage2")
                                SceneManager.LoadScene("Stage3");
                        }
                    }
                    else
                    {
                        Debug.Log("Đã đạt giới hạn Treasure trong stage này.");
                        StartCoroutine(RespawnItem());
                    }
                }
            }
            else if (itemType == ItemType.Health)
            {
                if (playerHealth != null)
                {
                    if (playerHealth.currentHealth < playerHealth.maxHealth)
                    {
                        playerHealth.Heal(10);
                        Debug.Log("Nhặt Health! +10 máu");
                    }
                    else
                    {
                        playerScore?.AddScore(50);
                        Debug.Log("Máu đầy! +50 điểm thay vì +10 máu");
                    }
                }
                StartCoroutine(RespawnItem());
            }
        }
    }


    IEnumerator RespawnItem()
    {
        itemCollider.enabled = false;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        SetRandomPosition();

        itemCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    void SetRandomPosition()
    {
        Vector3 spawnPos = gridManager.GetRandomPosition();
        transform.position = spawnPos;
    }

    /// <summary>
    /// Cập nhật UI hiển thị số Treasure còn lại.
    /// Chỉ cập nhật nếu có UI Text được gán.
    /// </summary>
    void UpdateTreasureUI()
    {
        if (treasureRemainingText != null)
        {
            int remaining = treasureLimit - collectedTreasureCount;
            treasureRemainingText.text = "Treasure remaining: " + remaining;
        }
    }
}
