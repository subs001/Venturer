using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float timer;
    private void Start()
    {
        Invoke("Destroy", timer);
    }
    private void Destroy()
    {
        Destroy(gameObject);
    }
}
