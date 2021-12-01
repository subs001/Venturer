using UnityEngine;
using System.Collections;
 
public class FootstepScript : MonoBehaviour {
	public float stepRate = 0.5f;
	public float stepCoolDown;
	public AudioClip footStep;
    public AudioSource audioSource;

	//get vars from player
	public PlayerMovement player;
 
	// Update is called once per frame
	void Update () {
		stepCoolDown -= Time.deltaTime;
		if ((Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f) && stepCoolDown < 0f && player.grounded){
            audioSource.PlayOneShot(footStep); 
			stepCoolDown = stepRate;
		}
	}
}