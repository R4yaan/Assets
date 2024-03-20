using System.Collections.Generic;
using UnityEngine;

public class Knight : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // All possible knight moves
        int[] xOffset = { 1, 1, 2, 2, -1, -1, -2, -2, };
        int[] yOffset = { 2, -2, 1, -1, -2, 2, -1, 1, };

        for (int i = 0; i < xOffset.Length; i++)
        {
            int newX = xPos + xOffset[i];
            int newY = yPos + yOffset[i];

            if (onBoard(newX, newY) && (board[newX, newY] == null || board[newX, newY].team != team))
            {
                moves.Add(new Vector2Int(newX, newY));
            }
        }

        return moves;
    }

    private bool onBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}
