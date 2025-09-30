using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayTimeUI : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public GameObject mainUI;
    public GameObject gameModeUI;
    public void onBackToMainUIClick()
    {
        mainUI.SetActive(true);
        gameModeUI.SetActive(false);
    }
    public void onClick1()
    {
        PlayerPrefs.SetInt("PlayTime", 60);
        sceneLoader.LoadScene("Game");
    }
    public void onClick1to1()
    {
        PlayerPrefs.SetInt("PlayTime", 60);
        PlayerPrefs.SetInt("BonusTime", 1);
        sceneLoader.LoadScene("Game");
    }
    public void onClick2to1()
    {
        PlayerPrefs.SetInt("PlayTime", 120);
        PlayerPrefs.SetInt("BonusTime", 1);
        sceneLoader.LoadScene("Game");
    }
    public void onClick3()
    {
        PlayerPrefs.SetInt("PlayTime", 180);
        sceneLoader.LoadScene("Game");
    }
    public void onClick3to2()
    {
        PlayerPrefs.SetInt("PlayTime", 180);
        PlayerPrefs.SetInt("BonusTime", 2);
        sceneLoader.LoadScene("Game");
    }
    public void onClick5()
    {
        PlayerPrefs.SetInt("PlayTime", 300);
        sceneLoader.LoadScene("Game");
    }
    public void onClick10()
    {
        PlayerPrefs.SetInt("PlayTime", 600);
        sceneLoader.LoadScene("Game");
    }
    public void onClick15to10()
    {
        PlayerPrefs.SetInt("PlayTime", 900);
        PlayerPrefs.SetInt("BonusTime", 10);
        sceneLoader.LoadScene("Game");
    }
    public void onClick30()
    {
        PlayerPrefs.SetInt("PlayTime", 1800);
        sceneLoader.LoadScene("Game");
    }
}
