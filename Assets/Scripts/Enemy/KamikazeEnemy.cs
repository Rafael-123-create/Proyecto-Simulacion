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
        speed = 2f; // Base descent speed before charge
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
        if (!isInitialized) return;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 moveDelta = Vector2.down * speed * Time.deltaTime;
        
        bool shouldFindNewTarget = false;
        
        if (playerTarget != null)
        {
            // Check if target is still active
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
                // No active players, just move down
                if (rb != null)
                {
                    rb.MovePosition(rb.position + moveDelta);
                }
                else
                {
                    transform.Translate((Vector3)moveDelta);
                }
                return;
            }
        }
        
        if (!isCharging)
        {
            // Check if we should start charging
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer < chargeDistanceThreshold * 3f)
            {
                StartCharge();
            }
        }
        else
        {
            // Charging toward player's last known position
            moveDelta = (Vector2)chargeDirection * chargeAcceleration * Time.deltaTime;
        }
        
        // Move using Rigidbody2D for proper collision detection
        if (rb != null)
        {
            rb.MovePosition(rb.position + moveDelta);
        }
        else
        {
            transform.Translate((Vector3)moveDelta);
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
