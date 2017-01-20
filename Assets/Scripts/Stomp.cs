using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stomp : MonoBehaviour
{
    public float startScale = 0.1f, endScale = 5, scaleTime = 3;
    public AnimationCurve scaleCurve;
    public int belongsToPlayerId;
    public float force = 5;
     
    public void Init(int playerid)
    {
        belongsToPlayerId = playerid;
        GameObject.Destroy(this.gameObject, scaleTime);
        StartCoroutine(DoScale());
    }

    IEnumerator DoScale()
    {
        float timer = scaleTime;
        Vector3 scaleVector = Vector3.one;
        while(timer > 0)
        {
            timer -= Time.deltaTime;
            float norma = scaleCurve.Evaluate((scaleTime - timer) / scaleTime);
            float thingie = norma * endScale + (1f - norma) * startScale;
            scaleVector.x = thingie;
            scaleVector.z = thingie;
            transform.localScale = scaleVector;
            yield return null;
        }
        yield return null;
    }
}
