using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void LocalMultiplayerLoad ()
    {
        //Loads main game scene for local multiplayer
        SceneManager.LoadScene("Game");
    }
    public void VariantsLoad ()
    {
        //Loads a version of the main game scene for local multiplayer with a different starting set up
        SceneManager.LoadScene("Horde");
    }
    public void TutorialLoad ()
    {
        //Loads a scene with a board and a tutorial for the player
        SceneManager.LoadScene("Tutorial");
    }
    public void QuitGame ()
    {
        //Closes application
        Debug.Log("Quit"); //Cannot quit application in unity editor
        Application.Quit();
    }

    public void MainMenuLoad ()
    {
        //Load the main menu
        SceneManager.LoadScene("MainMenu");
    }
}