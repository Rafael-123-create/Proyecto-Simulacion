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
        speed = 3f;
        health = 1;
        scoreValue = 10;
        
        // Adjust fire rate based on difficulty
        // Start slow (0.3 shots/sec = ~3.3s between shots), scale up to 1.5 shots/sec
        float difficultyScale = GetDifficultyScale();
        fireRate = Mathf.Lerp(0.2f, 0.8f, difficultyScale);
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
        
        // Move downward
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
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
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.speed = 12f; // Enemy bullet speed
                bullet.isPlayerBullet = false;
                bullet.damage = 1;
            }
            
            bool isVersus = GameManager.Instance != null && GameManager.Instance.IsVersusMode();
            if (isVersus)
            {
                bulletObj.layer = gameObject.layer;
            }
        }
    }
}
