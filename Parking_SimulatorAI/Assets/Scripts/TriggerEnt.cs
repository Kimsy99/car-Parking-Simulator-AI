using UnityEngine;

public class TriggerEnt : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) //if it entered the trigger
    {
        if (other.GetComponent<PlayerScore>().trigEnt == 0) //if we had not given it a score
        {
           // Debug.Log("entered"); //print that one has entered
            other.GetComponent<PlayerScore>().trigEnt = 10; //give score because it has entered the parking spot
        }
    }
}
