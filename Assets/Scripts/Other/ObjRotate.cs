using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRotate : MonoBehaviour
{
    public float rotationSpeed = 40f;
    public int axis = 1;//0x, 1y,2z

    // Update is called once per frame
    void Update()
    {
        switch (axis)
        {
            case 0:
                transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);
                break;
            case 1:
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
                break;
            case 2:
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                break;
        }
    }
}
