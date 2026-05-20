using UnityEngine;

public class KamikazeEnemy : EnemyController
{
    public float chargeAcceleration = 8f;
    public float chargeDistanceThreshold = 1f; // How close to player to start charging
    private Transform playerTarget;
    private bool isCharging = false;
    private Vector3 chargeDirection;
    
    public override void Initialize(EnemyType type)
    {
        base.Initialize(type);
        // Kamikaze specific initialization
        speed = 4f; // Base descent speed before charge
        health = 1;
        scoreValue = 20;
        
        // Find the closest player (in versus mode) or any player
        FindPlayerTarget();
    }
    
    void FindPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            playerTarget = null;
            return;
        }
        
        // Find closest player
        float closestDistance = Mathf.Infinity;
        Transform closest = null;
        
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player.transform;
            }
        }
        
        playerTarget = closest;
    }
    
    public override void Update()
    {
        if (!isInitialized || isDead) return;

        Vector3 moveDelta = Vector3.down * speed * Time.deltaTime;

        bool shouldFindNewTarget = false;

        if (playerTarget != null)
        {
            if (!playerTarget.gameObject.activeInHierarchy)
            {
                shouldFindNewTarget = true;
            }
        }
        else
        {
            shouldFindNewTarget = true;
        }

        if (shouldFindNewTarget)
        {
            FindPlayerTarget();
            if (playerTarget == null)
            {
                transform.Translate(moveDelta);
                CheckOffScreen();
                return;
            }
        }

        if (!isCharging)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer < chargeDistanceThreshold * 3f)
            {
                StartCharge();
            }
        }
        else
        {
            moveDelta = chargeDirection * chargeAcceleration * Time.deltaTime;
        }

        transform.Translate(moveDelta);
        CheckOffScreen();
    }

    void CheckOffScreen()
    {
        Camera cam = GetActiveCamera();
        if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize - 2f)
        {
            isDead = true;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            Destroy(gameObject);
        }
    }
    
    void StartCharge()
    {
        isCharging = true;
        // Calculate direction toward player's current position
        chargeDirection = (playerTarget.position - transform.position).normalized;
        // Optional: add visual/audio cue for charge start
        // For example, change color to red
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }

    protected override void UpdateBehavior()
    {
        // Kamikaze enemy behavior is handled in Update().
        // This override exists to satisfy the base class contract.
    }
    
    // Override Die to reset color if needed
    protected override void Die()
    {
        // Reset color if we changed it
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        base.Die();
    }
}
