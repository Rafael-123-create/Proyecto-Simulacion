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
    [Header("Spawn Settings")]
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    public Transform[] spawnPoints; // Top of screen positions
    public float initialSpawnDelay = 2f;
    public float difficultyIncreaseInterval = 30f; // How often to increase difficulty
    
    [Header("Difficulty Settings")]
    public float speedIncreasePerInterval = 0.2f;
    public float spawnRateIncreasePerInterval = 0.1f; // Reduces interval
    
    private float currentSpawnTimer;
    private float difficultyTimer;
    private float baseMinInterval = 1f;
    private float baseMaxInterval = 3f;
    
    void Start()
    {
        Debug.Log("WaveSpawner: Starting with " + enemyTypes.Count + " enemy types and " + spawnPoints.Length + " spawn points");
        
        // Initialize base intervals from the first enemy type (if any) or use defaults
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
    }
    
    void Update()
    {
        difficultyTimer += Time.deltaTime;
        
        // Increase difficulty over time
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            difficultyTimer = 0f;
            IncreaseDifficulty();
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
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject enemyObj = Instantiate(selected.prefab, spawnPoint.position, Quaternion.identity);
        Debug.Log("WaveSpawner: Spawned " + selected.type + " at " + spawnPoint.position);
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.Initialize(selected.type);
        }
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
        // Increase speed of existing enemies? Or just make new ones faster?
        // We'll increase the base speed for new enemies by adjusting the enemy type settings.
        // For simplicity, we'll just note that the enemy prefabs should have a base speed and we modify it in Initialize.
        // Alternatively, we can adjust the spawn rates and maybe add more enemy types later.
        // For now, we'll just log.
        Debug.Log("Difficulty increased!");
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
