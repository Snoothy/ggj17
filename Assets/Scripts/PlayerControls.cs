using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public static event Action<Vector3, int> OnStomp;

    public int PlayerId = 0;
    public bool hasInput = true;
    public KeyCode JumpKey, StompKey;
    public Rigidbody rbody;
    public Vector3 jumpForce, stompForce;
    public float movespeed = 3;
    public float maxspeed = 3;

    public bool isJumping, isStomping, isGrounded;
    public bool isInTheAir { get { return isJumping || isStomping; } }
    private float lastJumpTimestamp = float.MinValue;

    public void Setup(int playerid)
    {
        PlayerId = playerid;
    }

    void Update()
    {
        if(hasInput)
        {
		    if(Input.GetKey(JumpKey))
            {
                DoJump();
            }

            if (Input.GetKey(StompKey))
            {
                DoStomp();
            }

            DoInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground")
        {
            if(isStomping)
            {
                PerformStomp(collision.contacts[0].point);
            }
            isGrounded = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground" && Time.time - lastJumpTimestamp > 0.25f)
        {
            isGrounded = true;
            isStomping = false;
            isJumping = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Start herer
        if(other.tag == "Stomp" && isGrounded)
        {
            PerformStompHit((transform.position - other.transform.position).normalized, other.GetComponent<Stomp>().force);
        }
    }

    public void DoJump()
    {
        if(!isJumping && isGrounded)
        {
            rbody.AddForce(jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            isJumping = true; 
            lastJumpTimestamp = Time.time;
        }
    }

    public void DoStomp()
    {
        if(!isStomping && !isGrounded)
        {
            rbody.AddForce(stompForce, ForceMode.VelocityChange);
            isGrounded = false;
            isJumping = false;
            isStomping = true;
        }
    }

    public void DoInput(float x, float y)
    {
        if(x != 0 || y != 0)
        {
            rbody.AddForce(new Vector3(x * movespeed, 0, y * movespeed) * (isInTheAir ? 0.25f : 1f), ForceMode.VelocityChange);
            if (!isInTheAir && rbody.velocity.magnitude > maxspeed)
            {
                rbody.velocity = rbody.velocity.normalized * maxspeed;
            }
        }
    }

    void PerformStomp(Vector3 position)
    {
        isStomping = false;
        if(OnStomp != null)
        {
            OnStomp(position, PlayerId);
        }
    }

    void PerformStompHit(Vector3 dir, float force)
    {
        rbody.AddForce(dir * force, ForceMode.VelocityChange);
    }
}
