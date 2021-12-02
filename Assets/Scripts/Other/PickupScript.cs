using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public GameObject grapple;
    public GameObject rocketlauncher;
    // Start is called before the first frame update
    void Start()
    {
        grapple.SetActive(false);
        rocketlauncher.SetActive(false);
    }
    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "grapplegun")
        {
            Destroy(other.gameObject);
            grapple.SetActive(true);
            
        }
        if(other.gameObject.tag == "rocketlauncher"){
            Destroy(other.gameObject);
            rocketlauncher.SetActive(true);
                
        }
    }
}