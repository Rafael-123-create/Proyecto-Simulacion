using UnityEngine;

public enum EnemyType
{
    Shooter,
    Kamikaze
}

public abstract class EnemyController : MonoBehaviour
{
    public EnemyType type;
    public float speed = 2f;
    public int health = 1;
    public int scoreValue = 10;
    
    [Header("References")]
    public GameObject explosionPrefab;
    
    // Internal
    private float originalY;
    protected bool isInitialized = false;
    
    public virtual void Initialize(EnemyType type)
    {
        this.type = type;
        originalY = transform.position.y;
        isInitialized = true;
        
        // Set properties based on type
        switch (type)
        {
            case EnemyType.Shooter:
                speed = 1.5f;
                health = 1;
                scoreValue = 10;
                break;
            case EnemyType.Kamikaze:
                speed = 3f;
                health = 1;
                scoreValue = 20;
                break;
        }
    }
    
    public virtual void Update()
    {
        if (!isInitialized) return;
        
        UpdateMovement();
        UpdateBehavior();
        
        // Check if off screen (bottom)
        Camera cam = GetActiveCamera();
        if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize - 2f)
        {
            Destroy(gameObject);
        }
    }
    
    Camera GetActiveCamera()
    {
        // Try to find an active camera
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
    
    protected virtual void UpdateMovement() { }
    protected virtual void UpdateBehavior() { }
    
    public void TakeDamage(int damage, int fromPlayerNumber = 1)
    {
        health -= damage;
        lastHitByPlayer = fromPlayerNumber;
        if (health <= 0)
        {
            Die();
        }
    }
    
    private int lastHitByPlayer = 1;
    
    protected virtual void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDestroyed(type, lastHitByPlayer);
        }
        
        Destroy(gameObject);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;
        
        Debug.Log("EnemyController: OnCollisionEnter2D with " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ", layer: " + collision.gameObject.layer + ")");
        
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null && bullet.isPlayerBullet)
            {
                Debug.Log("EnemyController: Hit by player bullet from Player " + bullet.ownerPlayerNumber);
                TakeDamage(bullet.damage, bullet.ownerPlayerNumber);
                Destroy(collision.gameObject);
            }
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Debug.Log("EnemyController: Found PlayerController: " + (player != null ? "YES, playerNumber=" + player.playerNumber : "NO"));
            
            if (player != null && GameManager.Instance != null)
            {
                Debug.Log("EnemyController: Calling TakeLife for Player " + player.playerNumber);
                GameManager.Instance.TakeLife(player.playerNumber);
            }
            else if (GameManager.Instance == null)
            {
                Debug.LogError("EnemyController: GameManager.Instance is NULL!");
            }
            
            Die();
        }
    }
}
