using UnityEngine;

public class WindZone : MonoBehaviour
{
    public Vector2 windDirection = new Vector2(-1, 0); // 1 gió sang phải
    public float windStrength = 10f; // độ mạnh của gió

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Agent"))
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(windDirection.normalized * windStrength * Time.deltaTime, ForceMode2D.Force);
            }
        }
    }
}
