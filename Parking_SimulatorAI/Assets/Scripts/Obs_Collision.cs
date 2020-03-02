using UnityEngine;

public class Obs_Collision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Player") //if collided
        {
            other.gameObject.GetComponent<PlayerScore>().colScore = -100; //set collision score
    
            other.gameObject.GetComponent<PlayerScore>().diedNow(); //player died
   
        }
    }
}
