using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoHover : MonoBehaviour
{
    public Vector3 startV;
    public Vector3 hoverV;
    public float speed = 1;
    RectTransform rt; 
    private void Start()
    {
        rt = this.GetComponent<RectTransform>();
    }

    private void Update()
    {
        rt.anchoredPosition = startV + Mathf.Sin(Time.time * speed) * hoverV;
    }
}
