using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public bool isPlayerBullet = true;
    public int ownerPlayerNumber = 1; // Set false for enemy bullets
    
    void Update()
    {
        // Move using Rigidbody2D for proper collision detection
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector3 direction = isPlayerBullet ? Vector3.up : Vector3.down;
        
        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)direction * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
        
        // Destroy if off screen
        Camera cam = GetActiveCamera();
        if (cam != null)
        {
            float topBound = cam.transform.position.y + cam.orthographicSize + 2f;
            float bottomBound = cam.transform.position.y - cam.orthographicSize - 2f;
            
            if (isPlayerBullet && transform.position.y > topBound)
            {
                Destroy(gameObject);
            }
            else if (!isPlayerBullet && transform.position.y < bottomBound)
            {
                Destroy(gameObject);
            }
        }
    }
    
    Camera GetActiveCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy)
            {
                return cam;
            }
        }
        return Camera.main;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;
        
        if (isPlayerBullet)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, ownerPlayerNumber);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null && GameManager.Instance != null)
                {
                    GameManager.Instance.TakeLife(player.playerNumber);
                    Debug.Log("Bullet: Player " + player.playerNumber + " hit by enemy bullet");
                }
                Destroy(gameObject);
            }
        }
    }
}
