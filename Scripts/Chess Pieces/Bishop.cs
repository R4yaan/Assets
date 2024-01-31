using System.Collections.Generic;
using UnityEngine;

public class Bishop : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // All possible bishop moves
        int[] xOffset = { 1, 1, -1, -1 };
        int[] yOffset = { 1, -1, 1, -1 };

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
                    r.Add(new Vector2Int(newX, newY));
                }
                else
                {
                    if (board[newX, newY].team != team)
                    {
                        r.Add(new Vector2Int(newX, newY));
                    }
                    break; // Stop if capturing enemy piece
                }
            }
        }

        return r;
    }

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}
