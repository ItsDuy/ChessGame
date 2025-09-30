using UnityEngine;
using UnityEngine.UI;

// Static manager to store selected avatar sprites between scenes
[AddComponentMenu("AnhDuy/PlayerAvatarManager")]
public static class PlayerAvatarManager
{
    private static Sprite player1Avatar;
    private static Sprite player2Avatar;

    // Set the avatar sprites
    public static void SetAvatars(Sprite p1Avatar, Sprite p2Avatar)
    {
        player1Avatar = p1Avatar;
        player2Avatar = p2Avatar;
    }

    // Get Player 1 avatar
    public static Sprite GetPlayer1Avatar()
    {
        return player1Avatar;
    }

    // Get Player 2 avatar
    public static Sprite GetPlayer2Avatar()
    {
        return player2Avatar;
    }
}
