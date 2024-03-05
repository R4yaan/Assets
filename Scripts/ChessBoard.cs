using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    // Graphics variables
    [SerializeField] public Material tileMat;
    [SerializeField] private int tileSize = 10;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private float draggingOffset = 0.8f;

    //Prefabs and colours
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamColour;

    // Tile highlight variables
    private Camera currentCamera;
    private Vector2Int currentlyHovering;
    private Vector3 bounds;

    // FEN notation of board (starting position)
    private string[] FEN;
    //private string[] FEN = {"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-", "0 1"};

    // All active chess pieces
    private Pieces[,] chessPieces;

    // Board as 2D array of each tile
    private GameObject[,] board;

    // Boolean storing if it is white's turn
    private bool whiteTurn = true;
    private bool isInCheck = false;
    public GameObject whiteCamera;
    public GameObject blackCamera;

    // Piece that is currently selected to be moved
    private Pieces selectedPiece;

    // List of available moves for a piece at a point in time
    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    // En passant pawn X position
    public static int enPassantX = -1;
    // Castling availability
    public bool whiteKingCastle = true;
    public bool whiteQueenCastle = true;
    public bool blackKingCastle = true;
    public bool blackQueenCastle = true;

    // 75 move rule
    int halfmoveClock = 0;
    // Full moves made
    int fullmoveClock = 0;

    private void Awake()
    {
        whiteCamera.SetActive(true);
        blackCamera.SetActive(false);

        // Initialize the chessboard
        CreateBoard(tileSize);

        if (FEN != null)
        {
            InterpretFEN(FEN);
        }
        else
        {
            SpawnAllPieces();
        }
        PositionAllPieces();
    }

    private void Update()
    {
        // Ensures there's a valid camera
        if (!currentCamera)
        {
            // Sets current camera to main
            currentCamera = Camera.main;
            return;
        }

        

        // Casts a ray to the tiles based on mouse position
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 1000, LayerMask.GetMask("Tile", "Hover", "possibleMove")))
        {
            // Gets coordinates of the tile hit by ray (under the mouse)
            Vector2Int hitPos = LookupTileIndex(info.transform.gameObject);

            // Highlight the hovered tile and update highlighted when the mouse moves
            if (currentlyHovering == -Vector2Int.one)
            {
                // If not hovering over any tile, currentlyHovering set to hitPos
                currentlyHovering = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentlyHovering != hitPos)
            {
                // If the mouse moves to hover over a different tile, update the highlighted tile
                board[currentlyHovering.x, currentlyHovering.y].layer = ContainsValidMove(ref availableMoves, currentlyHovering) ? LayerMask.NameToLayer("possibleMove") : LayerMask.NameToLayer("Tile");
                currentlyHovering = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (isCheckmate())
            {
                Debug.Log("white wins: " + whiteTurn);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPos.x, hitPos.y] != null)
                {
                    selectedPiece = chessPieces[hitPos.x, hitPos.y];
                    if ((selectedPiece.team == 0 && !whiteTurn) || (selectedPiece.team == 1 && whiteTurn))
                    {
                        selectedPiece = null;
                    }
                    else
                    {
                        availableMoves = selectedPiece.GetAvailableMoves(ref chessPieces);
                        if (availableMoves.Count > 0)
                        {
                            List<Vector2Int> movesToRemove = new List<Vector2Int>();
                            foreach (Vector2Int move in availableMoves)
                            {
                                if (!MoveTo(selectedPiece, move.x, move.y, false))
                                {
                                    movesToRemove.Add(move);
                                }
                            }

                            foreach (Vector2Int illegalMove in movesToRemove)
                            {
                                availableMoves.Remove(illegalMove);
                            }
                        }


                        HighlightTiles();
                    }
                }
            }
            if (selectedPiece != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPos = new Vector2Int(selectedPiece.xPos, selectedPiece.yPos);
                bool legalMove = MoveTo(selectedPiece, hitPos.x, hitPos.y, true);
                if (!legalMove)
                {
                    selectedPiece.setPos(TileCenter(previousPos.x, previousPos.y));
                }
                else
                {
                    whiteTurn = !whiteTurn;
                    whiteCamera.SetActive(!whiteCamera.activeSelf);
                    blackCamera.SetActive(!blackCamera.activeSelf);
                    currentCamera = Camera.main;
                    RemoveHighlightTiles();
                    foreach (Pieces piece in chessPieces)
                    {
                        if (piece != null && ((piece.team == 0) == whiteTurn) && piece.type == ChessPieceType.King)
                        {
                            selectedPiece = piece;
                            availableMoves = selectedPiece.GetAvailableMoves(ref chessPieces);
                            if (availableMoves.Count > 0)
                            {
                                List<Vector2Int> movesToRemove = new List<Vector2Int>();
                                foreach (Vector2Int move in availableMoves)
                                {
                                    if (!MoveTo(selectedPiece, move.x, move.y, false))
                                    {
                                        movesToRemove.Add(move);
                                    }
                                }

                                foreach (Vector2Int illegalMove in movesToRemove)
                                {
                                    availableMoves.Remove(illegalMove);
                                }
                            }

                            break;
                        }
                    }
                }
                selectedPiece = null;
                RemoveHighlightTiles();
            }

        }
        else
        {
            // Reset tile highlighting when not hovering over any tile
            if (currentlyHovering != -Vector2Int.one)
            {
                board[currentlyHovering.x, currentlyHovering.y].layer = ContainsValidMove(ref availableMoves, currentlyHovering) ? LayerMask.NameToLayer("possibleMove") : LayerMask.NameToLayer("Tile");
                currentlyHovering = -Vector2Int.one;
            }
        }

        if (selectedPiece)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                selectedPiece.setPos(ray.GetPoint(distance) + Vector3.up * draggingOffset);
            }
        }
    }


    private bool MoveTo(Pieces selPiece, int x, int y, bool makeChanges)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2(x, y)))
        {
            return false;
        }

        if (makeChanges)
        {
            enPassantX = -1;
        }

        Vector2Int previousPos = new Vector2Int(selPiece.xPos, selPiece.yPos);
        int teamInCheck = simCheckMove(selPiece, x, y, previousPos);

        if (teamInCheck == selPiece.team)
        {
            isInCheck = true;
            return false;
        }
        
        if (chessPieces[x, y] != null)
        {
            Pieces targetPiece = chessPieces[x, y];

            if (selPiece.team == targetPiece.team)
            {
                return false;
            }
            else if (makeChanges)
            {
                Destroy(chessPieces[x, y].gameObject);
                chessPieces[x, y] = null;
            }
        }
        else if (makeChanges)
        {
            if (selPiece.type == ChessPieceType.Pawn && Math.Abs(y - selPiece.yPos) == 1 && Math.Abs(x - selPiece.xPos) == 1)
            {
                Destroy(chessPieces[x, (selPiece.team == 0) ? (y - 1) : (y + 1)].gameObject);
                chessPieces[x, (selPiece.team == 0) ? (y - 1) : (y + 1)] = null;
            }
        }

        teamInCheck = simCheckMove(selPiece, x, y, previousPos);
        if (makeChanges)
        {
            if (teamInCheck == -1)
            {
                isInCheck = false;
            }
            else
            {
                isInCheck = true;
            }
        }

        if (makeChanges)
        {
            chessPieces[x, y] = selPiece;
            chessPieces[previousPos.x, previousPos.y] = null;

            if (selPiece.type == ChessPieceType.Pawn && Math.Abs(y - selPiece.yPos) == 2)
            {
                enPassantX = x;
            }
        
            PositionSinglePiece(x, y);
        }

        return true;
    }


    private int simCheckMove(Pieces selPiece, int targetX, int targetY, Vector2Int previousPos)
    {
        Pieces[,] chessPiecesCopy = new Pieces[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessPieces[i, j] != null)
                {
                    // Copy attributes from the original piece to the clone
                    chessPiecesCopy[i, j] = Instantiate(chessPieces[i, j]).GetComponent<Pieces>();
                    chessPiecesCopy[i, j].CopyAttributes(chessPieces[i, j]);
                }
            }
        }

        // Place the selected piece and remove it from its previous position
        chessPiecesCopy[targetX, targetY] = selPiece;
        chessPiecesCopy[previousPos.x, previousPos.y] = null;

        // Check for possible moves and if the opponent's king is in check
        foreach (Pieces currentSimPiece in chessPiecesCopy)
        {
            if (currentSimPiece != null)
            {
                List<Vector2Int> availableMovesCopy = currentSimPiece.GetAvailableMoves(ref chessPiecesCopy);

                foreach (Vector2Int move in availableMovesCopy)
                {
                    Pieces targetPiece = chessPiecesCopy[move.x, move.y];
                    if (targetPiece != null && targetPiece.type == ChessPieceType.King && targetPiece.team != currentSimPiece.team)
                    {
                        return targetPiece.team; // Return the team of the king in check
                    }
                }
            }
        }

        return -1; // Return -1 if no king is in check
    }


    private bool isCheckmate()
    {
        if (!isInCheck)
        {
            return false;
        }

        int team = whiteTurn ? 0 : 1;

        foreach (Pieces currentPiece in chessPieces)
        {
            if (currentPiece != null && currentPiece.team == team)
            {
                List<Vector2Int> availableMovesCopy = currentPiece.GetAvailableMoves(ref chessPieces);

                foreach (Vector2Int move in availableMovesCopy)
                {
                    if (simCheckMove(currentPiece, move.x, move.y, new Vector2Int(currentPiece.xPos, currentPiece.yPos)) != currentPiece.team)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }


    // Generate the board with individual tiles
    private void CreateBoard(float boardSize)
    {
        // Adjust yOffset based on the object's position to align with board assets used
        yOffset += transform.position.y;

        // Initialize the 8x8 board
        board = new GameObject[8, 8];

        // Create individual tiles for the chessboard
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Generate each individual tile for each x,y coordinate on board
                board[x, y] = CreateTile(boardSize, x, y);
            }
        }

        // Calculate the bounds dynamically based on the actual board size
        bounds = new Vector3(4 * boardSize, 0, 4 * boardSize);
    }

    // Create an individual tile
    private GameObject CreateTile(float boardSize, int x, int y)
    {
        // Create tile and name it by its x,y coordinate on board
        GameObject tileObj = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObj.transform.parent = transform; // Link tile to the whole board

        // Render tile mesh
        Mesh mesh = new Mesh();
        tileObj.AddComponent<MeshFilter>().mesh = mesh;
        tileObj.AddComponent<MeshRenderer>().material = tileMat; // Set tile material

        // Set tile's 4 corners
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * boardSize, yOffset, y * boardSize) - bounds;
        vertices[1] = new Vector3(x * boardSize, yOffset, (y + 1) * boardSize) - bounds;
        vertices[2] = new Vector3((x + 1) * boardSize, yOffset, y * boardSize) - bounds;
        vertices[3] = new Vector3((x + 1) * boardSize, yOffset, (y + 1) * boardSize) - bounds;

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        tileObj.layer = LayerMask.NameToLayer("Tile");
        tileObj.AddComponent<BoxCollider>(); // Give tile collider

        return tileObj;
    }

    // Spawn all pieces by calling SpawnSinglePiece() for each chess piece
    private void SpawnAllPieces()
    {
        // 8x8 2D array of Pieces objects
        chessPieces = new Pieces[8, 8];

        int white = 0;
        int black = 1;

        // Spawning all white pieces in the starting position
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, white);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, white);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        // Spawn white pawns
        for (int i = 0; i < 8; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, white);
        }

        // Spawning all black pieces in the starting position
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, black);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, black);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        // Spawn black pawns
        for (int i = 0; i < 8; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, black);
        }
    }

    // Spawn a single piece
    private Pieces SpawnSinglePiece(ChessPieceType type, int team)
    {
        Pieces cPiece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Pieces>(); // Spawn a piece

        cPiece.type = type;
        cPiece.team = team;
        cPiece.GetComponent<MeshRenderer>().material = teamColour[team];
        //Sets piece team and type

        return cPiece;
    }

    // Setting piece positions
    private void PositionAllPieces()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y)
    {
        chessPieces[x, y].xPos = x;
        chessPieces[x, y].yPos = y;
        chessPieces[x, y].setPos(TileCenter(x, y));
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("possibleMove");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        foreach (Vector2Int move in moves)
        {
            if (move.x == pos.x && move.y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    // Gets the center of a tile
    private Vector3 TileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Lookup 2D index of a tile in the board array
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == hitInfo)
                {
                    // Return the hit tile
                    return new Vector2Int(x, y);
                }
            }
        }
        // Return an invalid index if not found (unused in practice)
        return new Vector2Int(-1, -1);
    }

    private Pieces[,] InterpretFEN(string[] FEN)
    {
        Pieces[,] pieces = new Pieces[8, 8];

        int rank = 7;
        int file = 0;

        int white = 0;
        int black = 1;
        var pieceCodes = new Dictionary<char, Pieces>()
        {
            {'r', SpawnSinglePiece(ChessPieceType.Rook, black)},
            {'n', SpawnSinglePiece(ChessPieceType.Knight, black)},
            {'b', SpawnSinglePiece(ChessPieceType.Bishop, black)},
            {'q', SpawnSinglePiece(ChessPieceType.Queen, black)},
            {'k', SpawnSinglePiece(ChessPieceType.King, black)},
            {'p', SpawnSinglePiece(ChessPieceType.Pawn, black)},
            {'R', SpawnSinglePiece(ChessPieceType.Rook, white)},
            {'N', SpawnSinglePiece(ChessPieceType.Knight, white)},
            {'B', SpawnSinglePiece(ChessPieceType.Bishop, white)},
            {'Q', SpawnSinglePiece(ChessPieceType.Queen, white)},
            {'K', SpawnSinglePiece(ChessPieceType.King, white)},
            {'P', SpawnSinglePiece(ChessPieceType.Pawn, white)}
        };
        for (int i = 0; i < FEN[0].Length; i++)
        {
            char character = FEN[0][i];
            if (character == '/')
            {
                rank--;
                file = 0;
            }
            else if (char.IsDigit(character))
            {
                file += int.Parse(character.ToString());
            }
            else
            {
                pieces[file, rank] = pieceCodes[character];
                file++;
            }
        }


        whiteTurn = FEN[1] == "w";

        whiteKingCastle = FEN[2].Contains("K");
        whiteQueenCastle = FEN[2].Contains("Q");
        blackKingCastle = FEN[2].Contains("k");
        blackQueenCastle = FEN[2].Contains("q");


        char enPassant = FEN[3][0];
        enPassantX = char.ToUpper(enPassant) - 65;


        halfmoveClock = int.Parse(FEN[4]);
        fullmoveClock = int.Parse(FEN[5]);

        return pieces;
    }
}