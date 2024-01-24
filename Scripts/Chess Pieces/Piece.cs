using UnityEngine;

public enum PieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}


public class Piece : MonoBehaviour
{
    public bool team;
    public int xPos;
    public int yPos;
    public PieceType tpye;
    
    private Vector3 targetPos;
    private Vector3 targetScale;

    string[] FEN = {"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-", "0 1"};
}
