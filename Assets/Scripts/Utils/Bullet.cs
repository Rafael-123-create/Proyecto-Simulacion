using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public bool isPlayerBullet = true;
    public int ownerPlayerNumber = 1;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        Vector3 direction = isPlayerBullet ? Vector3.up : Vector3.down;
        transform.Translate(direction * speed * Time.deltaTime);

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;
        if (Time.time - spawnTime < 0.1f) return;

        Bullet otherBullet = other.GetComponent<Bullet>();
        if (otherBullet != null)
        {
            Destroy(gameObject);
            Destroy(otherBullet.gameObject);
            return;
        }

        if (isPlayerBullet)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, ownerPlayerNumber);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null && GameManager.Instance != null)
                {
                    GameManager.Instance.TakeLife(player.playerNumber);
                }
                Destroy(gameObject);
            }
        }
    }
}
