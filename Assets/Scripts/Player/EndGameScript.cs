using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
CAUTION: 
Once the scene is reloaded, the lighting becomes dark. Need to change that
**/


public class EndGameScript : MonoBehaviour
{
    
    private Vector3 coordinates;
    private Rigidbody rb;
    // private Vector3 spawnpos, newspawnpos;
    // keep track of number of times player falls off
    // private int falls = 0;
    // // so as to update "falls" properly
    // private bool fell = false;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // spawnpos = transform.position;
    }

    void Update()
    {

        

        /** the following code is for the respawning of the player to the start/last checkpoint position if it falls down **/
        // if (transform.position.y <-60)
        // {
        //     if (!fell) 
        //     {
        //         fell = true;
        //     }

        //     Respawn();
        // }

    }


    /** 
    Check whether player falls more than 3 times, then display gameover scene
    **/
    void CheckFall()
    {
        // /** if you fall down more than 3 times, game over **/
        //     if (falls >= 3) 
        //     {
        //         GameManager.GameOver();
        //     }
    }


    /** 
    This commented function requires your object's istrigger to be set 
    **/
    // private void OnTriggerStay(Collider obj)
    // {
    //     if (obj.transform.tag == "object" && Input.GetKeyDown(KeyCode.E))
    //     {
    //         GameManager.NewLevel();
    //     }
    // }




    /** 
    1. Respawns the player to the specified position, (0.5f,0f,0.5f) distance away from the crystal
    2. Feel free to change the values
    **/
    // void Respawn()
    // {
    //     falls += 1;
    //     CheckFall();
    //     /** add particle effects if you want  **/
    //     //Instantiate(particles, transform.position, Quaternion.Euler(270,0,0));
    //     transform.position = new Vector3 (newspawnpos.x+0.5f,spawnpos.y,newspawnpos.z+0.5f);
    //     rb.velocity = Vector3.zero;
    //     rb.angularVelocity = Vector3.zero;
    //     fell = false;
    // }




    /**
    1. Stores the position of the last collided checkpoint
    2. The player respawns to this point after dying
    3. Add Collider to the empty gameobject acting as checkpoint
    **/
    // void OnTriggerEnter(Collider col)
    // {
    //     if(col.gameObject.tag == "Checkpoint")
    //         newspawnpos = col.transform.position;
    // }




    /** 
    0. Add the crystal asset from https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
    1. The new function with the interactable crystal, can add any tag to the crystal, 
    2. make the corr change in code
    3. Load the next level based on your variables
    4. Add particle effects
    **/
    private void OnCollisionStay(Collision obj)
    {
        //Debug.Log("Touched an object!!");
        /** May need to press the key E multiple times. I don't like how it doesn't work sometimes **/
        if (obj.transform.tag == "Player" && Input.GetKeyDown(KeyCode.E))
        {
                GameManager.GameOver();
        }
    }
}
