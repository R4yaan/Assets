using System.Collections.Generic;
using UnityEngine;

public class Queen : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // All possible queen moves
        int[] xOffset = { 1, 1, -1, -1, 1, 0, -1, 0 };
        int[] yOffset = { 1, -1, 1, -1, 0, 1, 0, -1 };

        for (int i = 0; i < xOffset.Length; i++)
        {
            int newX = xPos;
            int newY = yPos;

            while (true)
            {
                newX += xOffset[i];
                newY += yOffset[i];

                if (!onBoard(newX, newY))
                {
                    break; // End loop if out of board
                }

                if (board[newX, newY] == null)
                {
                    moves.Add(new Vector2Int(newX, newY));
                }
                else
                {
                    if (board[newX, newY].team != team)
                    {
                        moves.Add(new Vector2Int(newX, newY));
                    }
                    break; // Stop if capturing enemy piece
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
