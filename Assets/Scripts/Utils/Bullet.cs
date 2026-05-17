using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public bool isPlayerBullet = true; // Set false for enemy bullets
    
    void Update()
    {
        // Move upward (positive Y)
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        
        // Destroy if off screen (top)
        if (transform.position.y > Camera.main.orthographicSize + 2f)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Optional: handle collisions here or let other scripts handle via tags
        // For simplicity, we'll let Enemy and Player handle collisions via their own triggers
        // But we can destroy bullet when hitting something
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            // If bullet hits enemy or player, destroy it
            Destroy(gameObject);
        }
    }
}
