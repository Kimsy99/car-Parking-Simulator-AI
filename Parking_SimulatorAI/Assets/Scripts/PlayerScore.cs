using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public GameObject player; //denote the player itself
    public GameObject goal; //denote where we want it to park

    //score of car
    public float score; //current score
    
    public float distance; //distance left until we reach goal
    private float maxDistance; //distance between spawn position of player and goal, indicating the max distance score possible
    private float distScore; //score achieved by being nearer and nearer to goal
    
    public int colScore = 0; //score if collision occur
    public int trigEnt = 0; //score to indicate the car entered the trigger
    public int trigStay = 0; //score to indicate the car stayed in the trigger position
    public float timeScore = 0;
    public int anglePScore = 0;
    
    //below is not sure if will work, a prevention of multiple calls
    public bool ftime = true; //to indicate it is first time the car died, to prevent multiple calls to the diednow() function.
    public bool dead = false; //to indicate the car died or not
    public bool firstSent = true; //to indicate if it is the first time signal that the car died. 
    
    private void Awake() //called when car is created
    {
        score = 0; //set initial score
        
        maxDistance = Vector3.Distance(player.transform.position, goal.transform.position); //calculate max distance
        player.GetComponent<Car_Movement>().ensIS = true; //show that this step is complete and the neural network can begin to run
    }

    public void diedNow() //signalled by things that kill the car. Did not signal 'Health' directly to enable score to update before game ends
    {
        if (ftime) //if it is the first time the function has been invoked
        {
            ftime = false; //it is not the first time
            dead = true; //the car is dead
        }
       
    }
    
    void Update()
    {
        distance = Vector3.Distance(player.transform.position, goal.transform.position); //calculate current distance from goal
        distScore = maxDistance - distance; //calculate the score by being nearer to the goal
        
        if (trigStay == 60)
        {
            timeScore += Time.deltaTime;
        }
        else
        {
            timeScore = 0;
            anglePScore = 0;
        
        }
        
        if (player.transform.rotation.y > -5 && player.transform.rotation.y < 5 && trigStay == 60) //if it is parked straight
        {
            anglePScore = 5; //add score to indicate it parked straight
        }
        else
        {
            anglePScore = 0;
        }
        
        score = distScore + trigEnt + trigStay + colScore + timeScore + anglePScore; //actual calculation of score containing all the other variables that affect score other than distance

        if (firstSent) //if it is the first time we send a signal to health
        {
            if (dead) //if the car is dead
            { 
                firstSent = false; //we have signalled 'Health' already
                player.gameObject.GetComponent<Health>().finScore = score;
                player.gameObject.GetComponent<Health>().diedNow(); //signal 'Health' that the car is dead
               
            } 
        }
        
        

    }
}
