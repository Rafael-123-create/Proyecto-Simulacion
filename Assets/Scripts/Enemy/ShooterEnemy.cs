using UnityEngine;

public class ShooterEnemy : EnemyController
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    private float nextFireTime;
    
    public override void Initialize(EnemyType type)
    {
        base.Initialize(type);
        // Shooter specific initialization
        speed = 1.5f;
        health = 1;
        scoreValue = 10;
        
        // Adjust fire rate based on difficulty
        // Start slow (0.3 shots/sec = ~3.3s between shots), scale up to 1.5 shots/sec
        float difficultyScale = GetDifficultyScale();
        fireRate = Mathf.Lerp(0.3f, 1.5f, difficultyScale);
    }
    
    float GetDifficultyScale()
    {
        if (WaveSpawner.Instance != null)
        {
            return WaveSpawner.Instance.DifficultyScale;
        }
        return 0f;
    }
    
    public override void Update()
    {
        base.Update();
        if (!isInitialized) return;
        
        // Move downward using Rigidbody2D for proper collision detection
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.MovePosition(rb.position + Vector2.down * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
        
        // Shooting logic
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // Instantiate enemy bullet (moving downward)
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.speed = 5f; // Enemy bullet speed
                bullet.isPlayerBullet = false;
                bullet.damage = 1;
            }
            // Optional: add muzzle flash effect
        }
    }
}
