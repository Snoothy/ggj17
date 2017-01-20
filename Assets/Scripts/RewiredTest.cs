using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewiredTest : MonoBehaviour {

    private Rewired.Player player;

    // Use this for initialization
    void Start () {
		
	}

    void Awake()
    {
        player = Rewired.ReInput.players.GetPlayer("Player0"); // get the player by id
    }

    void Update()
    {

        if (player.GetAnyButton())
        {
            Debug.Log("Jump");
        };
    }
}
