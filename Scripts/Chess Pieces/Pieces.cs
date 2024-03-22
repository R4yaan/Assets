using System.Collections.Generic;
using UnityEngine;

//Stores and numbers all piece types to be immutable
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


//Parent class representing individual chess pieces
public class Pieces : MonoBehaviour
{
    //Stores chess piece team
    public int team;

    //Stores piece's current x and y position
    public int xPos;
    public int yPos;

    //Type of chess piece
    public ChessPieceType type;

    //Target position for movements and animations
    private Vector3 targetPos;

    //Copy attributes from another Pieces object
    public void CopyAttributes(Pieces original)
    {
        //Copy attributes from original piece
        type = original.type;
        team = original.team;
    }

    //Function to check if coordinates are on the board
    public bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }


    private void Update()
    {
        //Movement animation
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        //Class to be overwritten for each piece which returns possbile moves
        List<Vector2Int> moves = new List<Vector2Int>();
        return moves;
    }

    public virtual void setPos(Vector3 pos, bool force = false) //Force removes animation
    {
        //Move the piece
        targetPos = pos;
        if(force)
        {
            transform.position = targetPos;
        }
    }
}