using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void LocalMultiplayerLoad ()
    {
        // Loads main game scene for local multiplayer
        SceneManager.LoadScene("Game");
    }
    public void VariantsLoad ()
    {
        Debug.Log("Variants");
        //SceneManager.LoadScene("Variants");
    }
    public void TutorialLoad ()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void QuitGame ()
    {
        // Closes application
        Debug.Log("Quit"); // Cannot quit application in unity editor
        Application.Quit();
    }
}