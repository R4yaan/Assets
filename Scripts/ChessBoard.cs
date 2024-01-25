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
    private Vector2Int currentHover;
    private Vector3 bounds;

    // FEN notation of board (starting position)
    string[] FEN = {"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-", "0 1"};
    private Pieces[,] chessPieces;

    // Board as 2D array of each tile
    private GameObject[,] board;
    
    // Piece that is currently selected to be moved
    private Pieces selectedPiece;



    private void Awake()
    {
        // Initialize the chessboard
        CreateBoard(tileSize);

        SpawnAllPieces();
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
        if (Physics.Raycast(ray, out info, 1000, LayerMask.GetMask("Tile", "Hover")))
        {
            // Gets coordinates of tile hit by ray (under mouse)
            Vector2Int hitPos = LookupTileIndex(info.transform.gameObject);

            // Highlight the hovered tile and update highlighted when mouse moves
            if(currentHover == -Vector2Int.one)
            {
                // If not hovering over any tile, currentHover set to hitPos
                currentHover = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            if(currentHover != hitPos)
            {
                // If mouse moves to hover over a different tile, update the highlighted tile
                board[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPos;
                board[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }
            if(Input.GetMouseButtonDown(0))
            {
                if(chessPieces[hitPos.x,hitPos.y] != null)
                {
                    bool turn = true;
                    if(turn)
                    {
                        selectedPiece = chessPieces[hitPos.x,hitPos.y];
                    }
                }
            }
            if(selectedPiece != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPos = new Vector2Int(selectedPiece.xPos,selectedPiece.yPos);
                bool legalMove = MoveTo(selectedPiece, hitPos.x, hitPos.y);
                if(!legalMove)
                {
                    selectedPiece.setPos(TileCenter(previousPos.x,previousPos.y));
                    selectedPiece = null;
                }
                else
                {
                    selectedPiece = null;
                }
            }

        }
        else
        {
            // Reset tile highlighting when not hovering over any tile
            if (currentHover != -Vector2Int.one)
            {
                board[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }

        if(selectedPiece)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                selectedPiece.setPos(ray.GetPoint(distance) + Vector3.up * draggingOffset);
            }
        }
    }

    private bool MoveTo(Pieces selPiece, int x, int y)
    {
        Vector2Int previousPos = new Vector2Int(selPiece.xPos, selPiece.yPos);
        
        if (chessPieces[x,y] != null)
        {
            Pieces targetPiece = chessPieces[x,y];

            if(selPiece.team == targetPiece.team)
            {
                return false;
            }
            else
            {
                Destroy(chessPieces[x,y].gameObject);
                chessPieces[x,y] = null;
            }
        }
        
        chessPieces[x,y] = selPiece;
        chessPieces[previousPos.x,previousPos.y] = null;
        
        PositionSinglePiece(x,y);

        return true;
    }

    // Generate board with individual tiles
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
        // Create tile and name it by it's x,y coordinate on board
        GameObject tileObj = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObj.transform.parent = transform; // Link tile to whole board

        // Render tile mesh
        Mesh mesh = new Mesh();
        tileObj.AddComponent<MeshFilter>().mesh = mesh;
        tileObj.AddComponent<MeshRenderer>().material = tileMat; // Set tile material

        // Set tile's 4 corners
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * boardSize, yOffset, y * boardSize) - bounds;
        vertices[1] = new Vector3(x * boardSize, yOffset, (y+1) * boardSize) - bounds;
        vertices[2] = new Vector3((x+1) * boardSize, yOffset, y * boardSize) - bounds;
        vertices[3] = new Vector3((x+1) * boardSize, yOffset, (y+1) * boardSize) - bounds;

        int[] triangles = new int[] {0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        tileObj.layer = LayerMask.NameToLayer("Tile");
        tileObj.AddComponent<BoxCollider>(); // Give tile collider

        return tileObj;
    }

    // Spawn pieces
    private void SpawnAllPieces()
    {
        chessPieces = new Pieces[8,8];

        int white = 0;
        int black = 1;
        
        // Spawning all white pieces in starting position
        chessPieces[0,0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        chessPieces[1,0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[2,0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[3,0] = SpawnSinglePiece(ChessPieceType.Queen, white);
        chessPieces[4,0] = SpawnSinglePiece(ChessPieceType.King, white);
        chessPieces[5,0] = SpawnSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[6,0] = SpawnSinglePiece(ChessPieceType.Knight, white);
        chessPieces[7,0] = SpawnSinglePiece(ChessPieceType.Rook, white);
        for (int i = 0; i < 8; i++){
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, white);
        }

        // Spawning all black pieces in starting position
        chessPieces[0,7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        chessPieces[1,7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[2,7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[3,7] = SpawnSinglePiece(ChessPieceType.Queen, black);
        chessPieces[4,7] = SpawnSinglePiece(ChessPieceType.King, black);
        chessPieces[5,7] = SpawnSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[6,7] = SpawnSinglePiece(ChessPieceType.Knight, black);
        chessPieces[7,7] = SpawnSinglePiece(ChessPieceType.Rook, black);
        for (int i = 0; i < 8; i++){
            chessPieces[i,6] = SpawnSinglePiece(ChessPieceType.Pawn, black);
        }
    }
    private Pieces SpawnSinglePiece(ChessPieceType type, int team)
    {
        Pieces cPiece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Pieces>();

        cPiece.type = type;
        cPiece.team = team;
        cPiece.GetComponent<MeshRenderer>().material = teamColour[team];

        return cPiece;
    }

    // Setting piece positions
    private void PositionAllPieces()
    {
        for (int x = 0; x < 8; x++){
            for (int y = 0; y < 8; y++){
                if(chessPieces[x,y] != null){
                    PositionSinglePiece(x,y);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y)
    {
        chessPieces[x,y].xPos = x;
        chessPieces[x,y].yPos = y;
        chessPieces[x,y].setPos(TileCenter(x,y));
    }

    // Gets the centre of a tile
    private Vector3 TileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) + new Vector3(tileSize/2, 0, tileSize/2);
    }

    // Lookup 2D index of a tile in the board array
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if(board[x,y] == hitInfo)
                {
                    // Return the hit tile
                    return new Vector2Int(x,y);
                }
            }
        }
        // Return an invalid index if not found (unused in practice)
        return new Vector2Int(-1,-1);
    }
}
