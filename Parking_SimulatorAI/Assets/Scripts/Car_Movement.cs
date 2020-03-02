using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Movement : MonoBehaviour
{
    public GameObject targetNode; //the target where we want to park
    public GameObject player; //the player itself
    public Rigidbody rb; //the rb of the car
    public WheelCollider wheelFR, wheelFL, wheelRR, wheelRL; //the wheels of the car
    
    //forces of the car
    public float maxBreakTorque = 150f; //max break force
    public float maxMotorTorque = 60f; //max torque
    public float currentSpeed; //current speed
    public float maxSpeed=70f; //max speed
    public Vector3 centerOfMass; //centre mass of car
    public bool isBraking = false; //is the car braking?
    [Header("Sensors")]

    //angles of the sensors
    public float sideSensorAngle = 90;
    public float frontSensorAngle = 25;
    public float backSensorAngle = 155;
    public float frontSensorAngle2 = 55;
    public float backSensorAngle2 = 125;
    
    float timeLeft = 40.0f; //time before the car dies
    
    //outputs 
    public float inputAngle;
    public float inputMove;
    public float inputBrake;

    //inputs
    public float distFront;
    public float distRFront;
    public float distLFront;
    public float distSRight;
    public float distSLeft;
    public float distBack;
    public float distRBack;
    public float distLBack;
    public float distRFront2;
    public float distLFront2;
    public float distRBack2;
    public float distLBack2;

    public float score;

    public NeuralNetwork neu;
    public PlayerScore sc;

    public float angle;
    public float angle2;
    
    
    //enteredTrigger
    public bool ensI = false; //The car has set the neural network, and the car can start driving
    public bool ensIS = false;//The max distance has been set, car can start driving
    
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass; //set centre of mass
        neu.setSid(); //set neural network id going to be used
        ensI = true; //finish setting
    }

    void Update()
    {
        if (ensI && ensIS) //if havent set finish, dont run
        {
            Sensors();
            score = sc.distance;
            angle= Vector3.Angle(player.transform.forward, targetNode.transform.position - player.transform.position);//*Mathf.Deg2Rad;
            angle2= Vector3.Angle(player.transform.right, targetNode.transform.position - player.transform.right);
           

           if (angle2 > 90f)
           {
               angle *= -1;
           }

           neu.neuralStart(distFront, distRFront, distRFront2, distSRight, distRBack2, distRBack, distBack, distLBack, distLBack2, distSLeft, distLFront2, distLFront, score, angle, ref inputAngle, ref inputMove, ref inputBrake);
            ApplySteer();
            Braking();
            Drive();
            timeLeft -= Time.deltaTime;
        
            if(timeLeft < 0) //if time over, end
            {
                sc.diedNow();
            }
        
            //if fall out of floor, just in case glitch occurs (but probably will not
            if (rb.position.y < -1 || rb.position.y > 45 || transform.up.y < 0.1)
            {
                sc.colScore = -100;
                sc.diedNow();
            }  
        }
        
    }

    private void Sensors()
    {

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask1 = 1 << 9;
        int layerMask2 = 1 << 10;
        int layerMask = layerMask1 | layerMask2;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
          // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            distFront = hit.distance;
        }
        else
        {
          // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            distFront = 1000;
        }
        
        //right front
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(frontSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(frontSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distRFront = hit.distance;
        }
        else
        {
         //  Debug.DrawRay(transform.position, Quaternion.AngleAxis(frontSensorAngle, transform.up)*transform.forward * 1000, Color.yellow);
            distRFront = 1000;
        }
        
        //left front
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-frontSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-frontSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distLFront = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-frontSensorAngle, transform.up)*transform.forward * 1000, Color.yellow);
            distLFront = 1000;
        }
        
        //side right
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(sideSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(sideSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distSRight = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(sideSensorAngle, transform.up)*transform.forward *1000, Color.yellow);
            distSRight = 1000;
        }
        
        //side left
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distSLeft = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up)*transform.forward * 1000, Color.yellow);
            distSLeft = 1000;
        }
        
        //back right
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(backSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(backSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distRBack = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(backSensorAngle, transform.up)*transform.forward *1000, Color.yellow);
            distRBack = 1000;
        }
        
        //back left
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-backSensorAngle, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-backSensorAngle, transform.up)*transform.forward * hit.distance, Color.yellow);
            distLBack = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-backSensorAngle, transform.up)*transform.forward * 1000, Color.yellow);
            distLBack = 1000;
        }
        
        //back
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(180, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(180, transform.up)*transform.forward * hit.distance, Color.yellow);
            distBack = hit.distance;
        }
        else
        {
          //  Debug.DrawRay(transform.position, Quaternion.AngleAxis(180, transform.up)*transform.forward * 1000, Color.yellow);
            distBack = 1000;
        }
        
        //right front2
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(frontSensorAngle2, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(frontSensorAngle2, transform.up)*transform.forward * hit.distance, Color.yellow);
            distRFront2 = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(frontSensorAngle2, transform.up)*transform.forward * 1000, Color.yellow);
            distRFront2 = 1000;
        }
        
        //left front2
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-frontSensorAngle2, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
          // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-frontSensorAngle2, transform.up)*transform.forward * hit.distance, Color.yellow);
            distLFront2 = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-frontSensorAngle2, transform.up)*transform.forward * 1000, Color.yellow);
            distLFront2 = 1000;
        }
        
        //back right
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(backSensorAngle2, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(backSensorAngle2, transform.up)*transform.forward * hit.distance, Color.yellow);
            distRBack2 = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(backSensorAngle2, transform.up)*transform.forward *1000, Color.yellow);
            distRBack2 = 1000;
        }
        
        //back left
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-backSensorAngle2, transform.up)*transform.forward, out hit, Mathf.Infinity, layerMask))
        {
          //  Debug.DrawRay(transform.position, Quaternion.AngleAxis(-backSensorAngle2, transform.up)*transform.forward * hit.distance, Color.yellow);
            distLBack2 = hit.distance;
        }
        else
        {
           // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-backSensorAngle2, transform.up)*transform.forward * 1000, Color.yellow);
            distLBack2 = 1000;
        }
        
        
    }
    
    private void ApplySteer()
    {
        inputAngle *= 90;
        inputAngle -= 45;
        wheelFL.steerAngle = inputAngle;
        wheelFR.steerAngle = inputAngle;
    }

    private void Drive()
    {
        
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;
        if (inputMove >= 0.5)
        {
            if (currentSpeed < maxSpeed  && !isBraking) {
                wheelFL.motorTorque=maxMotorTorque;
                wheelFR.motorTorque=maxMotorTorque;
            }
            else
            {
                wheelFL.motorTorque = 0;
                wheelFR.motorTorque = 0;
            }
        }
        else
        {
            if (currentSpeed < maxSpeed && !isBraking) {
                wheelFL.motorTorque=-maxMotorTorque;
                wheelFR.motorTorque=-maxMotorTorque;
            }
            else
            {
                wheelFL.motorTorque = 0;
                wheelFR.motorTorque = 0;
            }
        }
    }
    
    private void Braking()
    {
        if (inputBrake >= 0.5)
        {
            isBraking = true;
        }
        else
        {
            isBraking = false;
        }
        if (isBraking)
        {
            
            wheelRL.brakeTorque = maxBreakTorque;
            wheelRR.brakeTorque = maxBreakTorque;
        }
        else
        {
            
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }
    }

    
 }
