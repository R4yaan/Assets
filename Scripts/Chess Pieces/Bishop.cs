using System.Collections.Generic;
using UnityEngine;

public class Bishop : Pieces
{
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //All possible bishop moves
        int[] xMovement = { 1, 1, -1, -1 };
        int[] yMovement = { 1, -1, 1, -1 };

        for (int i = 0; i < xMovement.Length; i++)
        {
            //Goes through all the positions in the x and y movement directions

            int newX = xPos;
            int newY = yPos;

            while (true)
            {
                newX += xMovement[i];
                newY += yMovement[i];

                if (!onBoard(newX, newY))
                {
                    break; //End loop if out of board
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
                    break; //End loop if there is a piece on target position
                }
            }
        }

        return moves;
    }
}
