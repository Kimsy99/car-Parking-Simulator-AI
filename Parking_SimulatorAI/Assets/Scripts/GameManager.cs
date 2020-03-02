using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static float restartDelay = 1f; //delay before restart occurs
    public static int countEnd = 0; //how many samples have finished running

    private void Update()
    {
        if (countEnd >= Clone.carNum) //if all cars finished running
        {
            countEnd = 0; //reset num of samples finished running
            EndGame(); //end the game
        }
    }
    
    void EndGame() //function to end the game
    {
        Debug.Log("Game Over");
        Invoke("Restart", restartDelay); //restart after the delay
       
    }
    
    void Restart() //restart the scene
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
