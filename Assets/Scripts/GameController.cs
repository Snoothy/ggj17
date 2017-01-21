using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static event Action GameStateChanged;

    public Transform SpawnPoint;
    public GameObject PlayerPrefab;
    private List<Rewired.Player> RePlayers = new List<Rewired.Player>();
    private Dictionary<int, PlayerControls> ActivePlayers = new Dictionary<int, PlayerControls>();

    private bool _gameStarted = false;
    private bool _gameIsStarting = false;
    private bool _gameOver = false;
    private bool _gameEnded = false;

    private Coroutine WinnerRoutine;

    public bool IsGameStarted { get { return _gameStarted; } }
    public bool IsGameStarting { get { return _gameIsStarting; } }
    public bool IsGameOver { get { return _gameOver; } }

	// Use this for initialization
	void Start () {
	    	
	}


    void Awake()
    {
        foreach (var player in ReInput.players.GetPlayers(false))
        {
            RePlayers.Add(player);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameStarted || _gameIsStarting || _gameEnded)
        {
            // Someone won
            if (AlivePlayers().Count <= 1)
            {
                PlayerControls winner = null;

                if (!_gameOver)
                {
                    winner = AlivePlayers().First();
                    winner.Win();
                    winner.DisablePlayer();
                    _gameOver = true;
                    if (winner.GetWins >= 3) _gameEnded = true;
                    WinnerRoutine = StartCoroutine(MoveWinner(winner.gameObject));

                    if (GameStateChanged != null)
                        GameStateChanged();
                }
            }
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
                    SoundManager.Instance.PlaySound(SoundManager.Instance.acSelect);
                }

                // Show who you are
                if (rePlayer.GetButtonDown("Jump") && ActivePlayers.ContainsKey(rePlayer.id))
                {
                    // TODO
                }

                // Leave
                if (rePlayer.GetButtonDown("Leave") && ActivePlayers.ContainsKey(rePlayer.id))
                {
                    Destroy(ActivePlayers[rePlayer.id].gameObject);
                    ActivePlayers.Remove(rePlayer.id);

                    // Update positions
                    var i = 0;
                    foreach (var player in ActivePlayers)
                    {
                        SetPlayerPosition(player.Value.gameObject, i);
                        i++;
                    }
                }

                // Changes hats
                if (rePlayer.GetNegativeButtonDown("Left") && ActivePlayers.ContainsKey(rePlayer.id))
                {
                    ActivePlayers[rePlayer.id].PrevHat();
                }

                if (rePlayer.GetButtonDown("Right") && ActivePlayers.ContainsKey(rePlayer.id))
                {
                    ActivePlayers[rePlayer.id].NextHat();
                }

                // Start game with more than 1 player
                if (rePlayer.GetButtonDown("Start") && ActivePlayers.ContainsKey(rePlayer.id) && ActivePlayers.Count > 1)
                {
                    // TODO check if players ready
                    StartCoroutine(GameStart());
                }
            }
        }

    }

    private List<PlayerControls> AlivePlayers()
    {
        var w = ActivePlayers.Where(player => player.Value.IsAlive).ToList();
        return w.Select(player => player.Value).ToList();
    }

    private PlayerControls CreatePlayer(int id)
    {
        var prefab = Instantiate(PlayerPrefab, SpawnPoint);
        var controls = prefab.GetComponent<PlayerControls>();
        controls.Setup(id, (PlayerColor)id, this);
        ActivePlayers.Add(id, controls);

        // Set position
        var i = 0;
        foreach (var player in ActivePlayers)
        {
            var go = player.Value.gameObject;
            SetPlayerPosition(go, i);
            i++;
        }
        return controls;
    }

    /// <summary>
    /// Sets the player join position
    /// </summary>
    /// <param name=""></param>
    /// <param name="t"></param>
    /// <param name="index"></param>
    private void SetPlayerPosition(GameObject t, int index)
    {
        var offsetRatio = 2.5;
        var offset = index * offsetRatio - offsetRatio * ActivePlayers.Count / 2.0f + offsetRatio / 2.0f;
        var prefabTransform = SpawnPoint.GetComponent<Transform>();
        t.GetComponent<Transform>().position = new Vector3(
            prefabTransform.position.x + (float)offset,
            prefabTransform.position.y - 2,
            prefabTransform.position.z
            );
    }

    IEnumerator GameStart()
    {
        _gameIsStarting = true;

        // Moves players to center

        if (GameStateChanged != null)
            GameStateChanged();

        // TODO  move players to center
        yield return StartCoroutine(CenterPlayers());

        foreach (var player in ActivePlayers)
        {
            player.Value.EnablePlayer();
        }

        // Explode spawn
        Explode();
        
        _gameStarted = true;
        if (GameStateChanged != null)
            GameStateChanged();
    }

    IEnumerator ResetGame(PlayerControls winner)
    {
        // Clean up if game ended
        if (_gameEnded)
        {
            if (ActivePlayers.Count > 0)
            {
                foreach (var player in ActivePlayers)
                {
                    Destroy(ActivePlayers[player.Value.PlayerId].gameObject);
                }
                ActivePlayers = new Dictionary<int, PlayerControls>();
            }

            _gameStarted = false;
            _gameEnded = false;
        }
        else
        {
            var i = 0;
            foreach (var player in ActivePlayers)
            {
                player.Value.Reset();
                player.Value.DisablePlayer();
                SetPlayerPosition(player.Value.gameObject, i);
                SoundManager.Instance.PlaySound(SoundManager.Instance.acSelect);
                i++;
                yield return new WaitForSeconds(0.5f);
            }

            // Next round
            StartCoroutine(GameStart());
        }
        
        _gameIsStarting = false;
        _gameOver = false;

        if (GameStateChanged != null)
            GameStateChanged();
        yield return null;
    }

    private void Explode()
    {
        var rads = Mathf.Deg2Rad * (((360.0f / ActivePlayers.Count) + Random.Range(0.0f, 360.0f) ) % 360.0f);
        var force = 10.0f;
        int i = 0;
        foreach (var player in ActivePlayers)
        {
            var rb = player.Value.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(Mathf.Cos(rads * i), 0.5f, Mathf.Sin(rads * i)) * force, ForceMode.Impulse);
            i++;
        }
    }

    IEnumerator CenterPlayers()
    {
        var startLocations = new List<Vector3>();
        var transforms = new List<Transform>();
        var startTime = Time.time;
        var duration = 2.0f;
        var endTime = startTime + duration;
        foreach (var player in ActivePlayers)
        {
            var playerTransform = player.Value.gameObject.GetComponent<Transform>();
            transforms.Add(playerTransform);
            startLocations.Add(playerTransform.position);
        }

        while (Time.time < endTime)
        {
            float fracComplete = (Time.time - startTime) / duration;
            var i = 0;
            foreach (var player in ActivePlayers)
            {
                player.Value.gameObject.GetComponent<Transform>().position =
                    Vector3.Slerp(startLocations[i],
                    SpawnPoint.position,
                    fracComplete);
                i++;
            }
            yield return null;
        }
        yield return null;
    }

    IEnumerator MoveWinner(GameObject winner)
    {
        var startLocation = winner.transform.position;
        var transforms = winner.transform;
        var startTime = Time.time;
        var duration = 2.0f;
        var endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float fracComplete = (Time.time - startTime) / duration;

            transforms.position =
                Vector3.Slerp(startLocation,
                SpawnPoint.position+Vector3.up*1.0f,
                fracComplete);

            yield return null;
        }

        // TODO confetti and sounds
        yield return new WaitForSeconds(3.0f);
        StartCoroutine(ResetGame(winner.GetComponent<PlayerControls>()));

        yield return null;
    }

    public List<PlayerControls> GetJoinedPlayers()
    {
        return ActivePlayers.Select(player => player.Value).ToList();
    }
}
