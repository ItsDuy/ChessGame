using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Unity.Mathematics;
public class Game : MonoBehaviour
{
    #region Game State
    private string playerTurn = "white";
    private bool gameOver = false;
    private int moveCounter = 0; // Add move counter
    private bool initialPause = true; // Add flag for initial pause
    #endregion



    #region Board State
    private GameObject[,] chessBoard = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];
    #endregion

    #region References
    public GameObject chessPiece;
    public AudioManager audioManager;
    public Canvas WinCanvas;
    public Canvas PlayerCanvas;
    public GameObject movePlate;
    private ClockSystem clockSystem;
    #endregion

    #region Move Validation
    public bool WouldMovePreventCheck(GameObject piece, int targetX, int targetY, string player)
    {
        Chessman cm = piece.GetComponent<Chessman>();

        // Store original position
        int originalX = cm.GetXBoard();
        int originalY = cm.GetYBoard();
        GameObject capturedPiece = GetPosition(targetX, targetY);

        // Simulate move
        SetPositionEmpty(originalX, originalY);
        if (capturedPiece != null)
            SetPositionEmpty(targetX, targetY);

        cm.SetXBoard(targetX);
        cm.SetYBoard(targetY);
        SetPosition(piece);

        // Check if this move would prevent check
        bool wouldPreventCheck = !IsInCheck(player);

        // Restore position
        SetPositionEmpty(targetX, targetY);
        cm.SetXBoard(originalX);
        cm.SetYBoard(originalY);
        SetPosition(piece);
        if (capturedPiece != null)
            SetPosition(capturedPiece);

        return wouldPreventCheck;
    }
    public bool IsMoveLegal(GameObject piece, int targetX, int targetY)
    {
        // Base validation
        if (!PositionOnBoard(targetX, targetY))
            return false;

        Chessman cm = piece.GetComponent<Chessman>();
        GameObject targetPiece = GetPosition(targetX, targetY);

        // Cannot move to a square occupied by own piece
        if (targetPiece != null && targetPiece.GetComponent<Chessman>().GetPlayer() == cm.GetPlayer())
            return false;

        // Special handling for kings
        if (cm.name.Contains("king"))
        {
            if (!IsBasicMoveLegal(piece, targetX, targetY))
                return false;

            if (WouldPutKingInCheck(piece, targetX, targetY))
            {
                return false;
            }
            return true;
        }

        // For all other pieces, validate basic movement then check for exposing king.
        if (!IsBasicMoveLegal(piece, targetX, targetY))
            return false;

        // Instead of checking IsInCheck separately, we simulate the move for every piece.
        return !WouldPutKingInCheck(piece, targetX, targetY);
    }

    private bool IsSquareProtectedByEnemy(int x, int y, string attackingPlayer)
    {
        string defendingPlayer = (attackingPlayer == "white") ? "black" : "white";
        GameObject targetPiece = GetPosition(x, y);

        // Temporarily remove the piece at target position to check if it's protected
        if (targetPiece != null)
            SetPositionEmpty(x, y);

        bool isProtected = IsSquareUnderAttack(attackingPlayer, x, y);

        // Restore the piece
        if (targetPiece != null)
            SetPosition(targetPiece);

        return isProtected;
    }

    private bool CanPieceAttackSquare(GameObject piece, int targetX, int targetY)
    {
        if (piece == null) return false;

        Chessman cm = piece.GetComponent<Chessman>();
        GameObject targetPiece = GetPosition(targetX, targetY);

        // Don't count friendly pieces as attackers
        if (targetPiece != null && targetPiece.GetComponent<Chessman>().GetPlayer() == cm.GetPlayer())
            return false;

        return IsBasicMoveLegal(piece, targetX, targetY);
    }
    private bool CanPieceBlockAttack(GameObject piece, Vector2Int blockingSquare)
    {
        Chessman cm = piece.GetComponent<Chessman>();

        // For pawns, we need special handling since they move differently than they capture
        if (piece.name.Contains("pawn"))
        {
            int direction = cm.GetPlayer() == "white" ? 1 : -1;
            int currentX = cm.GetXBoard();
            int currentY = cm.GetYBoard();

            // Pawns can only move forward, not sideways or backward
            if (blockingSquare.x != currentX)
                return false;

            // Check if the move is in the correct direction
            if ((blockingSquare.y - currentY) * direction <= 0)
                return false;

            // Check if it's a valid one or two square move
            int distance = Mathf.Abs(blockingSquare.y - currentY);
            if (distance > 2)
                return false;

            // Two square moves only from starting position
            if (distance == 2)
            {
                if ((cm.GetPlayer() == "white" && currentY != 1) ||
                    (cm.GetPlayer() == "black" && currentY != 6))
                    return false;

                // Check if path is clear for 2-square move
                int middleY = currentY + direction;
                if (GetPosition(currentX, middleY) != null)
                    return false;
            }
        }

        // Now use the regular move legality check
        return IsMoveLegal(piece, blockingSquare.x, blockingSquare.y) &&
               WouldMovePreventCheck(piece, blockingSquare.x, blockingSquare.y, cm.GetPlayer());
    }
    private bool IsBasicMoveLegal(GameObject piece, int targetX, int targetY)
    {
        Chessman cm = piece.GetComponent<Chessman>();
        int currentX = cm.GetXBoard();
        int currentY = cm.GetYBoard();

        // For kings - allow one square movement in any direction
        if (cm.name.Contains("king"))
        {
            int dx = Mathf.Abs(targetX - currentX);
            int dy = Mathf.Abs(targetY - currentY);

            // Check if it's a castling move
            if (dx == 2 && dy == 0)
            {
                return CanCastle(cm, targetX, targetY);
            }

            // Normal king move - one square in any direction
            return dx <= 1 && dy <= 1;
        }

        // For pawns
        if (cm.name.Contains("pawn"))
        {
            int direction = cm.GetPlayer() == "white" ? 1 : -1;
            bool isForwardMove = currentX == targetX && !IsOccupied(targetX, targetY);
            bool isDiagonalCapture = Mathf.Abs(targetX - currentX) == 1 &&
                                   (targetY - currentY) == direction &&
                                   IsOccupied(targetX, targetY);

            return isForwardMove || isDiagonalCapture;
        }

        // For knights
        if (cm.name.Contains("knight"))
        {
            int dx = Mathf.Abs(targetX - currentX);
            int dy = Mathf.Abs(targetY - currentY);
            return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
        }

        // For bishops, rooks and queens
        if (cm.name.Contains("bishop"))
        {
            if (Mathf.Abs(targetX - currentX) != Mathf.Abs(targetY - currentY))
                return false;
            return IsPathClear(currentX, currentY, targetX, targetY);
        }
        else if (cm.name.Contains("rook"))
        {
            if (targetX != currentX && targetY != currentY)
                return false;
            return IsPathClear(currentX, currentY, targetX, targetY);
        }
        else if (cm.name.Contains("queen"))
        {
            bool isDiagonal = Mathf.Abs(targetX - currentX) == Mathf.Abs(targetY - currentY);
            bool isStraight = targetX == currentX || targetY == currentY;
            if (!(isDiagonal || isStraight))
                return false;
            return IsPathClear(currentX, currentY, targetX, targetY);
        }

        return false;
    }

    private bool IsOccupied(int x, int y)
    {
        return GetPosition(x, y) != null;
    }

    public bool CanCastle(Chessman king, int targetX, int targetY)
    {
        string player = king.GetPlayer();
        if (king.HasMoved || IsInCheck(player))
        {
            return false;
        }

        bool isKingside = targetX > king.GetXBoard();
        int rookX = isKingside ? 7 : 0;
        GameObject rook = GetPosition(rookX, targetY);

        if (rook == null || !rook.name.Contains("rook"))
        {
            return false;
        }

        if (rook.GetComponent<Chessman>().HasMoved)
        {
            return false;
        }

        // Check if path is clear
        int start = king.GetXBoard();
        int end = targetX;
        for (int x = Mathf.Min(start, end) + 1; x < Mathf.Max(start, end); x++)
        {
            if (GetPosition(x, targetY) != null)
            {
                return false;
            }
        }



        return true;
    }

    private bool IsPathClearForCastling(Chessman king, int targetX, int targetY)
    {
        if (targetX > king.GetXBoard()) // Kingside
        {
            for (int x = king.GetXBoard() + 1; x < targetX; x++)
            {
                if (chessBoard[x, targetY] != null)
                    return false;
            }
            // Optional: Verify squares the king passes through are not under attack
            if (IsSquareUnderAttack(king.GetPlayer(), king.GetXBoard() + 1, targetY))
                return false;
        }
        else if (targetX < king.GetXBoard()) // Queenside
        {
            for (int x = targetX + 1; x < king.GetXBoard(); x++)
            {
                if (chessBoard[x, targetY] != null)
                    return false;
            }
            // Optional: Verify squares the king passes through are not under attack
            if (IsSquareUnderAttack(king.GetPlayer(), king.GetXBoard() - 1, targetY))
                return false;
        }

        return true;
    }

    private GameObject FindKing(string player)
    {
        GameObject[] pieces = GetAllPieces(player);
        foreach (GameObject piece in pieces)
        {
            if (piece != null && piece.name.Contains("king"))
                return piece;
        }
        return null;
    }

    private GameObject[] GetAllPieces(string player)
    {
        if (player == "white")
            return playerWhite.Where(p => p != null).ToArray();
        else
            return playerBlack.Where(p => p != null).ToArray();
    }

    private List<Vector2Int> GetPieceMoves(GameObject piece)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Chessman cm = piece.GetComponent<Chessman>();

        // Check piece type
        if (piece.name.Contains("pawn"))
        {
            // Pawn moves
            int direction = cm.GetPlayer() == "white" ? 1 : -1;
            int x = cm.GetXBoard();
            int y = cm.GetYBoard();

            // Forward move
            if (GetPosition(x, y + direction) == null)
            {
                moves.Add(new Vector2Int(x, y + direction));
                // First move can be 2 squares
                if (((cm.GetPlayer() == "white" && y == 1) ||
                     (cm.GetPlayer() == "black" && y == 6)) &&
                    GetPosition(x, y + 2 * direction) == null)
                {
                    moves.Add(new Vector2Int(x, y + 2 * direction));
                }
            }

            // Diagonal captures
            for (int i = -1; i <= 1; i += 2)
            {
                if (PositionOnBoard(x + i, y + direction))
                {
                    GameObject target = GetPosition(x + i, y + direction);
                    if (target != null && target.GetComponent<Chessman>().GetPlayer() != cm.GetPlayer())
                    {
                        moves.Add(new Vector2Int(x + i, y + direction));
                    }
                }
            }
        }
        else
        {
            // For all other pieces, check all board positions
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (IsMoveLegal(piece, x, y))
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return moves;
    }

    private bool IsSquareUnderAttack(string defendingPlayer, int x, int y)
    {
        string attackingPlayer = defendingPlayer == "white" ? "black" : "white";
        GameObject[] attackingPieces = GetAllPieces(attackingPlayer);

        foreach (GameObject piece in attackingPieces)
        {
            if (piece == null) continue;
            Chessman cm = piece.GetComponent<Chessman>();
            // Ignore pieces that are not present on the board (i.e. captured)
            if (GetPosition(cm.GetXBoard(), cm.GetYBoard()) != piece)
                continue;

            if (CanPieceAttackSquare(piece, x, y))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPathClear(int startX, int startY, int endX, int endY)
    {
        int dx = endX - startX;
        int dy = endY - startY;

        // Diagonal movement
        if (Mathf.Abs(dx) == Mathf.Abs(dy))
        {
            int xStep = dx > 0 ? 1 : -1;
            int yStep = dy > 0 ? 1 : -1;

            for (int i = 1; i < Mathf.Abs(dx); i++)
            {
                if (GetPosition(startX + i * xStep, startY + i * yStep) != null)
                    return false;
            }
            return true;
        }
        // Horizontal movement
        else if (dy == 0)
        {
            int xStep = dx > 0 ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dx); i++)
            {
                if (GetPosition(startX + i * xStep, startY) != null)
                    return false;
            }
            return true;
        }
        // Vertical movement
        else if (dx == 0)
        {
            int yStep = dy > 0 ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dy); i++)
            {
                if (GetPosition(startX, startY + i * yStep) != null)
                    return false;
            }
            return true;
        }

        return false;
    }

    public bool WouldPutKingInCheck(GameObject piece, int targetX, int targetY)
    {
        Chessman cm = piece.GetComponent<Chessman>();
        string player = cm.GetPlayer();

        // Store original position
        int originalX = cm.GetXBoard();
        int originalY = cm.GetYBoard();
        GameObject capturedPiece = GetPosition(targetX, targetY);

        // Simulate move
        SetPositionEmpty(originalX, originalY);
        if (capturedPiece != null)
            SetPositionEmpty(targetX, targetY);

        cm.SetXBoard(targetX);
        cm.SetYBoard(targetY);
        SetPosition(piece);

        // Check if this move would put own king in check
        bool wouldBeInCheck = IsInCheck(player);

        // Restore position
        SetPositionEmpty(targetX, targetY);
        cm.SetXBoard(originalX);
        cm.SetYBoard(originalY);
        SetPosition(piece);
        if (capturedPiece != null)
            SetPosition(capturedPiece);

        return wouldBeInCheck;
    }
    #endregion
    #region Check and Checkmate Logic
    public bool IsInCheck(string player)
    {
        GameObject king = FindKing(player);
        if (king == null) return false;

        Chessman kingPiece = king.GetComponent<Chessman>();
        return IsSquareUnderAttack(player, kingPiece.GetXBoard(), kingPiece.GetYBoard());
    }

    public List<GameObject> GetAttackingPieces(string player)
    {
        List<GameObject> attackers = new List<GameObject>();
        GameObject king = FindKing(player);
        if (king == null) return attackers;

        Chessman kingPiece = king.GetComponent<Chessman>();
        string opposingPlayer = (player == "white") ? "black" : "white";
        GameObject[] opposingPieces = GetAllPieces(opposingPlayer);

        foreach (GameObject piece in opposingPieces)
        {
            if (piece == null) continue;
            Chessman cm = piece.GetComponent<Chessman>();
            // Ignore pieces that are not present on the board (i.e. captured)
            if (GetPosition(cm.GetXBoard(), cm.GetYBoard()) != piece)
                continue;

            if (CanPieceAttackSquare(piece, kingPiece.GetXBoard(), kingPiece.GetYBoard()))
            {
                attackers.Add(piece);
            }
        }

        return attackers;
    }

    public bool IsCheckmate(string player)
    {
        // If not in check, can't be checkmate
        if (!IsInCheck(player)) return false;

        GameObject king = FindKing(player);
        if (king == null) return true; // No king means checkmate

        // Debug the situation
        Debug.Log($"Checking if {player} is in checkmate");

        // 1. Can the king move to a safe square?
        List<Vector2Int> safeMoves = GetKingSafeMoves(king);
        if (safeMoves.Count > 0)
        {
            Debug.Log($"King can escape to {safeMoves.Count} safe squares");
            return false; // King can move to safety
        }

        // 2. Get attacking pieces
        List<GameObject> attackers = GetAttackingPieces(player);
        Debug.Log($"King is attacked by {attackers.Count} pieces");

        // If attacked by more than one piece, king must move (which we already checked)
        if (attackers.Count > 1)
        {
            return true; // Can't block or capture multiple attackers
        }

        // 3. Can we capture the attacking piece?
        GameObject attacker = attackers[0];
        Chessman attackerPiece = attacker.GetComponent<Chessman>();
        int attackerX = attackerPiece.GetXBoard();
        int attackerY = attackerPiece.GetYBoard();

        // Check if any friendly piece can capture the attacker
        GameObject[] friendlyPieces = GetAllPieces(player);
        foreach (GameObject piece in friendlyPieces)
        {
            if (piece == null || piece.name.Contains("king")) continue;

            Chessman cm = piece.GetComponent<Chessman>();
            if (GetPosition(cm.GetXBoard(), cm.GetYBoard()) != piece)
            {
                Debug.Log($"Skipping captured piece {cm.name} that claims to be at ({cm.GetXBoard()}, {cm.GetYBoard()})");
                continue;
            }
            if (IsMoveLegal(piece, attackerX, attackerY) && WouldMovePreventCheck(piece, attackerX, attackerY, player))
            {
                Debug.Log($"{cm.name} can capture the attacker");
                return false; // Can capture the attacking piece
            }
        }

        // 4. Can we block the attack? (only applicable for bishops, rooks, queens)
        if (!attacker.name.Contains("knight") && !attacker.name.Contains("pawn"))
        {
            Chessman kingPiece = king.GetComponent<Chessman>();
            List<Vector2Int> blockingSquares = GetSquaresBetween(
                attackerPiece.GetXBoard(), attackerPiece.GetYBoard(),
                kingPiece.GetXBoard(), kingPiece.GetYBoard());
            foreach (GameObject piece in friendlyPieces)
            {
                if (piece == null || piece.name.Contains("king")) continue;
                Chessman cm = piece.GetComponent<Chessman>();
                if (GetPosition(cm.GetXBoard(), cm.GetYBoard()) != piece)
                {
                    Debug.Log($"Skipping captured piece {cm.name} that claims to be at ({cm.GetXBoard()}, {cm.GetYBoard()})");
                    continue;
                }
                foreach (Vector2Int square in blockingSquares)
                {
                    if (CanPieceBlockAttack(piece, square))
                    {
                        Debug.Log($"{piece.name} can block the attack at {square.x},{square.y}");
                        return false; // Can block the attack
                    }
                }
            }
        }

        Debug.Log($"{player} is in checkmate");
        return true;
    }

    public List<Vector2Int> GetKingSafeMoves(GameObject king)
    {
        List<Vector2Int> safeMoves = new List<Vector2Int>();
        Chessman kingPiece = king.GetComponent<Chessman>();
        string player = kingPiece.GetPlayer();

        // Check all adjacent squares (including castling moves)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Check if this is a legal king move 
                if (IsMoveLegal(king, x, y))
                {
                    // Temporarily move king to check if position is safe
                    int originalX = kingPiece.GetXBoard();
                    int originalY = kingPiece.GetYBoard();
                    GameObject capturedPiece = GetPosition(x, y);

                    // Simulate move
                    SetPositionEmpty(originalX, originalY);
                    if (capturedPiece != null)
                        SetPositionEmpty(x, y);

                    kingPiece.SetXBoard(x);
                    kingPiece.SetYBoard(y);
                    SetPosition(king);

                    // Check if king would still be under attack
                    bool stillUnderAttack = IsSquareUnderAttack(player, x, y);

                    // Restore position
                    SetPositionEmpty(x, y);
                    kingPiece.SetXBoard(originalX);
                    kingPiece.SetYBoard(originalY);
                    SetPosition(king);
                    if (capturedPiece != null)
                        SetPosition(capturedPiece);

                    if (!stillUnderAttack)
                    {
                        safeMoves.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return safeMoves;
    }

    private List<Vector2Int> GetSquaresBetween(int x1, int y1, int x2, int y2)
    {
        List<Vector2Int> squares = new List<Vector2Int>();

        int dx = x2 - x1;
        int dy = y2 - y1;

        // Only calculate intermediate squares for non-knight attacks
        if (Mathf.Abs(dx) == Mathf.Abs(dy)) // Diagonal
        {
            int xStep = dx > 0 ? 1 : -1;
            int yStep = dy > 0 ? 1 : -1;

            for (int i = 1; i < Mathf.Abs(dx); i++)
            {
                squares.Add(new Vector2Int(x1 + i * xStep, y1 + i * yStep));
            }
        }
        else if (dx == 0) // Vertical
        {
            int yStep = dy > 0 ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dy); i++)
            {
                squares.Add(new Vector2Int(x1, y1 + i * yStep));
            }
        }
        else if (dy == 0) // Horizontal
        {
            int xStep = dx > 0 ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dx); i++)
            {
                squares.Add(new Vector2Int(x1 + i * xStep, y1));
            }
        }

        return squares;
    }
    #endregion

    public struct Move
    {
        public int targetX;
        public int targetY;

        public Move(int x, int y)
        {
            targetX = x;
            targetY = y;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerWhite = new GameObject[]{ //white pieces
            CreateChessPiece(0, 0, "white_rook"),
            CreateChessPiece(1, 0, "white_knight"),
            CreateChessPiece(2, 0, "white_bishop"),
            CreateChessPiece(3, 0, "white_queen"),
            CreateChessPiece(4, 0, "white_king"),
            CreateChessPiece(5, 0, "white_bishop"),
            CreateChessPiece(6, 0, "white_knight"),
            CreateChessPiece(7, 0, "white_rook"),
            CreateChessPiece(0, 1, "white_pawn"),
            CreateChessPiece(1, 1, "white_pawn"),
            CreateChessPiece(2, 1, "white_pawn"),
            CreateChessPiece(3, 1, "white_pawn"),
            CreateChessPiece(4, 1, "white_pawn"),
            CreateChessPiece(5, 1, "white_pawn"),
            CreateChessPiece(6, 1, "white_pawn"),
            CreateChessPiece(7, 1, "white_pawn")
        };
        playerBlack = new GameObject[]{
            CreateChessPiece(0, 7, "black_rook"),
            CreateChessPiece(1, 7, "black_knight"),
            CreateChessPiece(2, 7, "black_bishop"),
            CreateChessPiece(3, 7, "black_queen"),
            CreateChessPiece(4, 7, "black_king"),
            CreateChessPiece(5, 7, "black_bishop"),
            CreateChessPiece(6, 7, "black_knight"),
            CreateChessPiece(7, 7, "black_rook"),
            CreateChessPiece(0, 6, "black_pawn"),
            CreateChessPiece(1, 6, "black_pawn"),
            CreateChessPiece(2, 6, "black_pawn"),
            CreateChessPiece(3, 6, "black_pawn"),
            CreateChessPiece(4, 6, "black_pawn"),
            CreateChessPiece(5, 6, "black_pawn"),
            CreateChessPiece(6, 6, "black_pawn"),
            CreateChessPiece(7, 6, "black_pawn")
        };
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
        clockSystem = FindObjectOfType<ClockSystem>();

        // Start initial pause coroutine
        StartCoroutine(InitialPauseCoroutine());
    }

    // Add a coroutine for initial pause
    private IEnumerator InitialPauseCoroutine()
    {
        initialPause = true;
        Debug.Log("Game starting in 3 seconds...");

        // Display a countdown message
        // You could add a UI element for this if desired

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        initialPause = false;
        Debug.Log("Game started!");
    }

    public GameObject CreateChessPiece(int x, int y, string piece)
    {
        GameObject go = Instantiate(chessPiece, new Vector3(x, y, -1), Quaternion.identity) as GameObject;
        Chessman cm = go.GetComponent<Chessman>();
        cm.name = piece;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return go;
    }
    public void SetPosition(GameObject go)
    {
        Chessman cm = go.GetComponent<Chessman>();
        chessBoard[cm.GetXBoard(), cm.GetYBoard()] = go;
    }
    public void SetPositionEmpty(int x, int y)
    {
        chessBoard[x, y] = null;
    }
    public GameObject GetPosition(int x, int y)
    {
        return chessBoard[x, y];
    }
    public bool PositionOnBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
    public GameObject GetChessman(int x, int y)
    {
        if (PositionOnBoard(x, y))
        {
            return chessBoard[x, y];
        }
        return null;
    }

    public GameObject MoveChessman(GameObject go, int x, int y)
    {
        GameObject capturedPiece = GetPosition(x, y);
        Chessman cm = go.GetComponent<Chessman>();

        // Make the move
        SetPositionEmpty(cm.GetXBoard(), cm.GetYBoard());
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.SetCoords();
        SetPosition(go);

        // Check for checkmate on opposite player
        string oppositePlayer = (cm.GetPlayer() == "white") ? "black" : "white";
        if (IsInCheck(oppositePlayer))
        {
            Debug.Log($"{oppositePlayer} is in check!");
            if (IsCheckmate(oppositePlayer))
            {
                Debug.Log($"{oppositePlayer} is in checkmate! Game over!");
                gameOver = true;
                WinCanvas.gameObject.SetActive(true);
                PlayerCanvas.gameObject.SetActive(false);
                if (audioManager != null)
                {
                    audioManager.PlayWinSound();
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWinSound();
                }
                else
                {
                    Debug.LogWarning("No AudioManager found to play win sound");
                }
            }
        }

        return capturedPiece;
    }

    public void CreateMovePlate(int x, int y) // Changed method name to PascalCase and added parameters
    {
        // Don't allow move plates during initial pause
        if (initialPause || movePlate == null)
        {
            return;
        }

        GameObject go = Instantiate(movePlate, new Vector3(x, y, -1), Quaternion.identity);
        MovePlate mp = go.GetComponent<MovePlate>();

        if (mp == null)
        {
            Destroy(go);
            return;
        }

        mp.SetReference(this.gameObject);
        mp.SetCoords(x, y);
    }

    public string GetplayerTurn()
    {
        return playerTurn;
    }
    public bool isGameOver()
    {
        return gameOver;
    }
    public void SwitchPlayerTurn()
    {
        if (playerTurn == "white")
        {
            playerTurn = "black";
        }
        else
        {
            playerTurn = "white";
            moveCounter++; // Increment move counter after both players have moved
        }

        // Switch the timers when turns change
        if (clockSystem != null)
        {
            clockSystem.SwitchTimers();
        }
    }

    public void EndGameOnTime(string losingPlayer)
    {
        gameOver = true;
        string winner = losingPlayer == "white" ? "Black" : "White";
        WinCanvas.gameObject.SetActive(true);
        audioManager.PlayWinSound();
    }

    public void Update()
    {
        if (gameOver)
        {
            WinCanvas.gameObject.SetActive(true);
            PlayerCanvas.gameObject.SetActive(false);

            // Handle restart input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            return; // Exit early if game is already over
        }

        // Only allow space key restart when not in initial pause
        if (!initialPause && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    private void RestartGame()
    {
        // Clean up current game state
        StopAllCoroutines();
        if (clockSystem != null)
        {
            clockSystem.StopAllTimers(); // Make sure to add this public method to ClockSystem
        }
        if (AudioManager.Instance != null)
        {
            // Instead of destroying audio, just reset its state
            AudioManager.Instance.ResetAudioState();
            AudioManager.Instance.PlayButtonClickSound();
        }
        // Reset game state
        gameOver = false;
        moveCounter = 0;
        playerTurn = "white";

        // Reload the scene
        StartCoroutine(DelayedSceneReload());
    }
    private IEnumerator DelayedSceneReload()
    {
        // Short delay to ensure sound effects have time to play
        yield return new WaitForSeconds(0.1f);

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public bool GetInitialPause()
    {
        return initialPause;
    }
}
