using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : MonoBehaviour
{
    public float Speed = 0.15f;
    void Start()
    {
        Invoke("DoIt", Speed);
    }

    void DoIt()
    {
        transform.Rotate(Vector3.forward, Random.value * 360);
        Invoke("DoIt", Speed);
    }
	
}
