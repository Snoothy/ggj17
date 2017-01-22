using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour {

	private GameObject skybox;

	// Use this for initialization
	void Start () {
		skybox = GameObject.Find("Skybox");
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.LookAt(skybox.transform);
	}
}
