using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    #region References
    public GameObject controller;
    private AudioManager audioManager;
    private PromotionScript promotionScript;
    GameObject reference = null;
    #endregion

    #region Move Properties
    public bool isCastling = false;
    public bool attack = false;
    public bool isEnPassant = false;
    int matrixX, matrixY;
    private GameObject enPassantTarget = null;
    public bool isTwoSquareMove = false;
    #endregion

    public void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        if (attack)
        {
            this.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);//red
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 1);//green
        }
        promotionScript = GameObject.FindGameObjectWithTag("PromotionUI").GetComponent<PromotionScript>();

        if (promotionScript == null)
        {
            Debug.LogError("PromotionScript component not found!");
        }
    }

    #region Move Execution
    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game sc = controller.GetComponent<Game>();

        if (reference == null) return;

        Chessman cm = reference.GetComponent<Chessman>();

        if (sc.WouldPutKingInCheck(reference, matrixX, matrixY))
            return;

        // Set flags for the moved piece BEFORE executing the move
        if (reference.name.EndsWith("pawn") && isTwoSquareMove)
        {
            reference.GetComponent<Chessman>().SetJustMoved2Squares(true);
            // Debug.Log($"Set JustMoved2Squares to true for {reference.name}");
        }
        else
        {
            // Reset the justMoved2Squares flag for all other moves
            reference.GetComponent<Chessman>().SetJustMoved2Squares(false);
        }

        // Execute the move with appropriate sounds
        if (isCastling)
        {
            ExecuteCastling(sc);
            audioManager.PlayCastlingSound();
        }
        else if (attack)
        {
            // Normal capture
            GameObject cp = sc.GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                sc.MoveChessman(reference, matrixX, matrixY);
                Destroy(cp);
                audioManager.PlayCaptureSound();
                Debug.Log("Regular capture executed");
            }
            // En Passant capture
            else if (isEnPassant)
            {
                Debug.Log($"[En Passant] Executing capture at ({matrixX},{matrixY})");

                if (enPassantTarget != null)
                {
                    Debug.Log($"[En Passant] Target: {enPassantTarget.name} at position ({enPassantTarget.GetComponent<Chessman>().GetXBoard()},{enPassantTarget.GetComponent<Chessman>().GetYBoard()})");

                    // Verify target is still valid
                    if (sc.GetPosition(enPassantTarget.GetComponent<Chessman>().GetXBoard(),
                                      enPassantTarget.GetComponent<Chessman>().GetYBoard()) == enPassantTarget)
                    {
                        sc.MoveChessman(reference, matrixX, matrixY);
                        sc.SetPositionEmpty(enPassantTarget.GetComponent<Chessman>().GetXBoard(),
                                          enPassantTarget.GetComponent<Chessman>().GetYBoard());
                        Destroy(enPassantTarget);
                        audioManager.PlayCaptureSound();
                        Debug.Log("[En Passant] Capture executed successfully!");
                    }
                    else
                    {
                        Debug.LogError("[En Passant] Target pawn is no longer at expected position!");
                        // Fallback to normal move since target is invalid
                        sc.MoveChessman(reference, matrixX, matrixY);
                        audioManager.PlayMoveSound();
                    }
                }
                else
                {
                    Debug.LogError("[En Passant] Attempting en passant capture but target is null!");
                    // Fallback to normal move
                    sc.MoveChessman(reference, matrixX, matrixY);
                    audioManager.PlayMoveSound();
                }
            }
        }
        else
        {
            sc.MoveChessman(reference, matrixX, matrixY);
            audioManager.PlayMoveSound();
        }

        reference.GetComponent<Chessman>().SetHasMoved();

        // Pawn promotion handling
        if (reference.name.Contains("pawn"))
        {
            HandlePawnPromotion(sc);
        }
        else
        {
            Chessman.SetLastMovedPawn(null);
        }

        // Complete the move
        FinalizeMovePosition(sc);

        // Check if the move put opponent in check
        string oppositePlayer = cm.GetPlayer() == "white" ? "black" : "white";
        if (sc.IsInCheck(oppositePlayer))
        {
            audioManager.PlayCheckSound();
        }
    }
    #endregion

    private void HandlePawnPromotion(Game sc)
    {
        Chessman.SetLastMovedPawn(reference);
        if (reference.GetComponent<Chessman>().GetPlayer() == "white" && matrixY == 7)
        {
            promotionScript.ShowPromotion("white", reference);
            audioManager.PlayPromotionSound();
        }
        else if (reference.GetComponent<Chessman>().GetPlayer() == "black" && matrixY == 0)
        {
            promotionScript.ShowPromotion("black", reference);
            audioManager.PlayPromotionSound();
        }
        Chessman.SetLastMovedPawn(reference);
    }

    private void FinalizeMovePosition(Game sc)
    {
        sc.SetPositionEmpty(
            reference.GetComponent<Chessman>().GetXBoard(),
            reference.GetComponent<Chessman>().GetYBoard());
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();
        sc.SwitchPlayerTurn();
        sc.SetPosition(reference);
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    #region Castling Execution
    private void ExecuteCastling(Game sc)
    {
        // Get the king
        Chessman king = reference.GetComponent<Chessman>();
        int originalX = king.GetXBoard();
        bool isKingside = matrixX > originalX;

        // Move king
        sc.SetPositionEmpty(originalX, matrixY);
        king.SetXBoard(matrixX);
        king.SetYBoard(matrixY);
        king.SetCoords();
        king.SetHasMoved();
        sc.SetPosition(reference);

        // Move rook
        int rookStartX = isKingside ? 7 : 0;
        int rookEndX = isKingside ? (matrixX - 1) : (matrixX + 1);
        GameObject rook = sc.GetPosition(rookStartX, matrixY);

        if (rook != null)
        {
            sc.SetPositionEmpty(rookStartX, matrixY);
            rook.GetComponent<Chessman>().SetXBoard(rookEndX);
            rook.GetComponent<Chessman>().SetYBoard(matrixY);
            rook.GetComponent<Chessman>().SetCoords();
            rook.GetComponent<Chessman>().SetHasMoved();
            sc.SetPosition(rook);
        }
    }
    #endregion

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }
    public GameObject GetReference()
    {
        return reference;
    }

    public void SetEnPassantTarget(GameObject target)
    {
        enPassantTarget = target;
    }

    private void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject mp in movePlates)
        {
            Destroy(mp);
        }
    }

    public GameObject MovePlateSpawn(int x, int y, bool isAttack = false, bool isCastle = false)
    {
        float xCoord = x * 0.66f - 2.3f;
        float yCoord = y * 0.66f - 2.3f;
        Game sc = controller.GetComponent<Game>();

        // Special handling for castle moves


        if (sc.IsMoveLegal(gameObject, x, y))
        {
            GameObject mp = Instantiate(controller.GetComponent<Game>().movePlate,
                new Vector3(xCoord, yCoord, -3.0f), Quaternion.identity);
            MovePlate mpp = mp.GetComponent<MovePlate>();
            mpp.SetCoords(x, y);
            mpp.SetReference(this.gameObject);
            mpp.isCastling = isCastle;

            if (isCastle)
            {
                mp.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.75f); // Blue for castle moves
            }
            else if (isAttack)
            {
                mpp.attack = true;
            }

            // If this is marked as an en passant move, set it as an attack move as well
            if (mpp.isEnPassant)
            {
                mpp.attack = true;
                mp.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1); // Red color for attack
            }

            return mp;
        }

        return null;
    }
}
