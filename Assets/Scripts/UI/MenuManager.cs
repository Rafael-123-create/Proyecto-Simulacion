using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectPanel;
    public GameObject controlsPanel;
    public GameObject nameInputPanel;
    public GameObject highScoresPanel;

    [Header("Name Input")]
    public InputField nameInputField;
    public Text nameErrorText;
    
    [Header("High Scores")]
    public Transform highScoresContent;
    public GameObject highScoreEntryPrefab;
    public Text minScoreText;

    [Header("Game Scene")]
    public string gameSceneName = "SampleScene";
    
    private bool isVersusMode = false;

    void Start()
    {
        ShowMainMenu();
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
        
        for (int i = 0; i < scores.Count; i++)
        {
            if (highScoreEntryPrefab != null)
            {
                GameObject entry = Instantiate(highScoreEntryPrefab, highScoresContent);
                Text[] texts = entry.GetComponentsInChildren<Text>();
                
                if (texts.Length >= 3)
                {
                    texts[0].text = (i + 1).ToString();
                    texts[1].text = scores[i].playerName;
                    texts[2].text = scores[i].score.ToString() + " (" + scores[i].gameMode + ")";
                }
            }
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
