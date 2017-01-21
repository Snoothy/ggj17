using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour {

    static Vector3 tempV;
    static RaycastHit info;
    private void Update()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out info, 50, LayerMask.NameToLayer("Ground")))
        {
            tempV = info.point;
            tempV.y += 1.75f;
            transform.position = tempV;
        }
    }
}
