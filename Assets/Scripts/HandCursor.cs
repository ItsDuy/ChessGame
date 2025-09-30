using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add UI namespace

public class HandCursor : MonoBehaviour
{
    [SerializeField] private Animator handCursorAnimator;
    [SerializeField] private float smoothFactor = 0.2f;//lower is smoother
    [SerializeField] private string clickTriggerName = "Click";
    [SerializeField] private Vector2 cursorOffset = Vector2.zero;//Set the offset so that the pos match with the actual cursor
    [SerializeField] private bool hideSystemCursor = true;//Hide the actual cursor

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Start()
    {
        handCursorAnimator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (rectTransform == null)
        {
            Debug.LogError("HandCursor requires a RectTransform component!");
            enabled = false;
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("HandCursor must be a child of a Canvas!");
            enabled = false;
            return;
        }

        if (hideSystemCursor)
        {
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Get mouse position
        Vector2 mousePos = Input.mousePosition;

        // Apply offset
        mousePos += cursorOffset;

        // Convert to local position within canvas
        Vector2 targetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera,
            out targetPos);

        // Set the position (smoothly)
        rectTransform.localPosition = Vector3.Lerp(
            rectTransform.localPosition,
            targetPos,
            smoothFactor);

        // Handle click animation
        if (Input.GetMouseButtonDown(0))
        {
            if (handCursorAnimator != null)
            {
                handCursorAnimator.SetTrigger(clickTriggerName);
            }
        }
    }

    private void OnDestroy()
    {
        // Make sure to restore cursor visibility when script is destroyed
        Cursor.visible = true;
    }
}