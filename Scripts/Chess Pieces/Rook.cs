using System.Collections.Generic;
using UnityEngine;

public class Rook : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Rook move up
        for (int i = yPos + 1; i < 8; i++)
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

        // Rook move down
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

        // Rook move right
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

        // Rook move left
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

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}