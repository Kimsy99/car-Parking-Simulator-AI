using UnityEngine;
using UnityEngine.UI;

public class UIinfo : MonoBehaviour
{
    public Text scoreText; //Store best score
    
    void Update()
    {
        //Print best score
        scoreText.text = ( "Gen " + NeuralNetwork.gen.ToString() + " best score: " + NeuralNetwork.scores[0].ToString());
    }
}
