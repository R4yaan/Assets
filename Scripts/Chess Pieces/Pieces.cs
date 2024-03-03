using System.Collections.Generic;
using UnityEngine;

// Stores and numbers all piece types to be immutable
public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}


// Parent class representing individual chess pieces
public class Pieces : MonoBehaviour
{
    // Team identifier for the chess piece
    public int team;

    // Current X and Y positions of the chess piece on the board
    public int xPos;
    public int yPos;

    // Type of chess piece
    public ChessPieceType type;

    // Target position and scale for movements and animations
    private Vector3 targetPos;
    private Vector3 targetScale;
    

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        return r;
    }

    public virtual void setPos(Vector3 pos, bool force = false)
    {
        targetPos = pos;
        if(force)
        {
            transform.position = targetPos;
        }
    }


}