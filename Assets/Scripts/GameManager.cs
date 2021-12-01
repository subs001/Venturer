using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/**
1. Add the Levels to build settings and maintain the order (scene number)
2. Add the game over scene to build settings
**/

public class GameManager : MonoBehaviour
{
    private static int currentLevel = 1; 
    private static int numoflevels = 2; /** assuming we have 2 levels **/

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public static void NewLevel(){
        if (currentLevel <= numoflevels) 
        {
            currentLevel += 1;
            SceneManager.LoadScene(currentLevel);
        }
        else 
        {
            print("You win"); /** Replace with appropriate stuff**/
        }
    }



    /**
    Assuming restarts current level
    **/
    public static void Restart()
    {
        SceneManager.LoadScene(currentLevel);
    }



    /**
    Assuming restarts from the first level
    **/
    public static void RestartGame()
    {
        SceneManager.LoadScene(2);
    }



    public static void GameOver()
    {
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene("GameOver");
    }

}
