using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectPanel;
    public GameObject controlsPanel;
    public GameObject nameInputPanel;
    public GameObject highScoresPanel;

    [Header("Name Input")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameErrorText;
    
    [Header("High Scores")]
    public Transform highScoresContent;
    public GameObject highScoreEntryPrefab;
    public TextMeshProUGUI minScoreText;

    [Header("Game Scene")]
    public string gameSceneName = "SampleScene";
    
    private bool isVersusMode = false;

    void Start()
    {
        ShowMainMenu();
        AudioHelper.PlayMenuMusic();
    }

    // --- Botones del Menu Principal ---

    public void OnPlayButton()
    {
        ShowModeSelect();
    }

    public void OnControlsButton()
    {
        ShowControls();
    }

    public void OnHighScoresButton()
    {
        ShowHighScores();
    }

    public void OnQuitButton()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // --- Seleccion de Modo ---

    public void OnSinglePlayerButton()
    {
        isVersusMode = false;
        ShowNameInput();
    }

    public void OnVersusButton()
    {
        isVersusMode = true;
        ShowNameInput();
    }
    
    // --- Name Input ---
    
    public void OnSubmitName()
    {
        string name = nameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(name))
        {
            nameErrorText.text = "Ingresa un nombre";
            return;
        }
        
        if (name.Length > 5)
        {
            nameErrorText.text = "Maximo 5 caracteres";
            return;
        }
        
        nameErrorText.text = "";
        ScoreManager.SetCurrentPlayerName(name);
        
        if (isVersusMode)
        {
            PlayerPrefs.SetInt("VersusMode", 1);
        }
        else
        {
            PlayerPrefs.SetInt("VersusMode", 0);
        }
        PlayerPrefs.Save();
        
        LoadGameScene();
    }

    // --- Navegacion entre paneles ---

    void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(nameInputPanel, false);
        SetPanelActive(highScoresPanel, false);
    }

    void ShowModeSelect()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, true);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(nameInputPanel, false);
        SetPanelActive(highScoresPanel, false);
    }

    void ShowControls()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, true);
        SetPanelActive(nameInputPanel, false);
        SetPanelActive(highScoresPanel, false);
    }
    
    void ShowNameInput()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(nameInputPanel, true);
        SetPanelActive(highScoresPanel, false);
        
        nameInputField.text = ScoreManager.GetCurrentPlayerName();
        nameErrorText.text = "";
        nameInputField.Select();
    }
    
    void ShowHighScores()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(nameInputPanel, false);
        SetPanelActive(highScoresPanel, true);
        
        DisplayHighScores();
    }
    
    void DisplayHighScores()
    {
        // Clear existing entries
        foreach (Transform child in highScoresContent)
        {
            Destroy(child.gameObject);
        }
        
        var scores = ScoreManager.GetHighScores();
        float entryHeight = 40f;
        float spacing = 5f;
        
        for (int i = 0; i < scores.Count; i++)
        {
            if (highScoreEntryPrefab != null)
            {
                GameObject entry = Instantiate(highScoreEntryPrefab, highScoresContent);
                RectTransform entryRect = entry.GetComponent<RectTransform>();
                
                // Manually position entries in a vertical stack from top
                entryRect.anchorMin = new Vector2(0, 1);
                entryRect.anchorMax = new Vector2(1, 1);
                entryRect.pivot = new Vector2(0.5f, 1);
                entryRect.anchoredPosition = new Vector2(0, -i * (entryHeight + spacing));
                entryRect.sizeDelta = new Vector2(0, entryHeight);
                
                TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI scoreText = entry.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
                
                if (rankText != null) rankText.text = (i + 1).ToString();
                if (nameText != null) nameText.text = scores[i].playerName;
                if (scoreText != null) scoreText.text = scores[i].score.ToString() + " (" + scores[i].gameMode + ")";
            }
        }
        
        // Update Content height to fit all entries
        RectTransform contentRect = highScoresContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = scores.Count * (entryHeight + spacing);
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, Mathf.Max(totalHeight, 1));
        }
        
        if (minScoreText != null)
        {
            int minScore = ScoreManager.GetMinHighScore();
            minScoreText.text = minScore > 0 ? "Minimo para entrar: " + minScore : "Juega para entrar al ranking!";
        }
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // --- Boton volver ---

    public void OnBackButton()
    {
        ShowMainMenu();
    }
}
