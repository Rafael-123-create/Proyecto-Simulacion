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
    protected bool isDead = false;
    
    public virtual void Initialize(EnemyType type)
    {
        this.type = type;
        originalY = transform.position.y;
        isInitialized = true;
        
        // Set properties based on type
        switch (type)
        {
            case EnemyType.Shooter:
                speed = 3f;
                health = 1;
                scoreValue = 10;
                break;
            case EnemyType.Kamikaze:
                speed = 4f;
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
            isDead = true;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            Destroy(gameObject);
        }
    }
    
    protected Camera GetActiveCamera()
    {
        bool isVersus = GameManager.Instance != null && GameManager.Instance.IsVersusMode();
        
        if (isVersus)
        {
            Camera[] cameras = FindObjectsByType<Camera>();
            Camera bestCam = null;
            float closestDist = Mathf.Infinity;
            
            foreach (Camera cam in cameras)
            {
                if (cam.enabled && cam.gameObject.activeInHierarchy)
                {
                    float dist = Vector3.Distance(transform.position, cam.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestCam = cam;
                    }
                }
            }
            
            if (bestCam != null) return bestCam;
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
        if (isDead) return;
        isDead = true;

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
        if (isDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null && GameManager.Instance != null)
            {
                GameManager.Instance.TakeLife(player.playerNumber);
            }

            Die();
        }
    }
}
