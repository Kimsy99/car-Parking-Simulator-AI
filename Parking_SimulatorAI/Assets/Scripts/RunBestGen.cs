using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RunBestGen : MonoBehaviour
{
    public static List<float[,]> runTxtGen() //To run the text file that is the best generation
    {
        
        //declare a temporary neural network to store the weights information of the best generation
        List<float[,]> temp = new List<float[,]>();
        float[,] weighInL1 = new float[NeuralNetwork.hidNodes, NeuralNetwork.inNodes];
        float[,] weighL1L2 = new float[NeuralNetwork.hidNodes, NeuralNetwork.hidNodes];
        float[,] weighL2Ou = new float[3, NeuralNetwork.hidNodes];
        temp.Add(weighInL1);
        temp.Add(weighL1L2);
        temp.Add(weighL2Ou);
        
        FileInfo theSourceFile = new FileInfo("test.txt"); //the information of the file we are going to use
        StreamReader reader = theSourceFile.OpenText(); //open the file we want to read

        for (int layer = 0; layer < temp.Count; layer++)
        {
            for (int i = 0; i < temp[layer].GetLength(0); i++)
            {
                for (int j = 0; j < temp[layer].GetLength(1); j++)
                {
                    temp[layer][i, j] = float.Parse(reader.ReadLine()); //read each line and fill the weights of the neural network iteratively
                }
            }
        }

        return temp; //return the neural network to be run by the car
    }
}

