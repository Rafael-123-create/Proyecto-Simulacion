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
    public Transform[] versusSpawnPoints; // Spawn points específicos para modo versus
    public float initialSpawnDelay = 1f;
    public float difficultyIncreaseInterval = 30f;
    
    [Header("Level Settings")]
    public int[] scoreThresholds = { 500, 1200, 2500 };
    
    [Header("Difficulty Settings")]
    public float speedIncreasePerInterval = 0.2f;
    public float spawnRateIncreasePerInterval = 0.25f;
    public int maxConcurrentEnemies = 25;
    
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
            baseMinInterval = 0.4f;
            baseMaxInterval = 1.2f;
            Debug.LogWarning("WaveSpawner: No enemy types configured!");
        }
        
        currentSpawnTimer = initialSpawnDelay;
        difficultyTimer = 0f;
        levelEnded = false;
    }
    
    public void RegenerateSpawnPoints()
    {
        bool isVersus = GameManager.Instance != null && GameManager.Instance.IsVersusMode();
        
        // Destroy old spawn containers
        GameObject oldContainer = GameObject.Find("VersusSpawnPoints");
        if (oldContainer != null) DestroyImmediate(oldContainer);
        oldContainer = GameObject.Find("SinglePlayerSpawnPoints");
        if (oldContainer != null) DestroyImmediate(oldContainer);
        
        if (isVersus)
        {
            GenerateVersusSpawnPoints();
        }
        else
        {
            GenerateSinglePlayerSpawnPoints();
        }
        
        Debug.Log("WaveSpawner: Spawn points regenerated. Total: " + (isVersus ? versusSpawnPoints?.Length : spawnPoints?.Length));
    }
    
    void GenerateSinglePlayerSpawnPoints()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = FindCameraByName("Player1Camera");
        }
        
        if (mainCam == null)
        {
            Debug.LogWarning("WaveSpawner: Cannot generate single player spawn points - camera not found");
            return;
        }
        
        float camHeight = 2f * mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        int laneCount = 5;
        float laneWidth = camWidth / laneCount;
        float screenLeft = -camWidth / 2f;
        
        float spawnY = mainCam.transform.position.y + mainCam.orthographicSize + 1f;
        
        List<Transform> newSpawnPoints = new List<Transform>();
        GameObject spawnContainer = new GameObject("SinglePlayerSpawnPoints");
        
        for (int lane = 0; lane < laneCount; lane++)
        {
            float laneCenterX = screenLeft + (lane + 0.5f) * laneWidth;
            GameObject point = new GameObject("SP_Spawn_L" + lane);
            point.transform.SetParent(spawnContainer.transform);
            point.transform.position = new Vector3(laneCenterX, spawnY, 0f);
            newSpawnPoints.Add(point.transform);
        }
        
        spawnPoints = newSpawnPoints.ToArray();
        Debug.Log("WaveSpawner: Generated " + spawnPoints.Length + " single player spawn points");
    }
    
    void GenerateVersusSpawnPoints()
    {
        Camera p1Cam = FindCameraByName("Player1Camera");
        Camera p2Cam = FindCameraByName("Player2Camera");
        
        if (p1Cam == null || p2Cam == null)
        {
            Debug.LogWarning("WaveSpawner: Cannot generate versus spawn points - cameras not found");
            return;
        }
        
        float camHeight = 2f * p1Cam.orthographicSize;
        float camWidth = camHeight * p1Cam.aspect;
        float halfWidth = camWidth * 0.5f;
        
        int laneCount = 5;
        float laneWidth = halfWidth / laneCount;
        
        float spawnY = p1Cam.transform.position.y + p1Cam.orthographicSize + 1f;
        
        List<Transform> newSpawnPoints = new List<Transform>();
        GameObject spawnContainer = new GameObject("VersusSpawnPoints");
        
        // Generate P1 spawn points (left side) - same formula as PlayerController.GetLaneCenterX()
        float p1ScreenLeft = -halfWidth;
        for (int lane = 0; lane < laneCount; lane++)
        {
            float laneCenterX = p1ScreenLeft + (lane + 0.5f) * laneWidth;
            GameObject point = new GameObject("P1_Spawn_L" + lane);
            point.transform.SetParent(spawnContainer.transform);
            point.transform.position = new Vector3(laneCenterX, spawnY, 0f);
            newSpawnPoints.Add(point.transform);
        }
        
        // Generate P2 spawn points (right side) - same formula as PlayerController.GetLaneCenterX()
        float p2ScreenLeft = 0f;
        for (int lane = 0; lane < laneCount; lane++)
        {
            float laneCenterX = p2ScreenLeft + (lane + 0.5f) * laneWidth;
            GameObject point = new GameObject("P2_Spawn_L" + lane);
            point.transform.SetParent(spawnContainer.transform);
            point.transform.position = new Vector3(laneCenterX, spawnY, 0f);
            newSpawnPoints.Add(point.transform);
        }
        
        versusSpawnPoints = newSpawnPoints.ToArray();
        Debug.Log("WaveSpawner: Generated " + versusSpawnPoints.Length + " versus spawn points");
    }
    
    Camera FindCameraByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Camera cam = obj.GetComponent<Camera>();
            if (cam != null) return cam;
            foreach (Transform child in obj.transform)
            {
                cam = child.GetComponent<Camera>();
                if (cam != null) return cam;
            }
        }
        return null;
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
        
        // Determine how many enemies to spawn based on difficulty
        int spawnCount = 1;
        if (DifficultyScale > 0.6f && Random.value < 0.4f)
        {
            spawnCount = Random.Range(2, 4); // 2-3 enemies at high difficulty
        }
        else if (DifficultyScale > 0.3f && Random.value < 0.2f)
        {
            spawnCount = 2; // 2 enemies at medium difficulty
        }
        
        // Check if we can spawn this many enemies
        int currentEnemyCount = FindObjectsByType<EnemyController>().Length;
        int availableSlots = maxConcurrentEnemies - currentEnemyCount;
        spawnCount = Mathf.Min(spawnCount, availableSlots);
        
        if (spawnCount <= 0) return;
        
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleEnemy();
        }
        
        if (spawnCount > 1)
        {
            Debug.Log("WaveSpawner: Spawned " + spawnCount + " enemies at once (DifficultyScale: " + DifficultyScale.ToString("F2") + ")");
        }
    }
    
    void SpawnSingleEnemy()
    {
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
            }
            else if (!p1Alive && p2Alive)
            {
                // Only player 2 is alive, spawn on right side
                side = 1;
            }
            else
            {
                // Both alive (or both dead, but game should be ending), spawn randomly
                side = Random.Range(0, 2);
            }
            
            // Use versus-specific spawn points
            if (versusSpawnPoints == null || versusSpawnPoints.Length == 0)
            {
                Debug.LogWarning("WaveSpawner: No versus spawn points configured!");
                return;
            }
            
            Transform[] sideSpawnPoints = GetVersusSideSpawnPoints(side);
            if (sideSpawnPoints.Length == 0)
            {
                Debug.LogWarning("WaveSpawner: No versus spawn points for side " + side);
                return;
            }
            Transform spawnPoint = sideSpawnPoints[Random.Range(0, sideSpawnPoints.Length)];
            GameObject enemyObj = Instantiate(selected.prefab, spawnPoint.position, Quaternion.identity);
            
            int enemyLayer = side == 0 ? VersusModeManager.Player1Layer : VersusModeManager.Player2Layer;
            enemyObj.layer = enemyLayer;
            
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
            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Initialize(selected.type);
            }
        }
    }
    
    Transform[] GetVersusSideSpawnPoints(int side)
    {
        List<Transform> sidePoints = new List<Transform>();
        foreach (Transform sp in versusSpawnPoints)
        {
            if (side == 0 && sp.position.x < 0f)
            {
                sidePoints.Add(sp);
            }
            else if (side == 1 && sp.position.x >= 0f)
            {
                sidePoints.Add(sp);
            }
        }
        return sidePoints.ToArray();
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
        
        float minInterval = baseMinInterval - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        float maxInterval = baseMaxInterval - (difficultyTimer / difficultyIncreaseInterval) * spawnRateIncreasePerInterval;
        
        // In versus mode, spawn enemies 1.25x faster
        bool isVersus = GameManager.Instance != null && GameManager.Instance.IsVersusMode();
        float versusMultiplier = isVersus ? 0.8f : 1f; // 0.8 = 1/1.25 (faster spawns)
        
        minInterval *= versusMultiplier;
        maxInterval *= versusMultiplier;
        
        // Clamp intervals to reasonable values
        minInterval = Mathf.Max(0.12f, minInterval);
        maxInterval = Mathf.Max(minInterval + 0.12f, maxInterval);
        
        currentSpawnTimer = Random.Range(minInterval, maxInterval);
    }
    
    void IncreaseDifficulty()
    {
        Debug.Log("Difficulty increased! Spawn rate faster, enemies faster.");
        
        // Increase speed of all existing enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.speed += speedIncreasePerInterval;
        }
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
