using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float transitionDelay = 0.5f;

    public void LoadScene(string sceneName)
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        // Start coroutine for scene loading with delay
        StartCoroutine(LoadSceneWithDelay(sceneName));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Wait for the button sound to play
        yield return new WaitForSeconds(transitionDelay);

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // After scene is loaded, make sure music continues
        if (AudioManager.Instance != null)
        {
            // Optional: You can stop and restart music if you want a different track per scene
            // AudioManager.Instance.StopBackgroundMusic();
            AudioManager.Instance.ResetAudioState();
            AudioManager.Instance.PlayBackgroundMusic();
        }
    }

    // Additional helper methods for specific scene transitions
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    public void LoadGameScene()
    {
        LoadScene("Game");
    }

    public void QuitGame()
    {
        // Play button sound before quitting
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        // Delay quitting to allow the sound to play
        StartCoroutine(QuitWithDelay());
    }

    private IEnumerator QuitWithDelay()
    {
        yield return new WaitForSeconds(transitionDelay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}