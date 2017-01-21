//Copyright (c) 2016 Kai Clavier [kaiclavier.com] Do Not Distribute
using UnityEngine;
using System.Collections;

public class ScreenshakeSampleController : MonoBehaviour {

	void Update () {
		if(Input.GetKeyDown(KeyCode.F)){
			Camera.main.Shake();
		}
		if(Input.GetKeyDown(KeyCode.K)){
			Camera.main.Kick(Vector3.left);
		}
	}
}
