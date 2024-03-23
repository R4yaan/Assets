using System.Collections.Generic;
using UnityEngine;

public class Knight : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //All possible knight moves
        int[] xMovement = { 1, 1, 2, 2, -1, -1, -2, -2 };
        int[] yMovement = { 2, -2, 1, -1, -2, 2, -1, 1 };

        //Loop through possible knight moves
        for (int i = 0; i < xMovement.Length; i++)
        {
            int newX = xPos + xMovement[i];
            int newY = yPos + yMovement[i];

            //Check that target position does not have an ally piece
            if (onBoard(newX, newY) &&
            (board[newX, newY] == null || board[newX, newY].team != team))
            {
                moves.Add(new Vector2Int(newX, newY));
            }
        }

        return moves;
    }
}
