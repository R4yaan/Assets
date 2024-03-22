using System.Collections.Generic;
using UnityEngine;

public class Pawn : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //Up if white team, down if black
        int direction = (team == 0) ? 1 : -1;

        //Move one square forward
        if (onBoard(xPos, yPos + direction) && board[xPos, yPos + direction] == null)
        {
            moves.Add(new Vector2Int(xPos, yPos + direction));
        }

        //Move two squares forward from starting position
        if (onBoard(xPos, yPos + (2 * direction)) && (yPos == 1 || yPos == 6) && board[xPos, yPos + direction] == null && board[xPos, yPos + (2 * direction)] == null)
        {
            moves.Add(new Vector2Int(xPos, yPos + (2 * direction)));
        }

        //Capture diagonally on the right
        if (onBoard(xPos + direction, yPos + direction))
        {
            //Check if there's a piece at the target position
            if (board[xPos + direction, yPos + direction] != null)
            {
                //Check if the piece at the target position is of a different team
                if (board[xPos + direction, yPos + direction].team != team)
                {
                    moves.Add(new Vector2Int(xPos + direction, yPos + direction));
                }
            }
            else if (xPos + direction == ChessBoard.enPassantX && yPos == (4 - team))
            {
                //Add en passant move if applicable
                moves.Add(new Vector2Int(xPos + direction, yPos + direction));
            }
        }

        //Capture diagonally on the left
        if (onBoard(xPos - direction, yPos + direction))
        {
            //Check if there's a piece at the target position
            if (board[xPos - direction, yPos + direction] != null)
            {
                //Check if the piece at the target position is of a different team
                if (board[xPos - direction, yPos + direction].team != team)
                {
                    moves.Add(new Vector2Int(xPos - direction, yPos + direction));
                }
            }
            else if (xPos - direction == ChessBoard.enPassantX && yPos == (4 - team))
            {
                //Add en passant move if applicable
                moves.Add(new Vector2Int(xPos - direction, yPos + direction));
            }
        }

        return moves;
    }
}
