using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 5;
    private Rigidbody2D rb;
    public GameObject owner;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Kiểm tra nếu vật chạm vào có Script PlayerHealth (không cần check Tag nữa cho chắc)
        PlayerHealth health = hitInfo.GetComponent<PlayerHealth>();

        if (health != null && hitInfo.gameObject != owner)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }

        // Nếu chạm đất
        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}