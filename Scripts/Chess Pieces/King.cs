using System.Collections.Generic;
using UnityEngine;

public class King : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //All possible king moves
        int[] xMovement = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] yMovement = { 1, 0, -1, 1, -1, 1, 0, -1 };

        //Loop through possible directions
        for (int i = 0; i < xMovement.Length; i++)
        {
            int newX = xPos + xMovement[i];
            int newY = yPos + yMovement[i];

            //Check if target position is on the board and does not have an ally piece on it
            if (onBoard(newX, newY) && (board[newX, newY] == null || board[newX, newY].team != team))
            {
                moves.Add(new Vector2Int(newX, newY));
            }
        }

        //Castling
        if (team == 0) //White king castling
        {
            //King side castling
            if (ChessBoard.whiteKingCastle && board[5, 0] == null && board[6, 0] == null)
            {
                moves.Add(new Vector2Int(6, 0));
            }
            //Queen side castling
            if (ChessBoard.whiteQueenCastle && board[1, 0] == null && board[2, 0] == null && board[3, 0] == null)
            {
                moves.Add(new Vector2Int(2, 0));
            }
        }
        else //Black king
        {
            //King side castling
            if (ChessBoard.blackKingCastle && board[5, 7] == null && board[6, 7] == null)
            {
                moves.Add(new Vector2Int(6, 7));
            }
            //Queen side castling
            if (ChessBoard.blackQueenCastle && board[1, 7] == null && board[2, 7] == null && board[3, 7] == null)
            {
                moves.Add(new Vector2Int(2, 7));
            }
        }

        return moves;
    }
}
