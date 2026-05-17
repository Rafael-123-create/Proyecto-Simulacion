using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text player1LivesText;
    public Text player2LivesText;
    public Text levelText;
    public Text getReadyText;
    public Text levelCompleteText;
    public Text gameOverText;
    public Text winnerText;

    [Header("Panels")]
    public GameObject getReadyPanel;
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;

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
        // Hide all panels initially
        HideAllPanels();
    }

    public void HideAllPanels()
    {
        if (getReadyPanel != null) getReadyPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void UpdateScore(int playerNumber, int score)
    {
        if (playerNumber == 1 && player1ScoreText != null)
        {
            player1ScoreText.text = "P1: " + score;
        }
        else if (playerNumber == 2 && player2ScoreText != null)
        {
            player2ScoreText.text = "P2: " + score;
        }
    }

    public void UpdateLives(int playerNumber, int lives)
    {
        if (playerNumber == 1 && player1LivesText != null)
        {
            player1LivesText.text = "Lives: " + lives;
        }
        else if (playerNumber == 2 && player2LivesText != null)
        {
            player2LivesText.text = "Lives: " + lives;
        }
    }

    public void SetLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }

    public void ShowLevelStartUI()
    {
        if (getReadyPanel != null)
        {
            getReadyPanel.SetActive(true);
            if (getReadyText != null)
            {
                getReadyText.text = "LEVEL START";
            }
        }
    }

    public void ShowGetReadyUI()
    {
        if (getReadyPanel != null)
        {
            getReadyPanel.SetActive(true);
            if (getReadyText != null)
            {
                getReadyText.text = "GET READY";
            }
        }
    }

    public void HideGetReadyUI()
    {
        if (getReadyPanel != null)
        {
            getReadyPanel.SetActive(false);
        }
    }

    public void ShowLevelCompleteUI(int player1Score, int player2Score)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            if (levelCompleteText != null)
            {
                levelCompleteText.text = "LEVEL COMPLETE\nP1: " + player1Score + "  P2: " + player2Score;
            }
        }
    }

    public void HideLevelCompleteUI()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void ShowGameOverUI(int player1Score, int player2Score, bool versusMode)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
            }
            if (winnerText != null)
            {
                if (versusMode)
                {
                    if (player1Score > player2Score)
                    {
                        winnerText.text = "PLAYER 1 WINS!\nP1: " + player1Score + "  P2: " + player2Score;
                    }
                    else if (player2Score > player1Score)
                    {
                        winnerText.text = "PLAYER 2 WINS!\nP1: " + player1Score + "  P2: " + player2Score;
                    }
                    else
                    {
                        winnerText.text = "IT'S A TIE!\nP1: " + player1Score + "  P2: " + player2Score;
                    }
                }
                else
                {
                    winnerText.text = "YOUR SCORE: " + player1Score;
                }
            }
        }
    }
}
