using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromotionScript : MonoBehaviour
{
    public Canvas BlackPromotionCanvas;
    public Canvas WhitePromotionCanvas;
    public Game game;
    private string player;
    private GameObject promotingPawn;
    private int promotionX;
    private int promotionY;

    public void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        HidePromotionCanvas();
    }

    public void ShowPromotion(string currentPlayer, GameObject pawn)
    {
        player = currentPlayer;
        promotingPawn = pawn;
        promotionX = pawn.GetComponent<Chessman>().GetXBoard();
        promotionY = pawn.GetComponent<Chessman>().GetYBoard();
        if (player == "black")
        {
            BlackPromotionCanvas.enabled = true;
            WhitePromotionCanvas.enabled = false;
        }
        else
        {
            WhitePromotionCanvas.enabled = true;
            BlackPromotionCanvas.enabled = false;
        }
    }
    public void PromoteToQueen() => PromotePawn("queen");
    public void PromoteToRook() => PromotePawn("rook");
    public void PromoteToBishop() => PromotePawn("bishop");
    public void PromoteToKnight() => PromotePawn("knight");

    private void PromotePawn(string pieceType)
    {
        if (promotingPawn != null)
        {
            string newPieceName = $"{player}_{pieceType}";
            promotingPawn.name = newPieceName;

            Chessman chessman = promotingPawn.GetComponent<Chessman>();
            chessman.Activate();

            HidePromotionCanvas();
            GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        }
    }
    public void HidePromotionCanvas()
    {
        BlackPromotionCanvas.enabled = false;
        WhitePromotionCanvas.enabled = false;
    }

}