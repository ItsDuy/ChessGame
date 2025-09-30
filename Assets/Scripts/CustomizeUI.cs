using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CustomizeUI : MonoBehaviour
{
    public GameObject customizePanel;
    public GameObject avatarPanel;
    public GameObject settingPanel;
    public GameObject HelpPanel;
    public GameObject mainUIPanel;
    public void onAvatarClick()
    {
        customizePanel.SetActive(false);
        avatarPanel.SetActive(true);
    }
    public void onBackToMainUIClick()
    {
        mainUIPanel.SetActive(true);
        customizePanel.SetActive(false);
    }
    public void onSettingClick()
    {
        customizePanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void onHelpClick()
    {
        customizePanel.SetActive(false);
        HelpPanel.SetActive(true);
    }
    public void onBackToCustomizeClick()
    {
        HelpPanel.SetActive(false);
        customizePanel.SetActive(true);
        settingPanel.SetActive(false);
        avatarPanel.SetActive(false);
    }
}
