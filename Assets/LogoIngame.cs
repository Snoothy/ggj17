using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoIngame : MonoBehaviour
{
    public GameController gc;
    public GameObject ToActivate; 

	void Update ()
    {
        if (gc.GetActivePlayerCount == 0 && !ToActivate.activeSelf)
            ToActivate.SetActive(true);
        else if (gc.GetActivePlayerCount > 0 && ToActivate.activeSelf)
            ToActivate.SetActive(false);
    }
}
