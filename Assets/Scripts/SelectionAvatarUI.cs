using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add this for scene transitions

public class SelectionAvatarUI : MonoBehaviour
{
    public GameObject gameModeUI;
    public GameObject customizeAvatatrUI;
    // Add public arrays to assign each row's avatars via the Inspector
    public GameObject[] row1;
    public GameObject[] row2;
    public GameObject[] row3;

    // Reference to the 2D avatars array
    public GameObject[][] avatars;

    // Add arrays for borders
    public Image[] borders1;  // borders for row 1
    public Image[] borders2;  // borders for row 2
    public Image[] borders3;  // borders for row 3
    private Image[][] borders;

    // New fields to track each player's selection position (row, col)
    Vector2Int player1Pos = new Vector2Int(0, 0);
    Vector2Int player2Pos = new Vector2Int(0, 0);
    int maxRows = 3, maxCols = 2; // 3 rows, 2 columns

    private Color player1Color = Color.red;
    private Color player2Color = Color.blue;
    private Color unselectedColor = Color.white;

    // Track previously selected avatars to reset their colors
    private GameObject previousPlayer1Avatar;
    private GameObject previousPlayer2Avatar;

    // Add scene name for transition
    public string playSceneName = "PlayScene";

    void Awake()
    {
        // Initialize the avatars array with the assigned rows
        avatars = new GameObject[][] { row1, row2, row3 };
        // Initialize borders array
        borders = new Image[][] { borders1, borders2, borders3 };

        SetupBorders();
    }

    void SetupBorders()
    {
        // Initialize all borders to white
        foreach (Image[] borderRow in borders)
        {
            foreach (Image border in borderRow)
            {
                if (border != null)
                {
                    border.color = unselectedColor;
                }
            }
        }
    }

    void Update()
    {
        // Player1 controls (arrow keys)
        if (Input.GetKeyDown(KeyCode.UpArrow) && player1Pos.x > 0)
            player1Pos.x--;
        if (Input.GetKeyDown(KeyCode.DownArrow) && player1Pos.x < maxRows - 1)
            player1Pos.x++;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && player1Pos.y > 0)
            player1Pos.y--;
        if (Input.GetKeyDown(KeyCode.RightArrow) && player1Pos.y < maxCols - 1)
            player1Pos.y++;

        // Player2 controls (WASD)
        if (Input.GetKeyDown(KeyCode.W) && player2Pos.x > 0)
            player2Pos.x--;
        if (Input.GetKeyDown(KeyCode.S) && player2Pos.x < maxRows - 1)
            player2Pos.x++;
        if (Input.GetKeyDown(KeyCode.A) && player2Pos.y > 0)
            player2Pos.y--;
        if (Input.GetKeyDown(KeyCode.D) && player2Pos.y < maxCols - 1)
            player2Pos.y++;

        // Update the UI highlight and avatar display
        UpdateAvatarDisplay();

        // Confirm selection
        if (Input.GetKeyDown(KeyCode.Return))
            ConfirmSelection();
    }

    void UpdateAvatarDisplay()
    {
        // Reset all borders to unselected first
        foreach (Image[] borderRow in borders)
        {
            foreach (Image border in borderRow)
            {
                if (border != null)
                {
                    border.color = unselectedColor;
                }
            }
        }

        // Update current selections
        if (borders[player1Pos.x][player1Pos.y] != null)
        {
            borders[player1Pos.x][player1Pos.y].color = player1Color;
        }

        if (borders[player2Pos.x][player2Pos.y] != null)
        {
            borders[player2Pos.x][player2Pos.y].color = player2Color;
        }
    }

    void ConfirmSelection()
    {
        // Save chosen avatars into PlayerPrefs (e.g., saving row and column indexes)
        PlayerPrefs.SetInt("Player1AvatarRow", player1Pos.x);
        PlayerPrefs.SetInt("Player1AvatarCol", player1Pos.y);
        PlayerPrefs.SetInt("Player2AvatarRow", player2Pos.x);
        PlayerPrefs.SetInt("Player2AvatarCol", player2Pos.y);
        PlayerPrefs.Save();

        // Get selected sprites from the avatars array
        Sprite whiteSprite = avatars[player1Pos.x][player1Pos.y].GetComponent<UnityEngine.UI.Image>().sprite;
        Sprite blackSprite = avatars[player2Pos.x][player2Pos.y].GetComponent<UnityEngine.UI.Image>().sprite;

        // Update the static PlayerAvatarManager
        PlayerAvatarManager.SetAvatars(whiteSprite, blackSprite);

        // Transition to the play scene
        ShowGameModeUI();
    }
    void ShowGameModeUI()
    {
        // Hide the customize avatar UI
            customizeAvatatrUI.SetActive(false);
            gameModeUI.SetActive(true);
        
    }
}
