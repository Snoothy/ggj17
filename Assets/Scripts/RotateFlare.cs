using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFlare : MonoBehaviour
{
    private float rotation = 0f;
    public float speed = 75.0f;
    public float x = 0f;
    public float y = 0f;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        rotation += speed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(new Vector3(x, y, rotation));
    }
}