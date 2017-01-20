using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public static event Action<Vector3, int, Color> OnStomp;

    public int PlayerId = 0;
    public Rigidbody rbody;
    public Vector3 jumpForce, stompForce;
    public float movespeed = 3;
    public float maxspeed = 3;

    public MoveState state = MoveState.none;
    public bool isGrounded;
    public bool isInTheAir { get { return !isGrounded; } }
    private float lastJumpTimestamp = float.MinValue;
    private float lastStompTimestamp = float.MinValue;
    public float minJumpCharge = 0.1f;
    public float maxJumpCharge = 1;
    public float minStompCharge = 0.1f;
    public float maxStompCharge = 1;
    public float currentJumpForce = 0;
    public float currentStompForce = 0;
    public float stompStunTimeMax = 1.5f;

    private Rewired.Player player;

    public Renderer MyRenderer;

    public void Setup(int playerid)
    {
        PlayerId = playerid;
    }

    private void Start()
    {
        switch (PlayerId)
        {
            case 0: MyRenderer.material.color = Color.red; break;
            case 1: MyRenderer.material.color = Color.yellow; break;
            case 2: MyRenderer.material.color = Color.green; break;
            case 3: MyRenderer.material.color = Color.black; break;
            case 4: MyRenderer.material.color = Color.blue; break;
            case 5: MyRenderer.material.color = Color.grey; break;
        }
        transform.position += Vector3.right * PlayerId;
    }

    void Update()
    {
        player = Rewired.ReInput.players.GetPlayer("Player" + PlayerId); // get the player by id

        if (state == MoveState.afterStomp)
        {
            float minS = Mathf.Abs(minStompCharge * stompForce.y);
            float maxS = Mathf.Abs(maxStompCharge * stompForce.y);
            if (Time.time - lastStompTimestamp < stompStunTimeMax * ((Mathf.Abs(currentStompForce) - minS) / (maxS - minS)))
                return;
            else
                state = MoveState.none;
        }

        //cannot charge or move while we are resolving the hit we received
        if (state == MoveState.hit)
            return; 

        if (player.GetButtonDown("Jump"))
        {
            if (isGrounded)
                ChargeJump();
            else
                ChargeStomp();
        }

        if (player.GetButtonUp("Jump"))
        {
            if (isGrounded)
                DoJump();
            else
                DoStomp();
        }

        //cannot move while charging stuff
        if (state == MoveState.chargingJump || state == MoveState.chargingStomp)
            return;

        /*if (player.GetButtonDown("Stomp"))
        {
            DoStomp();
        }*/

        DoInput(player.GetAxis("Move Horizontal"), player.GetAxis("Move Vertically"));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground")
        {
            if(state == MoveState.stomping)
            {
                PerformStomp(collision.contacts[0].point);
                state = MoveState.afterStomp;
            }
            else
            {
                state = MoveState.none;
            }
            isGrounded = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isGrounded && collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground" && Time.time - lastJumpTimestamp > 0.25f)
        {
            isGrounded = true;
            if(state != MoveState.afterStomp)
                state = MoveState.none;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Start herer
        Stomp stomper = other.transform.parent.GetComponent<Stomp>();
        if (other.tag == "Stomp" && isGrounded && stomper != null && stomper.belongsToPlayerId != PlayerId)
        {
            PerformStompHit((transform.position - other.transform.position).normalized + Vector3.up * 0.3f, stomper.force);
        }
    }

    public void ChargeJump()
    {
        if(isGrounded && state != MoveState.chargingJump && state != MoveState.jumping)
        {
            state = MoveState.chargingJump;
            lastJumpTimestamp = Time.time;
        }
    }

    public void DoJump()
    {
        if (isGrounded && state == MoveState.chargingJump)
        {
            currentJumpForce = jumpForce.y * Mathf.Clamp(Time.time - lastJumpTimestamp, minJumpCharge, maxJumpCharge);
            rbody.AddForce(currentJumpForce * Vector3.up, ForceMode.VelocityChange);
            isGrounded = false;
            state = MoveState.jumping;
            lastJumpTimestamp = Time.time;
        }
    }

    public void ChargeStomp()
    {
        if(!isGrounded && state != MoveState.chargingStomp && state != MoveState.stomping)
        {
            state = MoveState.chargingStomp;
            lastStompTimestamp = Time.time;
        }
    }

    public void DoStomp()
    {
        if (!isGrounded && state == MoveState.chargingStomp)
        {
            currentStompForce = stompForce.y * Mathf.Clamp(Time.time - lastStompTimestamp, minStompCharge, maxStompCharge);
            rbody.AddForce(currentStompForce * Vector3.up, ForceMode.VelocityChange);
            state = MoveState.stomping;
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
        lastStompTimestamp = Time.time;
        state = MoveState.afterStomp;
        if(OnStomp != null)
        {
            OnStomp(position, PlayerId, MyRenderer.material.color);
        }
    }

    void PerformStompHit(Vector3 dir, float force)
    {
        rbody.AddForce(dir * force, ForceMode.Impulse);
        isGrounded = false;
        state = MoveState.hit;
        lastJumpTimestamp = Time.time;
    }
}

public enum MoveState { none, chargingJump, chargingStomp, stomping, jumping, hit, afterStomp }