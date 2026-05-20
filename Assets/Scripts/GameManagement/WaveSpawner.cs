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
    
    [Header("Level Settings")]
    public int[] scoreThresholds = { 500, 1200, 2500 };
    
    [Header("Difficulty Settings")]
    public float speedIncreasePerInterval = 0.2f;
    public float spawnRateIncreasePerInterval = 0.1f;
    public int maxConcurrentEnemies = 15;
    
    private float currentSpawnTimer;
    private float difficultyTimer;
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
        levelEnded = false;
    }
    
    void Update()
    {
        if (levelEnded) return;
        
        difficultyTimer += Time.deltaTime;
        
        DifficultyScale = Mathf.Min(1f, difficultyTimer / (difficultyIncreaseInterval * 3f));
        
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            difficultyTimer = 0f;
            IncreaseDifficulty();
        }
        
        int currentLevel = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 0;
        int totalScore = GameManager.Instance != null ? GameManager.Instance.GetPlayerScore(1) + GameManager.Instance.GetPlayerScore(2) : 0;
        
        if (currentLevel < scoreThresholds.Length && totalScore >= scoreThresholds[currentLevel])
        {
            levelEnded = true;
            Debug.Log("WaveSpawner: Score threshold reached! Level " + (currentLevel + 1) + " complete (Score: " + totalScore + "/" + scoreThresholds[currentLevel] + ")");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelComplete();
            }
            return;
        }
        
        int enemyCount = FindObjectsByType<EnemyController>().Length;
        if (enemyCount >= maxConcurrentEnemies) return;
        
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
        
        int currentLevel = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 0;
        float levelMultiplier = 1f - (currentLevel * 0.15f);
        
        float minInterval = baseMinInterval * levelMultiplier - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        float maxInterval = baseMaxInterval * levelMultiplier - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        
        minInterval = Mathf.Max(0.15f, minInterval);
        maxInterval = Mathf.Max(minInterval + 0.15f, maxInterval);
        
        currentSpawnTimer = Random.Range(minInterval, maxInterval);
    }
    
    void IncreaseDifficulty()
    {
        Debug.Log("Difficulty increased! Spawn rate faster, enemies faster.");
    }
    
    public void ResetLevel()
    {
        difficultyTimer = 0f;
        levelEnded = false;
        DifficultyScale = 0f;
        currentSpawnTimer = initialSpawnDelay;
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
