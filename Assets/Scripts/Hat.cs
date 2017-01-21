using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hat : MonoBehaviour
{
    private List<SpriteRenderer> Hats;
    private int CurrentHat = 0;
	// Use this for initialization
	void Start ()
	{
	    Hats = gameObject.GetComponentsInChildren<SpriteRenderer>().ToList();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ClearHat()
    {
        foreach (var hat in Hats)
        {
            hat.enabled = false;
        }
    }

    public void SetHat(int id)
    {
        ClearHat();
        if (Hats.Count > id)
        {
            Hats[id].enabled = true;
            CurrentHat = id;
        }

    }

    public void NextHat()
    {
        SetHat((CurrentHat + 1) % Hats.Count);
    }

    public void PrevHat()
    {
        SetHat((CurrentHat - 1) % Hats.Count);
    }
}
