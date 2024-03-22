using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    public GameObject whiteVictoryScreen;
    public GameObject blackVictoryScreen;


    void Start()
    {
        whiteVictoryScreen.SetActive(false);
        blackVictoryScreen.SetActive(false);

        //Depending on who won, their victory screen is shown
        if (!ChessBoard.whiteTurn) //The player whose turn it was last lost
        {
            whiteVictoryScreen.SetActive(true);
        }
        else
        {
            blackVictoryScreen.SetActive(true);
        }
    }
}
