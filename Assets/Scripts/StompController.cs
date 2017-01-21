using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompController : MonoBehaviour
{
    public GameObject stompPrefab;
    private void Start()
    {
        PlayerControls.OnStomp += PlayerControls_OnStomp;
    }

    private void PlayerControls_OnStomp(Vector3 pos, int playerid, Color color, float normalizedForce)
    {
        pos.y += 0.37f;
        GameObject go = (GameObject)GameObject.Instantiate(stompPrefab, pos, stompPrefab.transform.rotation);
        go.GetComponent<Stomp>().Init(playerid, color, normalizedForce);
    }
}
