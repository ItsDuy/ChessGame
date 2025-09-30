using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Chess/Chessman")]
public class Chessman : MonoBehaviour
{
    #region Component References
    public GameObject controller;
    public GameObject movePlate;
    #endregion

    #region Position Variables
    private int xBoard = -1;
    private int yBoard = -1;
    private string player;
    #endregion

    #region Piece Sprites
    public Sprite black_King, black_Queen, black_Rook, black_Bishop, black_Knight, black_Pawn;
    public Sprite white_King, white_Queen, white_Rook, white_Bishop, white_Knight, white_Pawn;
    #endregion

    #region Movement Flags
    public bool HasMoved { get; private set; } = false;
    private bool justMoved2Squares = false;
    public bool JustMoved2Squares { get { return justMoved2Squares; } }
    private bool isEnPassant = false;
    private static GameObject lastMovedPawn = null;
    public bool hasMovedKingsideRook = false;
    public bool hasMovedQueensideRook = false;
    #endregion

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        movePlate = controller.GetComponent<Game>().movePlate;

        SetCoords();
        switch (this.name)
        {
            case "black_pawn":
                this.GetComponent<SpriteRenderer>().sprite = black_Pawn;
                player = "black";
                break;
            case "black_rook":
                this.GetComponent<SpriteRenderer>().sprite = black_Rook;
                player = "black";
                break;
            case "black_knight":
                this.GetComponent<SpriteRenderer>().sprite = black_Knight;
                player = "black";
                break;
            case "black_bishop":
                this.GetComponent<SpriteRenderer>().sprite = black_Bishop;
                player = "black";
                break;
            case "black_queen":
                this.GetComponent<SpriteRenderer>().sprite = black_Queen;
                player = "black";
                break;
            case "black_king":
                this.GetComponent<SpriteRenderer>().sprite = black_King;
                player = "black";
                break;

            //same for white pieces
            case "white_pawn":
                this.GetComponent<SpriteRenderer>().sprite = white_Pawn;
                player = "white";
                break;
            case "white_rook":
                this.GetComponent<SpriteRenderer>().sprite = white_Rook;
                player = "white";
                break;
            case "white_knight":
                this.GetComponent<SpriteRenderer>().sprite = white_Knight;
                player = "white";
                break;
            case "white_bishop":
                this.GetComponent<SpriteRenderer>().sprite = white_Bishop;
                player = "white";
                break;
            case "white_queen":
                this.GetComponent<SpriteRenderer>().sprite = white_Queen;
                player = "white";
                break;
            case "white_king":
                this.GetComponent<SpriteRenderer>().sprite = white_King;
                player = "white";
                break;
        }
    }
    public void SetCoords()
    {
        float x = xBoard;
        float y = yBoard;
        x = x * 0.66f - 2.3f;
        y = y * 0.66f - 2.3f;

        this.transform.position = new Vector3(x, y, -1.0f);

    }
    public int GetXBoard()
    {
        return xBoard;
    }
    public void SetXBoard(int x)
    {
        xBoard = x;
    }
    public int GetYBoard()
    {
        return yBoard;
    }
    public void SetYBoard(int y)
    {
        yBoard = y;
    }
    public void SetPlayer(string player)
    {
        this.player = player;
    }
    public string GetPlayer()
    {
        return player;
    }
    private void OnMouseUp()
    {
        if (!controller.GetComponent<Game>().isGameOver() && controller.GetComponent<Game>().GetplayerTurn() == player)
        {
            DestroyMovePlates();
            IntiateMovePlates();
        }
    }
    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }
    public void IntiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(-1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(0, -1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(-1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(0, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                if (!HasMoved) // Check castling only if king hasn't moved
                    CheckForCastling();
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }
    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();
        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else
            {
                if (cp.GetComponent<Chessman>().GetPlayer() != player)
                {
                    MovePlateSpawn(x, y, true);
                }
                break;
            }

            x += xIncrement;
            y += yIncrement;
        }
    }
    public void LMovePlate()
    {
        Game sc = controller.GetComponent<Game>();
        PointCheck(xBoard + 1, yBoard + 2);
        PointCheck(xBoard - 1, yBoard + 2);
        PointCheck(xBoard + 2, yBoard + 1);
        PointCheck(xBoard + 2, yBoard - 1);
        PointCheck(xBoard + 1, yBoard - 2);
        PointCheck(xBoard - 1, yBoard - 2);
        PointCheck(xBoard - 2, yBoard + 1);
        PointCheck(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        Game sc = controller.GetComponent<Game>();

        // Check all 8 surrounding squares for the king
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip current position

                int newX = xBoard + dx;
                int newY = yBoard + dy;

                if (sc.PositionOnBoard(newX, newY))
                {
                    GameObject targetPiece = sc.GetPosition(newX, newY);
                    bool isAttack = targetPiece != null && targetPiece.GetComponent<Chessman>().GetPlayer() != player;

                    // If square is empty or has enemy piece
                    if (targetPiece == null || isAttack)
                    {
                        MovePlateSpawn(newX, newY, isAttack);
                    }
                }
            }
        }

        // Check for castling only if king hasn't moved
        if (!HasMoved)
            CheckForCastling();
    }

    private void PointCheck(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);
            if (cp == null)
                MovePlateSpawn(x, y);
            else if (cp.GetComponent<Chessman>().GetPlayer() != player)
                MovePlateSpawn(x, y, true);
        }
    }
    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject go = sc.GetChessman(x, y);
            if (go == null)
            {
                GameObject movePlate = Instantiate(sc.movePlate, new Vector3(x * 0.66f - 2.3f, y * 0.66f - 2.3f, -2.0f), Quaternion.identity);
                MovePlate mp = movePlate.GetComponent<MovePlate>();
                mp.SetCoords(x, y);
                mp.SetReference(this.gameObject);
                if (go != null)
                {
                    if (go.GetComponent<Chessman>().GetPlayer() != this.GetPlayer())
                    {
                        mp.attack = true;
                    }
                }
            }
        }
    }

    #region Movement Validation
    public bool IsPathClear(int startX, int startY, int endX, int endY)
    {
        Game sc = controller.GetComponent<Game>();
        int dx = endX - startX;
        int dy = endY - startY;
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
        int x = startX + stepX;
        int y = startY + stepY;
        while (x != endX || y != endY)
        {
            if (sc.GetPosition(x, y) != null)
                return false;
            if (x != endX) x += stepX;
            if (y != endY) y += stepY;
        }
        return true;
    }
    #endregion

    #region Castling Logic
    public void CheckForCastling()
    {
        Game sc = controller.GetComponent<Game>();
        int row = (player == "white") ? 0 : 7;

        // Try castling moves if king hasn't moved
        if (!HasMoved)
        {
            // Try both kingside and queenside castle
            MovePlateSpawn(xBoard + 2, row, false, true);
            MovePlateSpawn(xBoard - 2, row, false, true);

        }
    }
    #endregion

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();

        // Forward movement
        if (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                MovePlateSpawn(x, y);

                // First move - can move 2 squares if in starting position
                if ((player == "black" && yBoard == 6) || (player == "white" && yBoard == 1))
                {
                    int twoSquaresY = player == "black" ? y - 1 : y + 1;
                    if (sc.GetPosition(x, twoSquaresY) == null)
                    {
                        GameObject mp = MovePlateSpawn(x, twoSquaresY);
                        if (mp != null)
                        {
                            MovePlate mpScript = mp.GetComponent<MovePlate>();
                            mpScript.isTwoSquareMove = true;
                        }
                    }
                }
            }
        }

        // Diagonal attack moves
        int[] attackX = { x - 1, x + 1 };
        foreach (int attackPosX in attackX)
        {
            // Normal diagonal capture
            if (sc.PositionOnBoard(attackPosX, y))
            {
                GameObject cp = sc.GetPosition(attackPosX, y);
                if (cp != null && cp.GetComponent<Chessman>().GetPlayer() != player)
                {
                    MovePlateSpawn(attackPosX, y, true);
                }
            }

            // En Passant check - more explicit verification
            if (sc.PositionOnBoard(attackPosX, yBoard))
            {
                Debug.Log($"[En Passant] Checking at position ({attackPosX},{yBoard}) for {player} pawn");
                GameObject adjacentPawn = sc.GetPosition(attackPosX, yBoard);

                // Check for en passant conditions
                if (adjacentPawn != null)
                {
                    Chessman adjacentChessman = adjacentPawn.GetComponent<Chessman>();
                    bool isPawn = adjacentPawn.name.Contains("pawn");
                    bool isEnemy = adjacentChessman.GetPlayer() != player;
                    bool isLastMoved = adjacentPawn == lastMovedPawn;
                    bool justMoved2 = adjacentChessman.JustMoved2Squares;

                    Debug.Log($"[En Passant] Found {adjacentPawn.name}: isPawn={isPawn}, isEnemy={isEnemy}, isLastMoved={isLastMoved}, justMoved2={justMoved2}");

                    if (isPawn && isEnemy && isLastMoved && justMoved2)
                    {
                        // The correct y position for en passant capture (behind the pawn)
                        int captureY = player == "white" ? yBoard + 1 : yBoard - 1;

                        // Create a move plate directly without using the IsMoveLegal check
                        // since en passant is a special case
                        float xCoord = attackPosX * 0.66f - 2.3f;
                        float yCoord = captureY * 0.66f - 2.3f;

                        Debug.Log($"[En Passant] Creating move plate at ({attackPosX},{captureY}) - Valid en passant detected!");

                        GameObject mp = Instantiate(controller.GetComponent<Game>().movePlate,
                                                   new Vector3(xCoord, yCoord, -3.0f), Quaternion.identity);

                        if (mp != null)
                        {
                            MovePlate mpScript = mp.GetComponent<MovePlate>();
                            mpScript.SetCoords(attackPosX, captureY);
                            mpScript.SetReference(this.gameObject);
                            mpScript.attack = true; // En passant is always an attack
                            mpScript.isEnPassant = true;
                            mpScript.SetEnPassantTarget(adjacentPawn);

                            // Set color to red for attack
                            mp.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);

                            Debug.Log($"[En Passant] Target set: {adjacentPawn.name} at ({adjacentChessman.GetXBoard()},{adjacentChessman.GetYBoard()})");
                        }
                        else
                        {
                            Debug.LogError("[En Passant] Failed to create move plate object");
                        }
                    }
                    else
                    {
                        Debug.Log("[En Passant] Not all conditions met for en passant capture");
                    }
                }
            }
        }
    }
    public GameObject MovePlateSpawn(int x, int y, bool isAttack = false, bool isCastle = false)
    {
        float xCoord = x * 0.66f - 2.3f;
        float yCoord = y * 0.66f - 2.3f;
        Game sc = controller.GetComponent<Game>();
        if (sc.IsMoveLegal(gameObject, x, y))
        {
            GameObject mp = Instantiate(controller.GetComponent<Game>().movePlate, new Vector3(xCoord, yCoord, -3.0f), Quaternion.identity);
            MovePlate mpp = mp.GetComponent<MovePlate>();
            mpp.SetCoords(x, y);
            mpp.SetReference(this.gameObject);
            if (isAttack)
            {
                mpp.attack = true;
                mpp.isEnPassant = isEnPassant;
            // Set color to red for attack
                mp.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            }
            else if (isCastle)
            {
                mpp.isCastling = true;
                // Set a different color for castle moves (blue)
                mp.GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            }
            else
            {
                // Default color for normal moves
                mp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            }
            return mp;
        }
        return null;
    }
    public void SetHasMoved()
    {
        HasMoved = true;

        // Check if this is a pawn
        if (this.name.Contains("pawn"))
        {
            // Reset previous pawn's "just moved 2 squares" flag first
            if (lastMovedPawn != null && lastMovedPawn != this.gameObject)
            {
                lastMovedPawn.GetComponent<Chessman>().justMoved2Squares = false;
            }

            // Update the lastMovedPawn reference
            lastMovedPawn = this.gameObject;
            // Note: justMoved2Squares is set in MovePlate.OnMouseUp based on isTwoSquareMove
        }

        // Set specific rook flags if this chessman is a rook
        if (this.name.Contains("rook"))
        {
            if (this.GetXBoard() == 0)
                hasMovedQueensideRook = true;
            else if (this.GetXBoard() == 7)
                hasMovedKingsideRook = true;
        }
    }

    // Method to set justMoved2Squares flag
    public void SetJustMoved2Squares(bool value)
    {
        justMoved2Squares = value;
    }

    #region Special Move Tracking
    public bool HasJustMoved2Squares() { return justMoved2Squares; }
    public static GameObject GetLastMovedPawn() { return lastMovedPawn; }
    public static void SetLastMovedPawn(GameObject pawn) { lastMovedPawn = pawn; }
    public bool GetIsEnPassant() { return isEnPassant; }
    public void SetIsEnPassant(bool value) { isEnPassant = value; }
    #endregion
}
