using UnityEngine;

public class TriggerStay : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) //if something enters the trigger
    {
        //Debug.Log("TrigStay entered"); //print that it has exited
        other.GetComponent<PlayerScore>().trigStay += 20; //add score to indicate it has entered
    }

   private void OnTriggerExit(Collider other) //if it exited the trigger
   {
       //Debug.Log("TrigStay exited"); //print that it has exited
       other.GetComponent<PlayerScore>().trigStay -= 20; //deduct the score

   }
}
