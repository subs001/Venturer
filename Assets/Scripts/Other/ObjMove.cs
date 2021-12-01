using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjMove : MonoBehaviour
{
    public float min = 2f;
    public float max = 6f;
    public bool dirHorizontal = true;

    // Update is called once per frame
    void Update()
    {
        if(!dirHorizontal){
        transform.position = new Vector3(transform.position.x, Mathf.PingPong(Time.time * 2, max - min) + min, transform.position.z);

        }
        else{
            transform.position = new Vector3(Mathf.PingPong(Time.time * 2, max - min) + min, transform.position.y, transform.position.z);
        }

    }
}
