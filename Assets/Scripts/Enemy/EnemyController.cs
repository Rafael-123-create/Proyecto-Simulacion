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
        if (transform.position.y < Camera.main.orthographicSize * -1f - 2f)
        {
            Destroy(gameObject);
        }
    }
    
    protected virtual void UpdateMovement() { }
    protected virtual void UpdateBehavior() { }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // Notify game manager of score (optional, we can also do via collision)
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player bullets
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null && bullet.isPlayerBullet)
            {
                TakeDamage(bullet.damage);
                Destroy(other.gameObject); // Destroy bullet
            }
        }
        
        // Handle collision with player (for kamikaze)
        if (other.CompareTag("Player") && type == EnemyType.Kamikaze)
        {
            // Damage player (we'll implement player health later)
            Die(); // Enemy dies on impact
            // TODO: Apply damage to player
        }
    }
}
