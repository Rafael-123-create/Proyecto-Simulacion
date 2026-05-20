using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType type;
    public GameObject prefab;
    [Range(0f, 1f)]
    public float spawnWeight = 1f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
}

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 2f;
    public float difficultyIncreaseInterval = 30f;
    public float levelDuration = 60f;
    
    [Header("Difficulty Settings")]
    public float speedIncreasePerInterval = 0.2f;
    public float spawnRateIncreasePerInterval = 0.1f;
    
    private float currentSpawnTimer;
    private float difficultyTimer;
    private float levelTimer;
    private float baseMinInterval = 1f;
    private float baseMaxInterval = 3f;
    private bool levelEnded = false;
    
    // Public difficulty scale (0 to 1) that increases over time
    public float DifficultyScale { get; private set; } = 0f;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        Debug.Log("WaveSpawner: Starting with " + enemyTypes.Count + " enemy types and " + spawnPoints.Length + " spawn points");
        
        if (enemyTypes.Count > 0)
        {
            baseMinInterval = enemyTypes[0].minSpawnInterval;
            baseMaxInterval = enemyTypes[0].maxSpawnInterval;
            
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                bool prefabValid = enemyTypes[i].prefab != null;
                Debug.Log("WaveSpawner: EnemyType " + i + " - prefab valid: " + prefabValid + 
                         (prefabValid ? " (" + enemyTypes[i].prefab.name + ")" : ""));
            }
        }
        else
        {
            baseMinInterval = 1f;
            baseMaxInterval = 3f;
            Debug.LogWarning("WaveSpawner: No enemy types configured!");
        }
        
        currentSpawnTimer = initialSpawnDelay;
        difficultyTimer = 0f;
        levelTimer = 0f;
        levelEnded = false;
    }
    
    void Update()
    {
        if (levelEnded) return;
        
        levelTimer += Time.deltaTime;
        difficultyTimer += Time.deltaTime;
        
        DifficultyScale = Mathf.Min(1f, difficultyTimer / (difficultyIncreaseInterval * 3f));
        
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            difficultyTimer = 0f;
            IncreaseDifficulty();
        }
        
        if (levelTimer >= levelDuration)
        {
            levelEnded = true;
            Debug.Log("WaveSpawner: Level duration reached (" + levelDuration + "s). Completing level...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelComplete();
            }
            return;
        }
        
        currentSpawnTimer -= Time.deltaTime;
        if (currentSpawnTimer <= 0f)
        {
            SpawnRandomEnemy();
            ResetSpawnTimer();
        }
    }
    
    void SpawnRandomEnemy()
    {
        if (enemyTypes.Count == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveSpawner: Cannot spawn - enemyTypes=" + enemyTypes.Count + ", spawnPoints=" + spawnPoints.Length);
            return;
        }
        
        EnemySpawnData selected = null;
        int attempts = 0;
        while (attempts < 10)
        {
            selected = SelectWeightedRandom(enemyTypes);
            if (selected != null && selected.prefab != null) break;
            attempts++;
        }
        
        if (selected == null || selected.prefab == null)
        {
            Debug.LogWarning("WaveSpawner: No valid enemy selected after " + attempts + " attempts");
            return;
        }
        
        bool isVersus = GameManager.Instance != null && GameManager.Instance.IsVersusMode();
        
        if (isVersus)
        {
            // Check which players are alive
            bool p1Alive = GameManager.Instance.GetPlayerLives(1) > 0;
            bool p2Alive = GameManager.Instance.GetPlayerLives(2) > 0;
            
            int side;
            if (p1Alive && !p2Alive)
            {
                // Only player 1 is alive, spawn on left side
                side = 0;
                Debug.Log("WaveSpawner: Player 2 dead - spawning only on Player 1 side");
            }
            else if (!p1Alive && p2Alive)
            {
                // Only player 2 is alive, spawn on right side
                side = 1;
                Debug.Log("WaveSpawner: Player 1 dead - spawning only on Player 2 side");
            }
            else
            {
                // Both alive (or both dead, but game should be ending), spawn randomly
                side = Random.Range(0, 2);
            }
            
            Transform[] sideSpawnPoints = GetSideSpawnPoints(side);
            if (sideSpawnPoints.Length == 0)
            {
                Debug.LogWarning("WaveSpawner: No spawn points for side " + side);
                return;
            }
            Transform spawnPoint = sideSpawnPoints[Random.Range(0, sideSpawnPoints.Length)];
            GameObject enemyObj = Instantiate(selected.prefab, spawnPoint.position, Quaternion.identity);
            
            int enemyLayer = side == 0 ? VersusModeManager.Player1Layer : VersusModeManager.Player2Layer;
            enemyObj.layer = enemyLayer;
            
            Debug.Log("WaveSpawner: Spawned " + selected.type + " for Player " + (side + 1) + " at " + spawnPoint.position);
            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Initialize(selected.type);
            }
        }
        else
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyObj = Instantiate(selected.prefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("WaveSpawner: Spawned " + selected.type + " at " + spawnPoint.position);
            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Initialize(selected.type);
            }
        }
    }
    
    Transform[] GetSideSpawnPoints(int side)
    {
        List<Transform> sidePoints = new List<Transform>();
        foreach (Transform sp in spawnPoints)
        {
            if (side == 0 && sp.position.x <= 0f)
            {
                sidePoints.Add(sp);
            }
            else if (side == 1 && sp.position.x > 0f)
            {
                sidePoints.Add(sp);
            }
        }
        return sidePoints.ToArray();
    }
    
    void ResetSpawnTimer()
    {
        if (enemyTypes.Count == 0) return;
        
        // Use the selected enemy's interval or average? Let's use a global interval that we adjust.
        float minInterval = baseMinInterval - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        float maxInterval = baseMaxInterval - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        
        // Clamp intervals to reasonable values
        minInterval = Mathf.Max(0.2f, minInterval);
        maxInterval = Mathf.Max(minInterval + 0.2f, maxInterval);
        
        currentSpawnTimer = Random.Range(minInterval, maxInterval);
    }
    
    void IncreaseDifficulty()
    {
        Debug.Log("Difficulty increased!");
    }
    
    public void ResetLevel()
    {
        levelTimer = 0f;
        difficultyTimer = 0f;
        levelEnded = false;
        DifficultyScale = 0f;
    }
    
    EnemySpawnData SelectWeightedRandom(List<EnemySpawnData> list)
    {
        float totalWeight = 0f;
        foreach (EnemySpawnData data in list)
        {
            if (data.prefab != null)
                totalWeight += data.spawnWeight;
        }
        
        if (totalWeight <= 0f) return list.Count > 0 ? list[0] : null;
        
        float randomPoint = Random.value * totalWeight;
        foreach (EnemySpawnData data in list)
        {
            if (data.prefab == null) continue;
            if (randomPoint < data.spawnWeight)
                return data;
            randomPoint -= data.spawnWeight;
        }
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].prefab != null) return list[i];
        }
        return null;
    }
}
