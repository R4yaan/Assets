using System.Collections.Generic;
using UnityEngine;

public class King : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // All ossible king moves
        int[] xOffset = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] yOffset = { 1, 0, -1, 1, -1, 1, 0, -1 };

        for (int i = 0; i < xOffset.Length; i++)
        {
            int newX = xPos + xOffset[i];
            int newY = yPos + yOffset[i];

            if (onBoard(newX, newY) && (board[newX, newY] == null || board[newX, newY].team != team))
            {
                r.Add(new Vector2Int(newX, newY));
            }
        }

        return r;
    }

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}
