using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    private float rotation = 0f;
	public float speed = 75.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	    rotation += speed * Time.deltaTime;
		transform.localRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, rotation, transform.rotation.eulerAngles.z));
	}
}
