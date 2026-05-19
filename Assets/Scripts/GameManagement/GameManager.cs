using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int totalLevels = 3;
    public float levelWaitTime = 3f;
    public float gameOverWaitTime = 3f;

    [Header("References")]
    public WaveSpawner waveSpawner;
    public UIManager uiManager;
    public VersusModeManager versusModeManager;
    public Transform player1Prefab;
    public Transform player2Prefab;
    public Transform[] spawnPoints;

    [Header("Player Settings")]
    public int startingLives = 3;
    public bool spawnPlayer2InScene = false; // If true, spawns player 2 even in single player
    public bool useExistingPlayersInScene = true; // If true, uses players already in scene instead of spawning new ones

    // Game state
    private int currentLevel = 0;
    private int[] playerScores = new int[2];
    private int[] playerLives = new int[2];
    private bool isGameOver = false;
    private bool isVersusMode = false;
    private bool isLevelComplete = false;

    // Events
    public delegate void ScoreChanged(int playerNumber, int newScore);
    public event ScoreChanged OnScoreChanged;

    public delegate void LivesChanged(int playerNumber, int newLives);
    public event LivesChanged OnLivesChanged;

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
        // Read mode from PlayerPrefs (set by menu)
        isVersusMode = PlayerPrefs.GetInt("VersusMode", 0) == 1;
        Debug.Log("GameManager: Starting in " + (isVersusMode ? "VERSUS" : "SINGLE PLAYER") + " mode | PlayerPrefs VersusMode=" + PlayerPrefs.GetInt("VersusMode", -1));
        
        if (isVersusMode && versusModeManager != null)
        {
            versusModeManager.EnableVersusMode();
        }
        else if (!isVersusMode && versusModeManager != null)
        {
            versusModeManager.DisableVersusMode();
        }
        
        InitializeGame();
    }

    void InitializeGame()
    {
        currentLevel = 0;
        playerScores[0] = 0;
        playerScores[1] = 0;
        playerLives[0] = startingLives;
        playerLives[1] = startingLives;
        isGameOver = false;
        isLevelComplete = false;

        if (uiManager != null)
        {
            uiManager.UpdateScore(1, playerScores[0]);
            uiManager.UpdateScore(2, playerScores[1]);
            uiManager.UpdateLives(1, playerLives[0]);
            uiManager.UpdateLives(2, playerLives[1]);
            uiManager.SetLevelText(currentLevel + 1);
        }

        StartLevel();
    }

    public void StartLevel()
    {
        isLevelComplete = false;
        
        // Clear existing enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // Clear existing bullets
        Bullet[] bullets = FindObjectsByType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        // Spawn players
        SpawnPlayers();

        // Enable wave spawner
        if (waveSpawner != null)
        {
            waveSpawner.enabled = true;
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.SetLevelText(currentLevel + 1);
            uiManager.ShowLevelStartUI();
        }

        StartCoroutine(LevelStartWait());
    }

    IEnumerator LevelStartWait()
    {
        if (uiManager != null)
        {
            uiManager.ShowGetReadyUI();
        }
        yield return new WaitForSeconds(2f);
        if (uiManager != null)
        {
            uiManager.HideGetReadyUI();
        }
    }

    void SpawnPlayers()
    {
        if (useExistingPlayersInScene)
        {
            PlayerController[] existingPlayers = FindObjectsByType<PlayerController>();
            int playerCount = existingPlayers.Length;
            
            Debug.Log("GameManager: Found " + playerCount + " existing player(s), versus=" + isVersusMode);
            for (int i = 0; i < playerCount; i++)
            {
                Debug.Log("GameManager: Player " + i + " - name: " + existingPlayers[i].gameObject.name + 
                         ", pos: " + existingPlayers[i].transform.position + 
                         ", active: " + existingPlayers[i].gameObject.activeSelf);
            }
            
            if (isVersusMode)
            {
                // In versus mode, we need 2 players
                if (playerCount >= 2)
                {
                    // Assign Player 1 to the one with lower x position
                    if (existingPlayers[0].transform.position.x < existingPlayers[1].transform.position.x)
                    {
                        existingPlayers[0].playerNumber = 1;
                        existingPlayers[0].gameObject.name = "Player1";
                        existingPlayers[0].gameObject.tag = "Player";
                        existingPlayers[0].gameObject.SetActive(true);
                        
                        existingPlayers[1].playerNumber = 2;
                        existingPlayers[1].gameObject.name = "Player2";
                        existingPlayers[1].gameObject.tag = "Player";
                        existingPlayers[1].gameObject.SetActive(true);
                    }
                    else
                    {
                        existingPlayers[1].playerNumber = 1;
                        existingPlayers[1].gameObject.name = "Player1";
                        existingPlayers[1].gameObject.tag = "Player";
                        existingPlayers[1].gameObject.SetActive(true);
                        
                        existingPlayers[0].playerNumber = 2;
                        existingPlayers[0].gameObject.name = "Player2";
                        existingPlayers[0].gameObject.tag = "Player";
                        existingPlayers[0].gameObject.SetActive(true);
                    }
                    
                    // Set explicit positions
                    existingPlayers[0].transform.position = new Vector3(-2f, -3f, 0f);
                    existingPlayers[1].transform.position = new Vector3(2f, -3f, 0f);
                    
                    Debug.Log("GameManager: Versus mode - Player1 at " + existingPlayers[0].transform.position + 
                             ", Player2 at " + existingPlayers[1].transform.position);
                }
                else if (playerCount == 1 && player2Prefab != null)
                {
                    existingPlayers[0].playerNumber = 1;
                    existingPlayers[0].gameObject.name = "Player1";
                    existingPlayers[0].gameObject.tag = "Player";
                    existingPlayers[0].gameObject.SetActive(true);
                    existingPlayers[0].transform.position = new Vector3(-2f, -3f, 0f);
                    
                    Vector3 player2Pos = new Vector3(2f, -3f, 0f);
                    GameObject player2Obj = Instantiate(player2Prefab, player2Pos, Quaternion.identity).gameObject;
                    player2Obj.name = "Player2";
                    player2Obj.tag = "Player";
                    PlayerController pc2 = player2Obj.GetComponent<PlayerController>();
                    if (pc2 != null) pc2.playerNumber = 2;
                    Debug.Log("GameManager: Versus mode - Spawned Player2 from prefab");
                }
                else if (player2Prefab != null)
                {
                    Vector3 player1Pos = new Vector3(-2f, -3f, 0f);
                    GameObject player1Obj = Instantiate(player1Prefab, player1Pos, Quaternion.identity).gameObject;
                    player1Obj.name = "Player1";
                    player1Obj.tag = "Player";
                    PlayerController pc1 = player1Obj.GetComponent<PlayerController>();
                    if (pc1 != null) pc1.playerNumber = 1;
                    
                    Vector3 player2Pos = new Vector3(2f, -3f, 0f);
                    GameObject player2Obj = Instantiate(player2Prefab, player2Pos, Quaternion.identity).gameObject;
                    player2Obj.name = "Player2";
                    player2Obj.tag = "Player";
                    PlayerController pc2 = player2Obj.GetComponent<PlayerController>();
                    if (pc2 != null) pc2.playerNumber = 2;
                    Debug.Log("GameManager: Versus mode - Spawned both players from prefabs");
                }
            }
            else
            {
                // Single player mode: only enable Player 1, disable Player 2
                if (playerCount >= 1)
                {
                    existingPlayers[0].playerNumber = 1;
                    existingPlayers[0].gameObject.name = "Player1";
                    existingPlayers[0].gameObject.tag = "Player";
                    existingPlayers[0].gameObject.SetActive(true);
                    existingPlayers[0].transform.position = new Vector3(0f, -3f, 0f);
                }
                
                // Disable any additional players
                for (int i = 1; i < playerCount; i++)
                {
                    Debug.Log("GameManager: Disabling extra player " + i);
                    existingPlayers[i].gameObject.SetActive(false);
                }
            }
            
            return;
        }

        // Spawn from prefabs
        PlayerController[] playersToDestroy = FindObjectsByType<PlayerController>();
        foreach (PlayerController player in playersToDestroy)
        {
            Destroy(player.gameObject);
        }

        if (player1Prefab != null)
        {
            Vector3 player1Pos = new Vector3(-2f, -3f, 0f);
            GameObject player1Obj = Instantiate(player1Prefab, player1Pos, Quaternion.identity).gameObject;
            player1Obj.name = "Player1";
            PlayerController pc1 = player1Obj.GetComponent<PlayerController>();
            if (pc1 != null) pc1.playerNumber = 1;
        }

        if (player2Prefab != null && isVersusMode)
        {
            Vector3 player2Pos = new Vector3(2f, -3f, 0f);
            GameObject player2Obj = Instantiate(player2Prefab, player2Pos, Quaternion.identity).gameObject;
            player2Obj.name = "Player2";
            PlayerController pc2 = player2Obj.GetComponent<PlayerController>();
            if (pc2 != null) pc2.playerNumber = 2;
        }
    }

    public void AddScore(int playerNumber, int points)
    {
        if (isGameOver) return;

        int index = playerNumber - 1;
        if (index < 0 || index >= 2) return;

        playerScores[index] += points;
        if (OnScoreChanged != null)
        {
            OnScoreChanged(playerNumber, playerScores[index]);
        }

        if (uiManager != null)
        {
            uiManager.UpdateScore(playerNumber, playerScores[index]);
        }
    }

    public void TakeLife(int playerNumber)
    {
        if (isGameOver) return;

        int index = playerNumber - 1;
        if (index < 0 || index >= 2) return;

        playerLives[index]--;
        Debug.Log("GameManager: Player " + playerNumber + " lost a life. Remaining: " + playerLives[index] + " | isVersusMode: " + isVersusMode);
        
        if (OnLivesChanged != null)
        {
            OnLivesChanged(playerNumber, playerLives[index]);
        }

        if (uiManager != null)
        {
            uiManager.UpdateLives(playerNumber, playerLives[index]);
        }

        if (isVersusMode)
        {
            Debug.Log("GameManager: Versus mode check - P1 lives: " + playerLives[0] + ", P2 lives: " + playerLives[1]);
            
            // If a player reaches 0 lives, disable them but keep the game going
            if (playerLives[index] <= 0)
            {
                Debug.Log("GameManager: Player " + playerNumber + " is out! Disabling player...");
                DisablePlayer(playerNumber);
            }
            
            // Game only ends when BOTH players are out
            if (playerLives[0] <= 0 && playerLives[1] <= 0)
            {
                Debug.Log("GameManager: Both players out of lives - ending game");
                EndGame();
            }
        }
        else
        {
            Debug.Log("GameManager: Single player check - P1 lives: " + playerLives[0]);
            if (playerLives[0] <= 0)
            {
                Debug.Log("GameManager: Player 1 out of lives - ending game");
                EndGame();
            }
        }
    }
    
    void DisablePlayer(int playerNumber)
    {
        PlayerController[] players = FindObjectsByType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player.playerNumber == playerNumber)
            {
                player.enabled = false;
                player.gameObject.SetActive(false);
                Debug.Log("GameManager: Disabled Player " + playerNumber + " (name: " + player.gameObject.name + ")");
            }
        }
    }

    public void EnemyDestroyed(EnemyType type, int destroyedByPlayer = 1)
    {
        if (isGameOver) return;
        
        int score = GetScoreForEnemy(type);
        AddScore(destroyedByPlayer, score);
        Debug.Log("GameManager: Enemy " + type + " destroyed by Player " + destroyedByPlayer + " for " + score + " points");
    }
    
    int GetScoreForEnemy(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Shooter: return 10;
            case EnemyType.Kamikaze: return 20;
            default: return 10;
        }
    }

    public void WaveComplete() { }

    public void LevelComplete()
    {
        if (isLevelComplete || isGameOver) return;

        isLevelComplete = true;

        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
        }

        if (uiManager != null)
        {
            uiManager.ShowLevelCompleteUI(playerScores[0], playerScores[1]);
        }

        StartCoroutine(LevelCompleteWait());
    }

    IEnumerator LevelCompleteWait()
    {
        yield return new WaitForSeconds(levelWaitTime);

        currentLevel++;
        if (currentLevel >= totalLevels)
        {
            EndGame();
        }
        else
        {
            StartLevel();
        }
    }

    void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GameManager: Game Over triggered!");

        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
        }

        // Destroy all existing enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // Destroy all existing bullets
        Bullet[] bullets = FindObjectsByType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        // Disable all player controllers to stop input
        PlayerController[] players = FindObjectsByType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.enabled = false;
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOverUI(playerScores[0], playerScores[1], isVersusMode);
        }

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(gameOverWaitTime);
        Debug.Log("GameManager: Returning to menu...");
        
        // Save high score before returning to menu
        SaveHighScore();
        
        SceneManager.LoadScene("Menu");
    }
    
    void SaveHighScore()
    {
        string playerName = ScoreManager.GetCurrentPlayerName();
        
        if (isVersusMode)
        {
            // In versus mode, only save the winner's score
            if (playerScores[0] > playerScores[1])
            {
                // Player 1 wins
                SaveScoreForPlayer(playerName + "P1", playerScores[0], "VS");
                Debug.Log("ScoreManager: Versus mode - Player 1 wins with " + playerScores[0] + " points");
            }
            else if (playerScores[1] > playerScores[0])
            {
                // Player 2 wins
                SaveScoreForPlayer(playerName + "P2", playerScores[1], "VS");
                Debug.Log("ScoreManager: Versus mode - Player 2 wins with " + playerScores[1] + " points");
            }
            else
            {
                // Tie - save both or just player 1
                SaveScoreForPlayer(playerName + "P1", playerScores[0], "VS");
                Debug.Log("ScoreManager: Versus mode - Tie! Saving Player 1 score: " + playerScores[0]);
            }
        }
        else
        {
            // Single player mode
            SaveScoreForPlayer(playerName, playerScores[0], "SP");
        }
    }
    
    void SaveScoreForPlayer(string name, int score, string mode)
    {
        if (score > 0 && ScoreManager.IsHighScore(score))
        {
            ScoreManager.SaveHighScore(name, score, mode);
            Debug.Log("ScoreManager: New high score saved! " + name + ": " + score + " (" + mode + ")");
        }
        else
        {
            Debug.Log("ScoreManager: Score " + score + " not high enough for " + mode + " mode (min: " + ScoreManager.GetMinHighScore() + ")");
        }
    }

    public void SetVersusMode(bool versus)
    {
        isVersusMode = versus;
        if (versusModeManager != null)
        {
            if (versus)
            {
                versusModeManager.EnableVersusMode();
            }
            else
            {
                versusModeManager.DisableVersusMode();
            }
        }
    }

    public bool IsVersusMode() { return isVersusMode; }
    public bool IsGameOver() { return isGameOver; }
    public int GetPlayerScore(int playerNumber) { return playerScores[playerNumber - 1]; }
    public int GetPlayerLives(int playerNumber) { return playerLives[playerNumber - 1]; }
}
