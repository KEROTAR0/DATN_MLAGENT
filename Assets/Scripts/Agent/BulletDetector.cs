using UnityEngine;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(CapsuleCollider2D))]
public class BulletDetector : MonoBehaviour
{
    [Header("Bullet Detection")]
    public string bulletTag = "Bullet";  // Tag gán cho các đối tượng đạn
    private bool isBulletDetected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(bulletTag))
        {
            isBulletDetected = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(bulletTag))
        {
            isBulletDetected = false;
        }
    }

    // Trả về trạng thái phát hiện đạn
    public bool IsBulletDetected()
    {
        return isBulletDetected;
    }

    // Reset trạng thái phát hiện đạn (tuỳ vào bạn gọi khi nào)
    public void ResetDetection()
    {
        isBulletDetected = false;
    }

    // Gửi thông tin đạn cho Agent qua VectorSensor
    public void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isBulletDetected ? 1f : 0f);
    }
}
