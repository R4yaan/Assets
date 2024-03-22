using System.Collections.Generic;
using UnityEngine;

public class Rook : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //Rook moving up
        for (int i = yPos + 1; i < 8; i++)
        {
            if (onBoard(xPos, i))
            {
                if (board[xPos, i] == null)
                {
                    //Add positions that are on the board and empty
                    moves.Add(new Vector2Int(xPos, i));
                }
                else
                {
                    //Add position if the piece is an enemy
                    if (board[xPos, i].team != team)
                    {
                        moves.Add(new Vector2Int(xPos, i));
                    }
                    break;
                    //Stop adding pieces in this direction after meeting another piece
                }
            }
        }

        //Rook moving down
        for (int i = yPos - 1; i >= 0; i--)
        {
            if (onBoard(xPos, i))
            {
                if (board[xPos, i] == null)
                {
                    moves.Add(new Vector2Int(xPos, i));
                }
                else
                {
                    if (board[xPos, i].team != team)
                    {
                        moves.Add(new Vector2Int(xPos, i));
                    }
                    break;
                }
            }
        }

        //Rook moving right
        for (int i = xPos + 1; i < 8; i++)
        {
            if (onBoard(i, yPos))
            {
                if (board[i, yPos] == null)
                {
                    moves.Add(new Vector2Int(i, yPos));
                }
                else
                {
                    if (board[i, yPos].team != team)
                    {
                        moves.Add(new Vector2Int(i, yPos));
                    }
                    break;
                }
            }
        }

        //Rook moving left
        for (int i = xPos - 1; i >= 0; i--)
        {
            if (onBoard(i, yPos))
            {
                if (board[i, yPos] == null)
                {
                    moves.Add(new Vector2Int(i, yPos));
                }
                else
                {
                    if (board[i, yPos].team != team)
                    {
                        moves.Add(new Vector2Int(i, yPos));
                    }
                    break;
                }
            }
        }

        return moves;
    }
}