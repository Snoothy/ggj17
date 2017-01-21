using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    private float rotation = 0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	    rotation += 75.0f * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation));
	}
}
