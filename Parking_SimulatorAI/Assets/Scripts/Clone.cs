using System;
using UnityEngine;


public class Clone : MonoBehaviour
{
    public GameObject carOriginal; //The original car that we need to clone
    public GameObject obsOri;
    public static int runNum = 1000; //the total number of samples we want to run per generation
    public static int carNum = 250; //the number of cars we can run at one time due to hardware limitations
    
    public static bool firstRun = true; //to denote it is the first run of the whole thing and we need to generate samples
    public static bool runBestG = true;
    public static bool trainBestG = false;//do we want to run the best generation
    public static Vector3 pos;
    public static System.Random r = new System.Random();
    public static int genChange = 0;
    public static int yRotation = 90;
    void Start()
    {
        if (NeuralNetwork.count == runNum)
        {
            ++genChange;
        }
        
        if (firstRun || NeuralNetwork.count == runNum)
        {
            float xCoor = (float)(r.NextDouble()*(-1.61+8.27)-8.27); //4.5, -22.3 x -27to 12//-14 16 z
            float zCoor = (float)(r.NextDouble()*(3.23+2.78)-2.78);
            yRotation = r.Next(0, 90);
            //zCoor = (float)(1 + (zCoor / 17.5) * 1.3); //-2.3 2.3
            pos = new Vector3(xCoor, 2, zCoor);
        }



        if (runBestG) //if we want to run the best generation only
        {
            if (trainBestG)
            {
                if (firstRun)
                {
                    NeuralNetwork.runFinSample();
                }
                
                if(firstRun || NeuralNetwork.count == runNum)
               {
                    NeuralNetwork.trainBGen();
                    firstRun = false;
                }
            }
            else
            {
                if (firstRun)//if it is first run, thus we need to fill in the sample
                {
                    carNum = 1; //run one car only
                    runNum = 1; //run one car only
                    NeuralNetwork.runFinSample(); //read the best generation into the neural network
                }  
            }
            
            createCars(); //start generating cars 
            
        }
        else //if we want to run genetic algorithm
        {
            Debug.Log(NeuralNetwork.count); //display the number of cars that have already been ran in this generation due to hardware limitations 
            if (firstRun) //if it is first run and thus we need to generate samples
            {
                NeuralNetwork.genSamples(); //generate the samples
                firstRun = false; //set the first run equal false
            }
            else if (NeuralNetwork.count == runNum) //if we have completed running this generation
            {
                NeuralNetwork.geneticAlg(); //perform genetic selection
            }
       
            createCars(); //continue creating cars
        }
    }
    

    void createCars() //to clone cars on the map
    {
        //GameObject obstacle = Instantiate(obsOri, pos, Quaternion.Euler(0f, yRotation, 0f));//, pos, Quaternion.Euler(0f, yRotation, 0f)); //clone cars
        for (int i = 0; i < carNum; ++i)
        {
            GameObject carClone = Instantiate(carOriginal);//, pos, Quaternion.Euler(0f, yRotation, 0f)); //clone cars
            carClone.name = "Car" + i; //set each car name
        }
    }
}
    
