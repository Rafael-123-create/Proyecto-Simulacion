using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [Header("Main Elements")]
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    [Header("Buttons")]
    public Button restartButton;
    public Button menuButton;

    [Header("Visual Effects")]
    public Image panelImage;
    public Image titleGlow;
    public float fadeInDuration = 0.8f;
    public float scaleDuration = 0.5f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    private bool isShowing = false;
    private float pulseTime = 0f;
    private bool canReturnToMenu = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (menuButton != null)
        {
            menuButton.interactable = false;
            menuButton.onClick.AddListener(OnMenu);
        }
    }

    void Update()
    {
        if (isShowing && titleGlow != null)
        {
            pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTime) * pulseAmount;
            titleGlow.rectTransform.localScale = Vector3.one * pulse;
        }

        if (isShowing && !canReturnToMenu)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.rKey.isPressed)
            {
                canReturnToMenu = true;
                if (menuButton != null)
                {
                    menuButton.interactable = true;
                }
            }
        }
    }

    public void ShowGameOver(int player1Score, int player2Score, bool versusMode)
    {
        if (panel == null) return;

        panel.SetActive(true);
        isShowing = true;
        canReturnToMenu = false;
        pulseTime = 0f;

        // Reset elements for animation
        if (panelImage != null)
        {
            Color c = panelImage.color;
            c.a = 0f;
            panelImage.color = c;
        }

        if (titleText != null)
        {
            titleText.rectTransform.localScale = Vector3.zero;
            titleText.text = "GAME OVER";
        }

        if (subtitleText != null)
        {
            subtitleText.rectTransform.localScale = Vector3.zero;
            if (versusMode)
            {
                if (player1Score > player2Score)
                    subtitleText.text = "PLAYER 1 WINS!";
                else if (player2Score > player1Score)
                    subtitleText.text = "PLAYER 2 WINS!";
                else
                    subtitleText.text = "IT'S A TIE!";
            }
            else
            {
                subtitleText.text = "FINAL SCORE";
            }
        }

        if (scoreText != null)
        {
            scoreText.rectTransform.localScale = Vector3.zero;
            if (versusMode)
                scoreText.text = "P1: " + player1Score + "  |  P2: " + player2Score;
            else
                scoreText.text = "SCORE: " + player1Score;
        }

        if (highScoreText != null)
        {
            highScoreText.rectTransform.localScale = Vector3.zero;
            int finalScore = versusMode ? Mathf.Max(player1Score, player2Score) : player1Score;
            int highScore = ScoreManager.GetMinHighScore();
            if (finalScore >= highScore && highScore > 0)
                highScoreText.text = "NEW HIGH SCORE!";
            else if (highScore > 0)
                highScoreText.text = "HIGH SCORE: " + highScore;
            else
                highScoreText.text = "";
        }

        // Start animations
        StartCoroutine(AnimateIn());
    }

    IEnumerator AnimateIn()
    {
        // Fade in panel
        if (panelImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                Color c = panelImage.color;
                c.a = alpha * 0.85f;
                panelImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Scale in title
        if (titleText != null)
            StartCoroutine(ScaleIn(titleText.rectTransform, 0.4f));

        yield return new WaitForSeconds(0.15f);

        // Scale in subtitle
        if (subtitleText != null)
            StartCoroutine(ScaleIn(subtitleText.rectTransform, 0.35f));

        yield return new WaitForSeconds(0.15f);

        // Scale in score
        if (scoreText != null)
            StartCoroutine(ScaleIn(scoreText.rectTransform, 0.3f));

        yield return new WaitForSeconds(0.15f);

        // Scale in high score
        if (highScoreText != null && !string.IsNullOrEmpty(highScoreText.text))
            StartCoroutine(ScaleIn(highScoreText.rectTransform, 0.3f));

        yield return new WaitForSeconds(0.2f);

        // Scale in buttons
        if (restartButton != null)
            StartCoroutine(ScaleIn(restartButton.GetComponent<RectTransform>(), 0.3f));

        yield return new WaitForSeconds(0.1f);

        if (menuButton != null)
            StartCoroutine(ScaleIn(menuButton.GetComponent<RectTransform>(), 0.3f));
    }

    IEnumerator ScaleIn(RectTransform rect, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Elastic ease out
            t = Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI) / 3) + 1;
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        rect.localScale = endScale;
    }

    void OnRestart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel();
        }
        Hide();
    }

    void OnMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void Hide()
    {
        isShowing = false;
        if (panel != null) panel.SetActive(false);
    }
}
