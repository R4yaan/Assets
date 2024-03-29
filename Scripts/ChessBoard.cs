using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChessBoard : MonoBehaviour
{
    //Graphics variables
    [SerializeField] public Material tileMat;
    [SerializeField] private int tileSize = 10;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private float draggingOffset = 0.8f;

    //Prefabs and colours
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamColour;

    //Tile highlight variables
    private Camera currentCamera;
    private Vector2Int currentlyHovering;
    private Vector3 bounds;

    //FEN notation of board (starting position)
    [SerializeField] private string FEN;

    //All active chess pieces
    private Pieces[,] chessPieces;

    //Board as 2D array of each tile
    private GameObject[,] board;

    //Boolean storing if it is white's turn
    public static bool whiteTurn = true;

    //Boolean storing if the current team is in check
    private bool isInCheck = false;

    //Boolean storing if white has won in horde
    private bool hordeWin = true;

    //Variables to store both camera's game objects and a text box
    public GameObject whiteCamera;
    public GameObject blackCamera;
    public TextMeshProUGUI textBox;

    //Piece that is currently selected to be moved
    private Pieces selectedPiece;

    //List of available moves for a piece at a point in time
    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    //En passant pawn X position
    public static int enPassantX = -1;
    //Castling availability
    public static bool whiteKingCastle = true;
    public static bool whiteQueenCastle = true;
    public static bool blackKingCastle = true;
    public static bool blackQueenCastle = true;

    //75 move rule
    int halfmoveClock = 0;
    //Full moves made
    int fullmoveClock = 0;

    //Run when program starts
    private void Awake()
    {
        //Sets up tutorial board (empty)
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            FEN = "8/8/8/8/8/8/8/8 w - - 0 1";
        }
        //Starts horde variant if in horde scene
        else if (SceneManager.GetActiveScene().name == "Horde")
        {
            FEN = "rnbqkbnr/pppppppp/8/1PP2PP1/PPPPPPPP/PPPPPPPP/PPPPPPPP/PPPPPPPP w KQkq - 0 1";
        }
        
        //Sets white side camera to active for game start
        whiteCamera.SetActive(true);
        blackCamera.SetActive(false);

        //Initialize the chessboard
        CreateBoard(tileSize);

        //Check if FEN was provided
        if (FEN.Length > 1)
        {
            chessPieces = InterpretFEN(FEN);
        }
        else
        {
            //Defualt chess board set up
            SpawnAllPieces();
        }
        //Visually move all pieces to their correct positions
        PositionAllPieces();

        //Starts tutorial sequence if in tutorial scene
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            StartCoroutine(Tutorial());
        }
    }
    
    //Run every frame
    private void Update()
    {
        //Ensures there's a valid camera
        if (!currentCamera)
        {
            //Sets current camera to main
            currentCamera = Camera.main;
            return;
        }

        

        //Casts a ray to the tiles based on mouse position
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 1000, LayerMask.GetMask("Tile", "Hover", "possibleMove")))
        {
            //Gets coordinates of the tile hit by ray (under the mouse)
            Vector2Int hitPos = LookupTileIndex(info.transform.gameObject);

            //Highlight the hovered tile and update highlighted when the mouse moves
            if (currentlyHovering == -Vector2Int.one)
            {
                //If not hovering over any tile, currentlyHovering set to hitPos
                currentlyHovering = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentlyHovering != hitPos)
            {
                //If the mouse moves to hover over a different tile, update the highlighted tile
                board[currentlyHovering.x, currentlyHovering.y].layer = ContainsValidMove(ref availableMoves, currentlyHovering) ?
                                                                                        LayerMask.NameToLayer("possibleMove") :
                                                                                        LayerMask.NameToLayer("Tile");
                currentlyHovering = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            //Checks for win condition
            if (isCheckmate())
            {
                SceneManager.LoadScene("Victory");
            }

            //Checks for black win condition in horde variant
            if (SceneManager.GetActiveScene().name == "Horde")
            {
                foreach (Pieces piece in chessPieces)
                {
                    if (piece && piece.team == 0)
                    {
                        //The horde win condition is not met if there is a white piece on the board
                        hordeWin = false;
                    }
                }
                //If there were no white pieces then the win condition was met
                if (hordeWin)
                {
                    SceneManager.LoadScene("Victory");
                }
                //Reset the win condition variable for the next check
                hordeWin = true;
            }

            //If clicking, check for pieces getting clicked on
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPos.x, hitPos.y] != null)
                {
                    selectedPiece = chessPieces[hitPos.x, hitPos.y]; //Get piece that was clicked on
                    if ((selectedPiece.team == 0 && !whiteTurn) || (selectedPiece.team == 1 && whiteTurn)) 
                    {
                        //Make sure only pieces of that player's colour can be chosen
                        selectedPiece = null;
                    }
                    else
                    {
                        availableMoves = selectedPiece.GetAvailableMoves(ref chessPieces); //Get all available moves for that piece
                        if (availableMoves.Count > 0)
                        {
                            //Remove illegal moves (if it leads to checkto themselves)
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

                        //Run function to highlight possible move tiles
                        HighlightTiles();
                    }
                }
            }
            //Placing piece down
            if (selectedPiece != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPos = new Vector2Int(selectedPiece.xPos, selectedPiece.yPos);
                //Checks if the move trying to be made is legal
                bool legalMove = MoveTo(selectedPiece, hitPos.x, hitPos.y, true);
                if (!legalMove)
                {
                    //Move it back to its previous position if illegal move
                    selectedPiece.setPos(TileCenter(previousPos.x, previousPos.y)); 
                }
                else
                {
                    whiteTurn = !whiteTurn; //Changes turn
                    whiteCamera.SetActive(!whiteCamera.activeSelf); //Changes cameras
                    blackCamera.SetActive(!blackCamera.activeSelf);
                    currentCamera = Camera.main;
                    RemoveHighlightTiles(); //Unhighlight tiles
                    //Change castling availability after king is moved
                    if (selectedPiece.type == ChessPieceType.King) 
                    {
                        //Once king moves then that team cannot castle either side
                        if (selectedPiece.team == 0)
                        {
                            whiteKingCastle = false;
                            whiteQueenCastle = false;
                        }
                        else
                        {
                            blackKingCastle = false;
                            blackQueenCastle = false;
                        }
                    }
                    //Change castling availability if rook is moved
                    else if (selectedPiece.type == ChessPieceType.Rook) 
                    {
                        //If statements to change the correct castling side for the correct team
                        if (selectedPiece.team == 0) 
                        {
                            if (previousPos.x == 0)
                            {
                                whiteQueenCastle = false;
                            }
                            else if (selectedPiece.team == 7)
                            {
                                whiteKingCastle = false;
                            }
                        }
                        else
                        {
                            if (previousPos.x == 0)
                            {
                                blackQueenCastle = false;
                            }
                            else if (selectedPiece.team == 7)
                            {
                                whiteKingCastle = false;
                            }
                        }
                    }

                    //Pawn promotion
                    if (selectedPiece.type == ChessPieceType.Pawn)
                    {
                        if (hitPos.y == 7 && selectedPiece.team == 0) //Check if pawn moved to end of board
                        {
                            //Destroy pawn and change to queen
                            Destroy(chessPieces[hitPos.x, hitPos.y].gameObject);
                            chessPieces[hitPos.x, hitPos.y] = SpawnSinglePiece(ChessPieceType.Queen, 0); 
                        }
                        else if (hitPos.y == 1 && selectedPiece.team == 1)
                        {
                            Destroy(chessPieces[hitPos.x, hitPos.y].gameObject);
                            chessPieces[hitPos.x, hitPos.y] = SpawnSinglePiece(ChessPieceType.Queen, 1);
                        }
                        PositionSinglePiece(hitPos.x, hitPos.y); //Position the queen in the place of the pawn
                    }

                    foreach (Pieces piece in chessPieces) //Iterate through all the pieces on board
                    {
                        //Check if there is a piece, if there is, checks if it is a king on the current team
                        if (piece != null && ((piece.team == 0) == whiteTurn) && piece.type == ChessPieceType.King)
                        {
                            //Get and update the king's available moves
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
                //Deselect piece and remove any highlighting for it
                selectedPiece = null;
                RemoveHighlightTiles();
            }

        }
        else
        {
            //Reset tile highlighting when not hovering over any tile
            if (currentlyHovering != -Vector2Int.one)
            {
                board[currentlyHovering.x, currentlyHovering.y].layer = ContainsValidMove(ref availableMoves, currentlyHovering) ?
                                                                        LayerMask.NameToLayer("possibleMove") :
                                                                        LayerMask.NameToLayer("Tile");
                currentlyHovering = -Vector2Int.one;
            }
        }

        if (selectedPiece)
        {
            //Moves selected piece along a raised horiszontal plane while dragging it
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                selectedPiece.setPos(ray.GetPoint(distance) + Vector3.up * draggingOffset);
            }
        }
    }

    //Takes a piece and target move as inputs and returns whether it is a legal move
    private bool MoveTo(Pieces selPiece, int x, int y, bool makeChanges)
    //makeChanges controls if the pieces are moved or just checked for legality
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2(x, y)))
        {
            //If the target move is not in the piece's possible moves then it is illegal
            return false;
        }

        if (makeChanges)
        {
            //If a move is being made then the en passant from the last turn will no longer be possible
            enPassantX = -1;
        }
        //Stores previous position
        Vector2Int previousPos = new Vector2Int(selPiece.xPos, selPiece.yPos);
        //Stores which team is in check after the move
        int teamInCheck = simCheckMove(selPiece, x, y, previousPos); 

        if (teamInCheck == selPiece.team)
        {
            //If the current team is in check after their move then it is an illegal move
            isInCheck = true;
            return false;
        }
        
        if (chessPieces[x, y] != null) //If the target position has a piece already
        {
            Pieces targetPiece = chessPieces[x, y];

            if (selPiece.team == targetPiece.team)
            {
                //It is illegal to move a piece onto your other piece
                return false;
            }
            else if (makeChanges)
            {
                //If you try to move onto your opponents piece then it will be destroyed
                Destroy(chessPieces[x, y].gameObject);
                chessPieces[x, y] = null;
            }
        }
        else if (makeChanges)
        {
            //Check if an en passant capture was made
            if (selPiece.type == ChessPieceType.Pawn &&
                Math.Abs(y - selPiece.yPos) == 1 &&
                Math.Abs(x - selPiece.xPos) == 1)
            {
                //Destroys the correct piece in the case of en passant (behind its new position)
                Destroy(chessPieces[x, (selPiece.team == 0) ? (y - 1) : (y + 1)].gameObject);
                chessPieces[x, (selPiece.team == 0) ? (y - 1) : (y + 1)] = null;
            }
        }

        teamInCheck = simCheckMove(selPiece, x, y, previousPos);
        if (makeChanges)
        {
            if (teamInCheck == -1)
            { //Did not check the other team
                isInCheck = false;
            }
            else
            { //Put the other team in check
                isInCheck = true;
            }
        }

        if (makeChanges)
        {
            //Move the piece to its new position
            chessPieces[x, y] = selPiece;
            chessPieces[previousPos.x, previousPos.y] = null;

            if (selPiece.type == ChessPieceType.Pawn && Math.Abs(y - selPiece.yPos) == 2)
            {
                //If a pawn moved forward twice then en passant is possible for it
                enPassantX = x;
            }

            if (selPiece.type == ChessPieceType.King && Math.Abs(x - selPiece.xPos) > 1)
            {
                //Move the rook accordingly
                if (x > selPiece.xPos) //King side castling
                {
                    //Determine the rook's position
                    int rookX = 7;
                    int rookY = selPiece.team == 0 ? 0 : 7;

                    //Move the rook to its new position
                    chessPieces[5, rookY] = chessPieces[rookX, rookY];
                    chessPieces[rookX, rookY] = null;
                    PositionSinglePiece(5, rookY); //Update the position of the rook
                }
                else //Queen side castling
                {
                    //Determine the rook's position
                    int rookX = 0;
                    int rookY = selPiece.team == 0 ? 0 : 7;

                    //Move the rook to its new position
                    chessPieces[3, rookY] = chessPieces[rookX, rookY];
                    chessPieces[rookX, rookY] = null;
                    PositionSinglePiece(3, rookY); //Update the position of the rook
                }                
            }

            //Position the piece visually
            PositionSinglePiece(x, y);
        }
        //Move is legal
        return true;
    }


    private int simCheckMove(Pieces selPiece, int targetX, int targetY, Vector2Int previousPos)
    {
        //Creates a new board with all the same pieces and positions
        Pieces[,] chessPiecesCopy = new Pieces[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessPieces[i, j] != null)
                {
                    //Copy attributes from the original piece to the clone
                    chessPiecesCopy[i, j] = Instantiate(chessPieces[i, j]).GetComponent<Pieces>();
                    chessPiecesCopy[i, j].CopyAttributes(chessPieces[i, j]);
                }
            }
        }

        //Place the selected piece and remove it from its previous position
        chessPiecesCopy[targetX, targetY] = selPiece;
        chessPiecesCopy[previousPos.x, previousPos.y] = null;

        //Check for possible moves and if the opponent's king is in check
        foreach (Pieces currentSimPiece in chessPiecesCopy)
        {
            if (currentSimPiece != null)
            {
                List<Vector2Int> availableMovesCopy = currentSimPiece.GetAvailableMoves(ref chessPiecesCopy);

                //For every piece checks every move
                foreach (Vector2Int move in availableMovesCopy)
                {
                    Pieces targetPiece = chessPiecesCopy[move.x, move.y];
                    //Checks if king is a possible target of a move
                    if (targetPiece != null &&
                        targetPiece.type == ChessPieceType.King &&
                        targetPiece.team != currentSimPiece.team)
                    {
                        return targetPiece.team; //Return the team of the king in check
                    }
                }
            }
        }

        return -1; //Return -1 if no king is in check
    }


    private bool isCheckmate()
    {
        if (!isInCheck)
        {
            //King must be in check for there to be a checkmate
            return false;
        }

        int team = whiteTurn ? 0 : 1;

        foreach (Pieces currentPiece in chessPieces)
        {
            if (currentPiece != null && currentPiece.team == team) //For all of this team's pieces
            {
                List<Vector2Int> availableMovesCopy = currentPiece.GetAvailableMoves(ref chessPieces);

                foreach (Vector2Int move in availableMovesCopy)
                {
                    if (simCheckMove(currentPiece, move.x, move.y, new Vector2Int(currentPiece.xPos, currentPiece.yPos)) != currentPiece.team)
                    {
                        //If there are any legal moves (take them out of check) then it is not checkmate
                        return false;
                    }
                }
            }
        }
        //If there are no legal moves then it is checkmate
        return true;
    }


    //Generate the board with individual tiles
    private void CreateBoard(float boardSize)
    {
        //Adjust yOffset based on the object's position to align with board assets used
        yOffset += transform.position.y;

        //Initialize the 8x8 board
        board = new GameObject[8, 8];

        //Create individual tiles for the chessboard
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                //Generate each individual tile for each x,y coordinate on board
                board[x, y] = CreateTile(boardSize, x, y);
            }
        }

        //Calculate the bounds dynamically based on the actual board size
        bounds = new Vector3(4 * boardSize, 0, 4 * boardSize);
    }

    //Create an individual tile
    private GameObject CreateTile(float boardSize, int x, int y)
    {
        //Create tile and name it by its x,y coordinate on board
        GameObject tileObj = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObj.transform.parent = transform; //Link tile to the whole board

        //Render tile mesh
        Mesh mesh = new Mesh();
        tileObj.AddComponent<MeshFilter>().mesh = mesh;
        tileObj.AddComponent<MeshRenderer>().material = tileMat; //Set tile material

        //Set tile's 4 corners
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * boardSize, yOffset, y * boardSize) - bounds;
        vertices[1] = new Vector3(x * boardSize, yOffset, (y + 1) * boardSize) - bounds;
        vertices[2] = new Vector3((x + 1) * boardSize, yOffset, y * boardSize) - bounds;
        vertices[3] = new Vector3((x + 1) * boardSize, yOffset, (y + 1) * boardSize) - bounds;

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        tileObj.layer = LayerMask.NameToLayer("Tile"); //Set default layer for highlighting
        tileObj.AddComponent<BoxCollider>(); //Give tile collider

        return tileObj;
    }

    //Spawn all pieces by calling SpawnSinglePiece() for each chess piece
    private void SpawnAllPieces()
    {
        //8x8 2D array of Pieces objects
        chessPieces = new Pieces[8, 8];

        int white = 0;
        int black = 1;

        //Spawning all white pieces in the starting position
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, white);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, white);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        //Spawn white pawns
        for (int i = 0; i < 8; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, white);
        }

        //Spawning all black pieces in the starting position
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, black);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, black);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        //Spawn black pawns
        for (int i = 0; i < 8; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, black);
        }
    }

    //Spawn a single piece
    private Pieces SpawnSinglePiece(ChessPieceType type, int team)
    {
        //Spawn a piece
        Pieces cPiece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Pieces>();

        cPiece.type = type;
        cPiece.team = team;
        cPiece.GetComponent<MeshRenderer>().material = teamColour[team];
        //Sets piece team and type

        return cPiece;
    }

    //Setting piece positions
    private void PositionAllPieces()
    {
        //Runs PositionSinglePiece for every piece on the board
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
        //Align piece attributes with board position
        chessPieces[x, y].xPos = x;
        chessPieces[x, y].yPos = y;
        chessPieces[x, y].setPos(TileCenter(x, y)); //Centre piece on tile
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            //Applies possibleMoves layer to the legal moves a selected piece has
            board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("possibleMove");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            //Resets layer to the default
            board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        //Resets list of available moves
        availableMoves.Clear();
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        //Checks if a move is in that piece's possible moves
        foreach (Vector2Int move in moves)
        {
            if (move.x == pos.x && move.y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    //Gets the center of a tile
    private Vector3 TileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Lookup 2D index of a tile in the board array
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == hitInfo)
                {
                    //Return the positon of the tile that was hit
                    return new Vector2Int(x, y);
                }
            }
        }
        //Return an invalid index if not found
        return new Vector2Int(-1, -1);
    }

    //Fucntion to interpret a FEN string given and return the board it describs
    private Pieces[,] InterpretFEN(string FENString)
    {
        string[] FEN = FENString.Split(' ');

        Pieces[,] pieces = new Pieces[8, 8]; //Creates the board
        
        //FEN starts at the top right
        int rank = 7;
        int file = 0;

        int white = 0;
        int black = 1;
        //Creates a dictionary to decode the characters in FEN to the piece they represent
        var pieceCodes = new Dictionary<char, Tuple<ChessPieceType, int>>()
        {
            {'r', Tuple.Create(ChessPieceType.Rook, black)},
            {'n', Tuple.Create(ChessPieceType.Knight, black)},
            {'b', Tuple.Create(ChessPieceType.Bishop, black)},
            {'q', Tuple.Create(ChessPieceType.Queen, black)},
            {'k', Tuple.Create(ChessPieceType.King, black)},
            {'p', Tuple.Create(ChessPieceType.Pawn, black)},
            {'R', Tuple.Create(ChessPieceType.Rook, white)},
            {'N', Tuple.Create(ChessPieceType.Knight, white)},
            {'B', Tuple.Create(ChessPieceType.Bishop, white)},
            {'Q', Tuple.Create(ChessPieceType.Queen, white)},
            {'K', Tuple.Create(ChessPieceType.King, white)},
            {'P', Tuple.Create(ChessPieceType.Pawn, white)}
        };

        for (int i = 0; i < FEN[0].Length; i++)
        {
            char character = FEN[0][i];
            if (character == '/') //Forward slashes indicate next rank (1 down)
            {
                //Move to start of row below
                file = 0;
                rank--;
            }
            else if (char.IsDigit(character))
            {
                //Numbers represent how many empty tiles in a row there are
                file += int.Parse(character.ToString());
            }
            else
            {
                //Letters are spawned using the dictionary in their position
                pieces[file, rank] = SpawnSinglePiece(pieceCodes[character].Item1,
                                                    pieceCodes[character].Item2);
                file++;
            }
        }

        //whiteTurn is set to true if it says "w" and false if "b"
        whiteTurn = FEN[1] == "w";

        //Sets castling rights
        whiteKingCastle = FEN[2].Contains("K");
        whiteQueenCastle = FEN[2].Contains("Q");
        blackKingCastle = FEN[2].Contains("k");
        blackQueenCastle = FEN[2].Contains("q");

        //Sets en passant file if ther is one
        char enPassant = FEN[3][0];
        enPassantX = char.ToUpper(enPassant) - 65; //Converts the letters used for files to numbers

        //Clocks counting moves and half moves
        halfmoveClock = int.Parse(FEN[4]);
        fullmoveClock = int.Parse(FEN[5]);

        return pieces;
    }

   
    private IEnumerator Tutorial()
    {
        //Set up the chessboard
        chessPieces = new Pieces[8, 8];

        textBox.text = "Welcome to the tutorial. Click to continue";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Place a pawn and explain its movement
        chessPieces[4, 1] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
        PositionSinglePiece(4, 1);
        textBox.text = 
        "This is a pawn. It can move 1 square forward or 2 squares forward on its first move";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the pawn two squares forward
        chessPieces[4, 3] = chessPieces[4, 1];
        chessPieces[4, 1] = null;
        PositionSinglePiece(4, 3);
        textBox.text = "This is the pawn moving forward 2 squares";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the pawn one more square forward
        chessPieces[4, 4] = chessPieces[4, 3];
        chessPieces[4, 3] = null;
        PositionSinglePiece(4, 4);
        textBox.text = "This is the pawn moving forward 1 more square";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Clear the board
        ClearBoard();

        //Place a knight and explain its movement
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, 0);
        PositionSinglePiece(1, 0);
        textBox.text = 
        "This is a knight. It can move in an L-shape (2 vertically/horizontally then 1 horizontally/vertically)";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the knight in an L-shape
        chessPieces[2, 2] = chessPieces[1, 0];
        chessPieces[1, 0] = null;
        PositionSinglePiece(2, 2);
        textBox.text = "This is the knight moving 2 squares vertically then 1 square horizontally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        ClearBoard();

        //Place a bishop and explain its movement
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, 0);
        PositionSinglePiece(2, 0);
        textBox.text = "This is a bishop. It can move diagonally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the bishop diagonally
        chessPieces[5, 3] = chessPieces[2, 0];
        chessPieces[2, 0] = null;
        PositionSinglePiece(5, 3);
        textBox.text = "This is the bishop moving diagonally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        ClearBoard();

        //Place a rook and explain its movement
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, 0);
        PositionSinglePiece(0, 0);
        textBox.text = "This is a rook. It can move horizontally or vertically";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the rook horizontally
        chessPieces[7, 0] = chessPieces[0, 0];
        chessPieces[0, 0] = null;
        PositionSinglePiece(7, 0);
        textBox.text = "This is the rook moving horizontally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        ClearBoard();

        //Place a queen and explain its movement
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, 0);
        PositionSinglePiece(3, 0);
        textBox.text = "This is a queen. It combines the movements of a rook and a bishop";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the queen diagonally
        chessPieces[7, 4] = chessPieces[3, 0];
        chessPieces[3, 0] = null;
        PositionSinglePiece(7, 4);
        textBox.text = "This is the queen moving diagonally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the queen vertically
        chessPieces[7, 7] = chessPieces[7, 4];
        chessPieces[7, 4] = null;
        PositionSinglePiece(7, 7);
        textBox.text = "This is the queen moving vertically";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        ClearBoard();

        //Place a king and explain its movement
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, 0);
        PositionSinglePiece(4, 0);
        textBox.text = "This is a king. It can move one square in any direction";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        //Move the king one square diagonally
        chessPieces[5, 1] = chessPieces[4, 0];
        chessPieces[4, 0] = null;
        PositionSinglePiece(5, 1);
        textBox.text = "This is the king moving diagonally";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();

        ClearBoard();

        textBox.text = "End of tutorial. Click to return to main menu";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("MainMenu");
    }


    private void ClearBoard()
    {
        //Iteratively deletes every piece on the board
        foreach (Pieces piece in chessPieces)
        {
            if (piece)
            {
                Destroy(chessPieces[piece.xPos, piece.yPos].gameObject);
                chessPieces[piece.xPos, piece.yPos] = null;
            }
        }
    }
}
