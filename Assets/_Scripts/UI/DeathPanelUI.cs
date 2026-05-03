using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathPanelUI : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;
        G.IsPaused = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        G.IsPaused = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}