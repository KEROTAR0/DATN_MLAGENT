using UnityEngine;

public class Cannon : MonoBehaviour
{
    public GameObject bombPrefab;   // Prefab của quả bom
    public Transform throwPoint;    // Vị trí ném bom
    public float minThrowForce = 5f; 
    public float maxThrowForce = 15f;    
    public float throwAngle = 45f;  // Góc ném bom (tính theo độ)
    public float throwInterval = 3f; // Thời gian giữa các lần ném bom

    private Transform target;       // Vị trí của người chơi
    private float nextThrowTime;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Agent").transform;
        nextThrowTime = Time.time + throwInterval;
    }

    void Update()
    {
        if (Time.time >= nextThrowTime)
        {
            ThrowBomb();
            
            nextThrowTime = Time.time + throwInterval;
        }
    }

    void ThrowBomb()
    {
        float throwForce = Random.Range(minThrowForce, maxThrowForce);

        if (bombPrefab != null && target != null)
        {
            GameObject bomb = Instantiate(bombPrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            // Tính toán hướng ném dựa vào vị trí người chơi
            Vector2 direction = (target.position - throwPoint.position).normalized;

            // Chuyển đổi góc từ độ sang radian
            float angleRad = throwAngle * Mathf.Deg2Rad;

            // Tạo vận tốc với góc ném lên trời
            Vector2 throwVelocity = new Vector2(direction.x * Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * throwForce;

            // Gán vận tốc cho bom
            rb.linearVelocity = throwVelocity;
        }
    }
}
