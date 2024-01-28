using System.Collections.Generic;
using UnityEngine;

public class Pawn : Pieces
{
    

    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        
        int direction = (team == 0) ? 1 : -1;

        if(board[xPos, yPos + direction] == null)
        {
            r.Add(new Vector2Int(xPos,yPos + direction));
        }
        if((yPos-3.5)*direction == -2.5 && board[xPos, yPos + (2*direction)] == null)
        {
            r.Add(new Vector2Int(xPos,yPos + (2*direction)));
        }


        // Check for capturing diagonally on right
        if (xPos + direction >= 0 && xPos + direction < 8 && yPos + direction >= 0 && yPos + direction < 8)
        {
            if (board[xPos + direction, yPos + direction] != null && board[xPos + direction, yPos + direction].team != team)
            {
                r.Add(new Vector2Int(xPos + direction, yPos + direction));
            }
        }

        // Check for capturing diagonally on left
        if (xPos - direction >= 0 && xPos - direction < 8 && yPos + direction >= 0 && yPos + direction < 8)
        {
            if (board[xPos - direction, yPos + direction] != null && board[xPos - direction, yPos + direction].team != team)
            {
                r.Add(new Vector2Int(xPos - direction, yPos + direction));
            }
        }
        

        // En passant
        if(ChessBoard.enPassantX != -1)
        {
            // Check for en passant on right
            if (xPos + direction >= 0 && xPos + direction < 8 && yPos + direction >= 0 && yPos + direction < 8)
            {
                if (xPos + direction == ChessBoard.enPassantX && board[xPos + direction, yPos].team != team)
                {
                    r.Add(new Vector2Int(xPos + direction, yPos + direction));
                }
            }

            // Check for en passant on left
            if (xPos - direction >= 0 && xPos - direction < 8 && yPos + direction >= 0 && yPos + direction < 8)
            {
                if (xPos - direction == ChessBoard.enPassantX && board[xPos - direction, yPos].team != team)
                {
                    r.Add(new Vector2Int(xPos - direction, yPos + direction));
                }
            }
        }

        return r;
    }
}