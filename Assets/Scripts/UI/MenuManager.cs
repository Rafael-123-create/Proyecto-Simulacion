using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectPanel;
    public GameObject controlsPanel;

    [Header("Game Scene")]
    public string gameSceneName = "SampleScene";

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
        // Configurar modo single player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetVersusMode(false);
        }
        PlayerPrefs.SetInt("VersusMode", 0);
        PlayerPrefs.Save();
        LoadGameScene();
    }

    public void OnVersusButton()
    {
        // Configurar modo versus
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetVersusMode(true);
        }
        PlayerPrefs.SetInt("VersusMode", 1);
        PlayerPrefs.Save();
        LoadGameScene();
    }

    // --- Navegacion entre paneles ---

    void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, false);
    }

    void ShowModeSelect()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, true);
        SetPanelActive(controlsPanel, false);
    }

    void ShowControls()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(controlsPanel, true);
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
