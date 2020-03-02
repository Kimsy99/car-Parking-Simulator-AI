using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Health : MonoBehaviour
{
    public GameObject player;
    public bool FTcalled = true;
    public int scoreNum = 0;
    public float finScore = 0;
     
    public void diedNow()
    {
        if(FTcalled)
        {
            FTcalled = false;
            scoreNum = player.GetComponent<NeuralNetwork>().neurNum;
            
            player.GetComponent<Car_Movement>().enabled = false;
            NeuralNetwork.scores[scoreNum] = finScore;

            if (NeuralNetwork.maxScore <= NeuralNetwork.scores[scoreNum])
            {
                NeuralNetwork.maxScore = NeuralNetwork.scores[scoreNum];
                Debug.Log(NeuralNetwork.maxScore);
            }

            
            
            GameManager.countEnd++;
            Destroy(player);
        }
    }   
}

