using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Loading")]
    public string gameplaySceneName = "FactCheckScene";

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public SettingsMenuController settingsMenu;

    public void PlayGame()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("MainMenuController gameplaySceneName is empty.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (settingsMenu != null)
        {
            settingsMenu.Open();
        }
    }

    public void CloseSettings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.Close();
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
