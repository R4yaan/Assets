using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}


public class Pieces : MonoBehaviour
{
    public int team;
    public int xPos;
    public int yPos;
    public ChessPieceType type;
    
    private Vector3 targetPos;
    private Vector3 targetScale;
    
}
