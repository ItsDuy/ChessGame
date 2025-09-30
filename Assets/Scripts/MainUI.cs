using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
public class MainUI : MonoBehaviour
{
    // Start is called before the first frame update
    public SceneLoader SceneLoader;
    public Canvas mainCanvas;
    public Canvas gameModeCanvas;
    public Canvas customizeCanvas;
    public GameObject audioManagerPrefab; // Reference to the AudioManager prefab

    private void Start()
    {
        // Check if AudioManager instance exists, if not, instantiate it
        if (AudioManager.Instance == null && audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
        }

        // Make sure background music is playing
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }
    }

    public void OnClickStart()
    {
        mainCanvas.gameObject.SetActive(false);
        gameModeCanvas.gameObject.SetActive(true);
    }

    public void OnClickCustomize()
    {
        mainCanvas.gameObject.SetActive(false);
        customizeCanvas.gameObject.SetActive(true);
    }

    public void OnClickExit()
    {
        // Use SceneLoader to properly handle quitting the game
        if (SceneLoader != null)
        {
            SceneLoader.QuitGame();
        }
        else
        {
            // Fallback if SceneLoader is not assigned
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
