using System.Collections.Generic;
using UnityEngine;

public class King : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // All ossible king moves
        int[] xOffset = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] yOffset = { 1, 0, -1, 1, -1, 1, 0, -1 };

        for (int i = 0; i < xOffset.Length; i++)
        {
            int newX = xPos + xOffset[i];
            int newY = yPos + yOffset[i];

            if (onBoard(newX, newY) && (board[newX, newY] == null || board[newX, newY].team != team))
            {
                moves.Add(new Vector2Int(newX, newY));
            }
        }

        // Castling
        if (team == 0)
        {
            if (ChessBoard.whiteKingCastle && board[5, 0] == null && board[6, 0] == null)
            {
                moves.Add(new Vector2Int(6, 0));
            }
            if (ChessBoard.whiteQueenCastle && board[1, 0] == null && board[2, 0] == null && board[3, 0] == null)
            {
                moves.Add(new Vector2Int(2, 0));
            }
        }
        else
        {
            if (ChessBoard.blackKingCastle && board[5, 7] == null && board[6, 7] == null)
            {
                moves.Add(new Vector2Int(6, 7));
            }
            if (ChessBoard.blackQueenCastle && board[1, 7] == null && board[2, 7] == null && board[3, 7] == null)
            {
                moves.Add(new Vector2Int(2, 7));
            }
        }


        return moves;
    }

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}
