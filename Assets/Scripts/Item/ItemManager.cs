using UnityEngine;
using System.Collections;

public class ItemManager : MonoBehaviour
{
    [Header("Tham chiếu và cấu hình")]
    public GridManager gridManager;
    public float respawnDelay = 1f;
    public float rewardAmount = 1f;

    private Collider2D itemCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (gridManager == null)
        {
            Debug.LogError("Chưa gán GridManager cho ItemManager!");
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Agent"))
        {
            Debug.Log("Nhặt item! Thêm phần thưởng +1");
            AgentController agentController = other.GetComponent<AgentController>();
            if (agentController != null)
            {
                agentController.PickUpItem();
            }

            StartCoroutine(RespawnItem());
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
        //spawnPos += new Vector3(0.5f, 0.5f, 0f);  // Dịch phải 0.5, lên 0.5
        transform.position = spawnPos;
    }
}
