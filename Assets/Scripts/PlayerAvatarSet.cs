using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    [Tooltip("UI Image to display Player 1's avatar")]
    public Image player1AvatarImage;

    [Tooltip("UI Image to display Player 2's avatar")]
    public Image player2AvatarImage;

    void Start()
    {
        // Load avatars from PlayerAvatarManager
        DisplaySavedAvatars();
    }

    public void DisplaySavedAvatars()
    {
        // Get the sprites from PlayerAvatarManager
        Sprite player1Avatar = PlayerAvatarManager.GetPlayer1Avatar();
        Sprite player2Avatar = PlayerAvatarManager.GetPlayer2Avatar();

        // Assign the sprites to UI images
        if (player1AvatarImage != null && player1Avatar != null)
        {
            player1AvatarImage.sprite = player1Avatar;
        }

        if (player2AvatarImage != null && player2Avatar != null)
        {
            player2AvatarImage.sprite = player2Avatar;
        }
    }
}
