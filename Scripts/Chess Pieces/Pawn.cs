using System.Collections.Generic;
using UnityEngine;

public class Pawn : Pieces
{
    

    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // Move one square forward
        if (onBoard(xPos, yPos + direction) && board[xPos, yPos + direction] == null)
        {
            r.Add(new Vector2Int(xPos, yPos + direction));
        }

        // Move two squares forward from starting position
        if (onBoard(xPos, yPos + (2 * direction)) && (yPos - 3.5) * direction == -2.5 && board[xPos, yPos + (2 * direction)] == null)
        {
            r.Add(new Vector2Int(xPos, yPos + (2 * direction)));
        }

        // Capture diagonally on the right
        if (onBoard(xPos + direction, yPos + direction) && board[xPos + direction, yPos + direction] != null && board[xPos + direction, yPos + direction].team != team)
        {
            r.Add(new Vector2Int(xPos + direction, yPos + direction));
        }

        // Capture diagonally on the left
        if (onBoard(xPos - direction, yPos + direction) && board[xPos - direction, yPos + direction] != null && board[xPos - direction, yPos + direction].team != team)
        {
            r.Add(new Vector2Int(xPos - direction, yPos + direction));
        }

        // En passant
        if (ChessBoard.enPassantX != -1)
        {
            // Check for en passant on the right
            if (onBoard(xPos + direction, yPos + direction) && xPos + direction == ChessBoard.enPassantX && board[xPos + direction, yPos].team != team)
            {
                r.Add(new Vector2Int(xPos + direction, yPos + direction));
            }

            // Check for en passant on the left
            if (onBoard(xPos - direction, yPos + direction) && xPos - direction == ChessBoard.enPassantX && board[xPos - direction, yPos].team != team)
            {
                r.Add(new Vector2Int(xPos - direction, yPos + direction));
            }
        }

        return r;
    }

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

}