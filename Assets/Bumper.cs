using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    public float force = 10f;
    public Animator anim; 

    public void DoBump()
    {
        anim.SetTrigger("bump");
    }
}
