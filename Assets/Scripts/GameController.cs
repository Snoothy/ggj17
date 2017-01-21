using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject PlayerPrefab;
    private List<Rewired.Player> RePlayers;
    private Dictionary<int, PlayerControls> ActivePlayers = new Dictionary<int, PlayerControls>();

    private bool _gameStarted = false;

	// Use this for initialization
	void Start () {
	    	
	}

    void Awake()
    {
        RePlayers = (List<Player>) ReInput.players.GetPlayers();
    }
	
	// Update is called once per frame
	void Update () {
	    if (_gameStarted)
	    {

	    }
	    else
	    {
	        // Player selection
	        foreach (var rePlayer in RePlayers)
	        {
                // Join game
	            if (rePlayer.GetButtonDown("Jump") && !ActivePlayers.ContainsKey(rePlayer.id))
	            {
	                var player = CreatePlayer(rePlayer.id);
	            }

                // Changes hats TODO

                // Start game 
	            if (rePlayer.GetButtonDown("Start") && ActivePlayers.ContainsKey(rePlayer.id))
	            {
	                // TODO check if players ready
	                GameStart();
	            }
	        }
	    }

	}

    private PlayerControls CreatePlayer(int id)
    {
        var prefab = Instantiate(PlayerPrefab, SpawnPoint);
        var controls = prefab.GetComponent<PlayerControls>();
        ActivePlayers.Add(id, controls);
        return controls;
    }

    private void GameStart()
    {
        // TODO
        _gameStarted = true;
    }

    public List<PlayerControls> GetJoinedPlayers()
    {
        return ActivePlayers.Select(player => player.Value).ToList();
    }

    public bool IsGameStarted()
    {
        return _gameStarted;
    }

}
