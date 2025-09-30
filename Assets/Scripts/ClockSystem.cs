using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClockSystem : MonoBehaviour
{
    [SerializeField] private float whiteTimer = 180f;
    [SerializeField] private float blackTimer = 180f;
    
    public TextMeshProUGUI whiteTimerText;
    public TextMeshProUGUI blackTimerText;
    public Image whiteTimerBackground;  // Add reference to white timer background
    public Image blackTimerBackground;  // Add reference to black timer background
    public Image whiteBorder;  // Add reference to white border
    public Image blackBorder;  // Add reference to black border
    
    private Color activeColor = Color.red;
    private Color inactiveColor = Color.white;
    
    private bool isWhiteTimerRunning = false;
    private bool isBlackTimerRunning = false;
    private Game gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<Game>();
        // Initialize timers based on selected play time
        int selectedPlayTime = PlayerPrefs.GetInt("PlayTime", 180);
        whiteTimer = selectedPlayTime;
        blackTimer = selectedPlayTime;
        // Update timer display at the beginning
        UpdateWhiteTimerDisplay();
        UpdateBlackTimerDisplay();
        // Wait for 2 seconds before starting the chess game
        StartCoroutine(DelayedStart());
    }
    
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        // Start white timer since white moves first after delay
        StartWhiteTimer();
        SetWhiteActive();
    }

    void Update()
    {
        // Stop updating if game is over
        if (gameManager != null && gameManager.isGameOver())
            return;

        if (isWhiteTimerRunning)
        {
            if (whiteTimer > 0)
            {
                whiteTimer -= Time.deltaTime;
                UpdateWhiteTimerDisplay();
                if (whiteTimer <= 0)
                {
                    whiteTimer = 0;
                    isWhiteTimerRunning = false;
                    HandleTimeOut("white");
                }
            }
        }

        if (isBlackTimerRunning)
        {
            if (blackTimer > 0)
            {
                blackTimer -= Time.deltaTime;
                UpdateBlackTimerDisplay();
                if (blackTimer <= 0)
                {
                    blackTimer = 0;
                    isBlackTimerRunning = false;
                    HandleTimeOut("black");
                }
            }
        }
    }

    private void UpdateWhiteTimerDisplay()
    {
        if (whiteTimerText != null)
        {
            int minutes = Mathf.FloorToInt(whiteTimer / 60);
            int seconds = Mathf.FloorToInt(whiteTimer % 60);
            whiteTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateBlackTimerDisplay()
    {
        if (blackTimerText != null)
        {
            int minutes = Mathf.FloorToInt(blackTimer / 60);
            int seconds = Mathf.FloorToInt(blackTimer % 60);
            blackTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void SwitchTimers()
    {
        // Added bonus time for the player finishing their move
        int bonus = PlayerPrefs.GetInt("BonusTime", 0);
        if (isWhiteTimerRunning)
        {
            whiteTimer += bonus;
            StopWhiteTimer();
            StartBlackTimer();
        }
        else
        {
            blackTimer += bonus;
            StopBlackTimer();
            StartWhiteTimer();
        }
    }

    private void StartWhiteTimer()
    {
        isWhiteTimerRunning = true;
        isBlackTimerRunning = false;
        SetWhiteActive();
    }

    private void StartBlackTimer()
    {
        isBlackTimerRunning = true;
        isWhiteTimerRunning = false;
        SetBlackActive();
    }

    private void SetWhiteActive()
    {
        // Change background colors to show active/inactive state
        if (whiteTimerBackground != null && whiteBorder != null)
        {
            whiteBorder.color = activeColor;  // Set to red for active player
        }
        if (blackTimerBackground != null && blackBorder != null)
        {
            blackBorder.color = inactiveColor;  // Set to white for inactive player
        }
    }

    private void SetBlackActive()
    {
        // Change background colors to show active/inactive state
        if (blackTimerBackground != null && blackBorder != null)
        {
            blackBorder.color = activeColor;  // Set to red for active player
        }
        if (whiteTimerBackground != null && whiteBorder != null)
        {
            whiteBorder.color = inactiveColor;  // Set to white for inactive player
        }
    }

    private void StopWhiteTimer()
    {
        isWhiteTimerRunning = false;
    }

    private void StopBlackTimer()
    {
        isBlackTimerRunning = false;
    }

    private void HandleTimeOut(string player)
    {
        StopAllTimers();
        if (gameManager != null)
        {
            Debug.Log($"{player} lost on time!");
            gameManager.EndGameOnTime(player);
        }
    }

    public void StopAllTimers()
    {
        isWhiteTimerRunning = false;
        isBlackTimerRunning = false;
        // Reset both backgrounds to inactive when game ends
        if (whiteTimerBackground != null) whiteTimerBackground.color = inactiveColor;
        if (blackTimerBackground != null) blackTimerBackground.color = inactiveColor;
        if (whiteBorder != null) whiteBorder.color = inactiveColor;
        if (blackBorder != null) blackBorder.color = inactiveColor;
    }
}
