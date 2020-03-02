using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    //class shared
    public static int gen = 0; //The generation we are currently on
    public static int numOfSamples = Clone.runNum; //default number of samples we generate each generation
    public static int count = 0; //how many has already ran in this generation
    
    public static List<List<float[,]>> samples = new List<List<float[,]>>();  //save individual samples of neural networks
    public static float[] scores = new float[numOfSamples]; //store scores of all samples
    public static float[] splicingProb = new float[280]; //record probability of samples chosen for splicing based on their score
    
    public static float prevBestGenScore = 0; //previous generation best score
    public static float maxScore = 0; //Max score recorded
    public static float mutRate = 0.13f; //mutation rate
    public static System.Random r = new System.Random(); //random generator
    
    //neural network features that all samples have
    public static int inNodes = 14; //number of input nodes
    public static int outNodes = 3; //number of output nodes
    public static int hidNodes = (2*(inNodes+outNodes)/3)+1; //number of hidden nodes
    

    //object individual car
    public int neurNum; //The neural network number we will use for this car
    public bool fTime = true; //first time this sample ran?
    
    //size of input output nodes
    public float[] inputs = new float[inNodes]; //input nodes
    public float[] hiddL1 = new float[hidNodes]; //hidden layer 1
    public float[] hiddL2 = new float[hidNodes]; //hidden layer 2
    public float[] outputs = new float[outNodes]; //output node
    
    public List<float[,]> neurTBU = new List<float[,]>();  //neural to be used

    
    public static void runFinSample()//Run the neural network we want it to run
    {
        samples.Add(RunBestGen.runTxtGen());//Add the one we want it to run
    }

    public static void genSamples() //Generate all the samples
    {
        Debug.Log("Generating samples..");
        for (int i = 0; i<numOfSamples; ++i)
        { 
            samples.Add(genRNeu()); //generate all the samples
        }
        Debug.Log("Generated samples");
    }

   
    public static void geneticAlg()
    {
        Debug.Log("Doing genetic algorithm..");
        
        sortScore(); //sort the samples and the scores
      
        //save the best score
        string s2 = "score" + gen + ".txt";
        using (StreamWriter sw2 = new StreamWriter(s2))
        {
            sw2.WriteLine(scores[0]);
        }
        
        //save best in current generation
        string s1 = "gen" + gen + ".txt";

        List<float[,]> temp = samples[0];
        
        using (StreamWriter sw = new StreamWriter(s1))
        {
            for (int layer = 0; layer < temp.Count; layer++)
            {
                for (int i = 0; i < temp[layer].GetLength(0); i++)
                {
                    for (int j = 0; j < temp[layer].GetLength(1); j++)
                    {
                        sw.WriteLine(temp[layer][i, j]);
                    }
                }
            }
        }
        
        
        mutate();//perform mutation
        
        //print the scores
        for (int i = 0; i < 10; ++i)
        {
            Debug.Log(i + ": " + scores[i]);
        }
        
        ++gen; //we are moving on to the next generation
        count = 0; //used neural network number is reset
        
        //see if need to increase mutation rate
        if (System.Math.Abs(prevBestGenScore - scores[0]) <= 0.01)
        {
            mutRate = mutRate * 1.1f;
        }
        else
        {
            mutRate = mutRate * 0.9f;
        }
        
        prevBestGenScore = scores[0];
        
    }

    public void setSid() //set the neural network we are going to use for this car
    {
        neurNum = count;
        ++count;
        neurTBU = samples[neurNum];
    }

    public void neuralStart(float distFront, float distRFront, float distRFront2, float distSRight, float distRBack2, float distRBack, float distBack, float distLBack, float distLBack2, float distSLeft, float distLFront2, float distLFront, float score, float angle, ref float inputAngle, ref float inputMove, ref float inputBrake)
    {
        inputs[0] = score;
        inputs[1] = distFront;
        inputs[2] = distRFront;
        inputs[3] = distRFront2;
        inputs[4] = distSRight;
        inputs[5] = distRBack2;
        inputs[6] = distRBack;
        inputs[7] = distBack;
        inputs[8] = distLBack;
        inputs[9] = distLBack2;
        inputs[10] = distSLeft;
        inputs[11] = distLFront2;
        inputs[12] = distLFront;
        inputs[13] = angle;

        //do neural network
        for (int i = 0; i < hidNodes; ++i) 
        {
            for (int j = 0; j < inNodes; ++j)
            { 
                hiddL1[i] += inputs[j] * neurTBU[0][i, j];
            }
                    
            hiddL1[i] = (float)(1 / (1 + Math.Exp(-hiddL1[i])));
        }

        for (int i = 0; i < hidNodes; ++i)
        { 
            for (int j = 0; j < hidNodes; ++j)
            { 
                hiddL2[i] += hiddL1[j] * neurTBU[1][i, j];
            }
                
            hiddL2[i] = (float)(1 / (1 + Math.Exp(-hiddL2[i])));
        }
        
        for (int i = 0; i < 3; ++i) 
        {
            for (int j = 0; j < hidNodes; ++j)
            { 
                outputs[i] += hiddL2[j] * neurTBU[2][i, j];
            }
                    
            outputs[i] = (float)(1 / (1 + Math.Exp(-outputs[i])));
        }

        inputAngle = outputs[0]; 
        inputMove = outputs[1]; 
        inputBrake = outputs[2];

    }
    
    public static void sortScore() //sort the scores and neural networks
    {
        for (int i = 0; i < numOfSamples-1; i++)
        {
            for (int j = i+1; j < numOfSamples; j++)
            {
                if (scores[j] > scores[i])
                {
                    
                    float temp2 = scores[i];
                    scores[i] = scores[j];
                    scores[j] = temp2;

                    List<float[,]> temp1 = copyListF(samples[i]);
                    List<float[,]> temp3 = copyListF(samples[j]);
                    samples[j] = temp1;
                    samples[i] = temp3;
                    
                }
            }
        }
    }

    public static List<float[,]> copyListF(List<float[,]> a) //used to copy neural networks
    {
        //declare temporary neural network
        List<float[,]> tmp = new List<float[,]>();
        float[,] weighInL1 = new float[hidNodes, inNodes];
        float[,] weighL1L2 = new float[hidNodes, hidNodes];
        float[,] weighL2Ou = new float[outNodes, hidNodes];

        for (int i = 0; i < hidNodes; ++i)
        {
            for (int j = 0; j < inNodes; ++j)
            {
                weighInL1[i, j] = a[0][i, j];
            }
        }
        
        for (int i = 0; i < hidNodes; ++i)
        {
            for (int j = 0; j < hidNodes; ++j)
            {
                weighL1L2[i, j] = a[1][i, j];
            }
        }
        
        for (int i = 0; i < outNodes; ++i)
        {
            for (int j = 0; j < hidNodes; ++j)
            {
                weighL2Ou[i, j] = (a[2][i, j]);
            }
        }
        
        
        tmp.Add(weighInL1);
        tmp.Add(weighL1L2);
        tmp.Add(weighL2Ou);

        return tmp;
    }
    
    public static void mutate()
    {
        Debug.Log("Mutating");
        samples.RemoveRange(280, 720);

        calSplicingProb();
        
        for (int j = 0; j < 280; ++j) //for these networks, perform splicing
        {
            //generate two random numbers
            float a = (float)(r.NextDouble()); 
            float b = (float)(r.NextDouble());
            
            //to save which networks are going to be chosen for splicing
            int select1 = 0;
            int select2 = 0;
            
            //select the two neural networks that are going to be used
            for (int i = 0; i < 280; ++i)
            {
                if (a <= splicingProb[i])
                {
                    select1 = i;
                    break;
                }
            }
            for (int i = 0; i < 280; ++i)
            {
                if (b <= splicingProb[i])
                {
                    select2 = i;
                    break;
                }
            }
            
            List<float[,]> tempSam1 = copyListF(samples[select1]); //copy the samples
            List<float[,]> tempSam2 = copyListF(samples[select2]); //copy the samples
            
            splicing(ref tempSam1, ref tempSam2); //splice the samples
            
            randMut(ref tempSam1); //mutate the samples
            randMut(ref tempSam2); //mutate the samples
            
            samples.Add(tempSam1); //add the samples
            samples.Add(tempSam2); //add the samples
        } 

        for (int i = 0; i < 160; ++i) //generate random networks to prevent local maxima
        {
            samples.Add(genRNeu());
        }
        
        Debug.Log(samples.Count);
    }

    public static void calSplicingProb()
    { 
        float sumProb = 0f; //Sum of probability

        for (int i = 0; i < 280; i++)
        {
            sumProb += (float)(Math.Exp(scores[i]/10)); //total of the exponent scores
        }

        splicingProb[0] = (float) (Math.Exp(scores[0]/10)) / sumProb;
        
        for (int j = 1; j < 280; j++)
        {
            splicingProb[j] = splicingProb[j - 1] + (float) (Math.Exp(scores[j]/10)) / sumProb; //individual probabilities
        }
    }
    
    public static void splicing(ref List<float[,]> s1, ref List<float[,]> s2)
    {
        for (int layer = 0; layer < s1.Count; layer++)
        {
            for (int i = 0; i < s1[layer].GetLength(0); ++i)
            {
                for (int j = 0; j < s1[layer].GetLength(1); ++j)
                {
                    float weight = (float)(r.NextDouble());
                    float prob = (float)(r.NextDouble());
                    
                    if (prob < weight)
                    {
                        float temp = s2[layer][i, j];
                        s2[layer][i, j] = s1[layer][i, j];
                        s1[layer][i, j] = temp;
                    }
                }
            }
        }

    }
    
    public static void randMut(ref List<float[,]> samp) //perform random mutation
    {
        int fLoop, sLoop, tLoop;
        int fCount, sCount, tCount; //number of weights to be mutated 
        
        if ((int)(samp.Count * mutRate) == 0)
        {
            fCount = 1;
        }
        else
        {
            fCount = (int) (samp.Count * mutRate);
        }
        
        for (int i = 0; i < fCount; i++)
        {
            fLoop = r.Next(0, samp.Count - 1); //randomize index of first loop
            if ((int)(samp[fLoop].GetLength(0)*mutRate) == 0)
            {
                sCount = 1;
            }
            else
            {
                sCount = (int) (samp[fLoop].GetLength(0)*mutRate);
            }
            for (int j = 0; j < sCount; j++)
            {
                sLoop = r.Next(0, samp[fLoop].GetLength(0) - 1); //randomize index of second loop
                if ((int)(samp[fLoop].GetLength(1)*mutRate) == 0)
                {
                    tCount = 1;
                }
                else
                {
                    tCount = (int) (samp[fLoop].GetLength(1)*mutRate);
                }
                
                for (int k = 0; k < tCount; k++)
                {
                    tLoop = r.Next(0, samp[fLoop].GetLength(1) - 1); //randomize index of third loop
                    float prob = (float)r.NextDouble();
                    
                    if (prob < 0.3)
                    {
                        samp[fLoop][sLoop, tLoop] += (float)r.NextDouble();
                    }
                    else if (prob < 0.6)
                    {
                        samp[fLoop][sLoop, tLoop] -= (float)r.NextDouble();
                    }
                    else
                    {
                        float u1 = (float)(1.0-r.NextDouble()); //uniform(0,1] random doubles
                       float u2 = (float)(1.0-r.NextDouble());
                       float randStdNormal =(float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
                                            
                       samp[fLoop][sLoop,tLoop] = (float)(4*Math.Sqrt(6/17f) * randStdNormal); 
                    }

                    
                }  
            }
        }
    }
    
    public static List<float[,]> genRNeu() //generate individual random networks
    {
        //individual neural network
        List<float[,]> indSample = new List<float[,]>(); 
        float[,] weighInL1 = new float[hidNodes, inNodes]; 
        float[,] weighL1L2 = new float[hidNodes, hidNodes];
        float[,] weighL2Ou = new float[3, hidNodes];
            
        indSample.Add(weighInL1);
        indSample.Add(weighL1L2);
        indSample.Add(weighL2Ou);

        for (int k = 0; k < indSample.Count; ++k)
        {
            for (int i = 0; i < indSample[k].GetLength(0); ++i)
            {
                for (int j = 0; j < indSample[k].GetLength(1); ++j)
                {
                    //To generate uniform random numbers
                    float u1 = (float)(1.0-r.NextDouble()); //uniform(0,1] random doubles
                    float u2 = (float)(1.0-r.NextDouble());
                    float randStdNormal =(float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
                    float randVal = (float)(4*Math.Sqrt(6/17f) * randStdNormal); //before that is *4
                        
                    //assign the number
                    indSample[k][i, j] = randVal;
                }
            }
        }
            
        return indSample;
    }

    public static void trainBGen()
    {
        mutRate = 0.2f;
        List<float[,]> temporary = new List<float[,]>();
        
        if (Clone.firstRun)
        {
            for (int i = 0; i < 999; ++i)
            {
                temporary = copyListF(samples[0]);
                randMut(ref temporary);
                samples.Add(temporary);
            }
        }
        else
        {
            sortScore(); //sort the samples and the scores
      
            //save the best score
            string s2 = "score" + gen + ".txt";
            using (StreamWriter sw2 = new StreamWriter(s2))
            {
                sw2.WriteLine(scores[0]);
            }
        
            //save best in current generation
            string s1 = "gen" + gen + ".txt";

            List<float[,]> temp = samples[0];
        
            using (StreamWriter sw = new StreamWriter(s1))
            {
                for (int layer = 0; layer < temp.Count; layer++)
                {
                    for (int i = 0; i < temp[layer].GetLength(0); i++)
                    {
                        for (int j = 0; j < temp[layer].GetLength(1); j++)
                        {
                            sw.WriteLine(temp[layer][i, j]);
                        }
                    }
                }
            }
            
            samples.RemoveRange(280, 720);
            
            calSplicingProb();
            
            for (int j = 0; j < 360; ++j) //for these networks, perform splicing
            {
                //generate two random numbers
                float a = (float)(r.NextDouble()); 
                float b = (float)(r.NextDouble());
            
                //to save which networks are going to be chosen for splicing
                int select1 = 0;
                int select2 = 0;
            
                //select the two neural networks that are going to be used
                for (int i = 0; i < 280; ++i)
                {
                    if (a <= splicingProb[i])
                    {
                        select1 = i;
                        break;
                    }
                }
                for (int i = 0; i < 280; ++i)
                {
                    if (b <= splicingProb[i])
                    {
                        select2 = i;
                        break;
                    }
                }
            
                List<float[,]> tempSam1 = copyListF(samples[select1]); //copy the samples
                List<float[,]> tempSam2 = copyListF(samples[select2]); //copy the samples
            
                splicing(ref tempSam1, ref tempSam2); //splice the samples
            
                randMut(ref tempSam1); //mutate the samples
                randMut(ref tempSam2); //mutate the samples
            
                samples.Add(tempSam1); //add the samples
                samples.Add(tempSam2); //add the samples
            } 
            
            //print the scores
            for (int i = 0; i < 10; ++i)
            {
                Debug.Log(i + ": " + scores[i]);
            }
        
            ++gen; //we are moving on to the next generation
            count = 0; //used neural network number is reset
        
            //see if need to increase mutation rate
            if (System.Math.Abs(prevBestGenScore - scores[0]) <= 0.01)
            {
                mutRate = mutRate * 1.1f;
            }
            else
            {
                mutRate = mutRate * 0.9f;
            }
        
            prevBestGenScore = scores[0];
            
        }
        
        
    }

}
