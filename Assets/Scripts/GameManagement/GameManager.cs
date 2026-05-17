using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int totalLevels = 3;
    public float levelWaitTime = 3f; // Time to wait between levels
    public float gameOverWaitTime = 3f; // Time to wait before showing game over screen

    [Header("References")]
    public WaveSpawner waveSpawner;
    public UIManager uiManager;
    public VersusModeManager versusModeManager;
    public Transform player1Prefab;
    public Transform player2Prefab;
    public Transform[] spawnPoints; // For enemies

    [Header("Player Settings")]
    public int startingLives = 3;

    // Game state
    private int currentLevel = 0;
    private int[] playerScores = new int[2]; // Index 0 for player 1, 1 for player 2
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
        // Singleton pattern
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
        // Initialize game
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

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(1, playerScores[0]);
            uiManager.UpdateScore(2, playerScores[1]);
            uiManager.UpdateLives(1, playerLives[0]);
            uiManager.UpdateLives(2, playerLives[1]);
            uiManager.SetLevelText(currentLevel + 1);
        }

        // Start first level
        StartLevel();
    }

    public void StartLevel()
    {
        isLevelComplete = false;
        // Clear any existing enemies? We'll let the wave spawner handle spawning.
        // We can destroy all enemies if needed.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // Clear bullets?
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
        bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
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

        // Start level wait coroutine
        StartCoroutine(LevelStartWait());
    }

    IEnumerator LevelStartWait()
    {
        // Show "GET READY" or similar
        if (uiManager != null)
        {
            uiManager.ShowGetReadyUI();
        }
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        if (uiManager != null)
        {
            uiManager.HideGetReadyUI();
        }
        // Then let the wave spawner start spawning
    }

    void SpawnPlayers()
    {
        // Destroy existing players if any
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in existingPlayers)
        {
            Destroy(player);
        }

        // Spawn player 1 (left side)
        if (player1Prefab != null)
        {
            Vector3 player1Pos = new Vector3(-4f, 0f, 0f); // Adjust as needed
            Instantiate(player1Prefab, player1Pos, Quaternion.identity);
        }

        // Spawn player 2 (right side)
        if (player2Prefab != null && isVersusMode)
        {
            Vector3 player2Pos = new Vector3(4f, 0f, 0f); // Adjust as needed
            Instantiate(player2Prefab, player2Pos, Quaternion.identity);
        }
        else if (!isVersusMode && player2Prefab != null)
        {
            // In single player mode, we might not spawn player 2, or we can spawn it as an AI?
            // For now, we only spawn player 2 in versus mode.
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
        if (OnLivesChanged != null)
        {
            OnLivesChanged(playerNumber, playerLives[index]);
        }

        if (uiManager != null)
        {
            uiManager.UpdateLives(playerNumber, playerLives[index]);
        }

        // Check if player is out of lives
        if (playerLives[index] <= 0)
        {
            // Player is dead
            // We can handle player death here (e.g., disable player object)
            // For now, we just note it.
        }

        // Check if game is over (both players out of lives in versus mode, or player 1 out in single player)
        if (isVersusMode)
        {
            if (playerLives[0] <= 0 && playerLives[1] <= 0)
            {
                EndGame();
            }
        }
        else
        {
            if (playerLives[0] <= 0)
            {
                EndGame();
            }
        }
    }

    public void EnemyDestroyed(EnemyType type)
    {
        // Optional: give score based on type? We'll let the enemy controller handle scoring via AddScore.
        // But we can also do it here if we want centralized scoring.
        // For now, we'll let the enemy controller call AddScore when it dies.
    }

    public void WaveComplete()
    {
        // Check if all waves are done for the level? We'll use a simple wave count.
        // Alternatively, we can have the wave spawner tell us when a wave is complete and we track the wave number.
        // For simplicity, we'll say that after a certain number of waves, the level is complete.
        // We'll implement this in the WaveSpawner and have it call LevelComplete() on the GameManager.
    }

    public void LevelComplete()
    {
        if (isLevelComplete || isGameOver) return;

        isLevelComplete = true;

        // Disable wave spawner
        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
        }

        // Clear remaining enemies and bullets? Optional.

        // Show level complete UI
        if (uiManager != null)
        {
            uiManager.ShowLevelCompleteUI(playerScores[0], playerScores[1]);
        }

        // Wait then go to next level or game complete
        StartCoroutine(LevelCompleteWait());
    }

    IEnumerator LevelCompleteWait()
    {
        yield return new WaitForSeconds(levelWaitTime);

        currentLevel++;
        if (currentLevel >= totalLevels)
        {
            // Game complete
            EndGame();
        }
        else
        {
            // Start next level
            StartLevel();
        }
    }

    void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Disable wave spawner
        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
        }

        // Show game over UI
        if (uiManager != null)
        {
            uiManager.ShowGameOverUI(playerScores[0], playerScores[1], isVersusMode);
        }

        // Optionally, return to main menu after a delay
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(gameOverWaitTime);
        // Load main menu scene
        // SceneManager.LoadScene("MainMenu");
        // For now, we just log.
        Debug.Log("Game Over. Returning to menu.");
    }

    // Versus mode control
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

    // Getters
    public bool IsVersusMode() { return isVersusMode; }
    public bool IsGameOver() { return isGameOver; }
    public int GetPlayerScore(int playerNumber) { return playerScores[playerNumber - 1]; }
    public int GetPlayerLives(int playerNumber) { return playerLives[playerNumber - 1]; }
}
