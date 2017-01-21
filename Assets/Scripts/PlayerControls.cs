﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public static event Action<Vector3, int, Color, float> OnStomp;

    public int PlayerId = 0;
    public Rigidbody rbody;
    public Vector3 jumpForce, stompForce;
    public float movespeed = 3;
    public float maxspeed = 3;

    private MoveState _stateInner = MoveState.none;
    private GameController GameController;

    public MoveState state
    {
        get { return _stateInner; }
        set
        {
            _stateInner = value;
            switch (_stateInner)
            {
                case MoveState.chargingJump:
                case MoveState.chargingStomp:
                    MyFace.sprite = FaceCharging;
                    break;
                case MoveState.jumping:
                    MyFace.sprite = FaceJump;
                    anim.SetTrigger("jump");
                    anim.ResetTrigger("landed");
                    break;
                case MoveState.hit:
                case MoveState.afterStomp:
                    MyFace.sprite = FaceStunned;
                    anim.SetTrigger("stomped");
                    break;
                case MoveState.prepareStomp:
                case MoveState.stomping:
                    MyFace.sprite = FacePound;
                    anim.SetTrigger("willPound");
                    break;
                case MoveState.none:
                    MyFace.sprite = FaceNormal;
                    anim.SetTrigger("landed");
                    //anim.ResetTrigger("landed");
                    anim.ResetTrigger("willPound");
                    anim.ResetTrigger("stomped");
                    anim.ResetTrigger("jump");
                    break;
            }
        }
    }

    RaycastHit info_hittable;
    [Tooltip("Used for calculating if players can be hit by waves before/after grounded")]
    public float hittableAirDistance = 2.5f;
    public bool IsHittable
    {
        get
        {
            if (isGrounded)
                return true;
            if (Time.time - lastGroundedTime < hitAfterGrounded)
                return true;
            if (Physics.Raycast(transform.position, Vector3.down, out info_hittable, 2.5f, 1 << LayerMask.NameToLayer("Ground")))
                return true;
            return false;
        }
    }
    public bool isGrounded;
    float lastGroundedTime = 0;
    Vector3 groundedPoint = Vector3.zero;
    public float hitAfterGrounded = 0.3f;
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

    public float maxStompRange = 10;
    public float stompDelay = 0.3f;

    public GameObject DustPrefab;

    private Rewired.Player player;

    public Renderer MyRenderer;
    public SpriteRenderer MyFace;
    public Sprite FaceNormal, FaceStunned, FacePound, FaceCharging, FaceJump;
    public ParticleSystem psystem;

    public Screenshake screenshaker;

    public Color color { get { return MyRenderer.material.color; } }
    public PlayerColor playercolor = PlayerColor.green;

    public Animator anim;

    public bool IsAlive = true;

    public void Setup(int playerid, PlayerColor mycolor, GameController gameController)
    {
        DisablePlayer();
        GameController = gameController;
        PlayerId = playerid;
        SetColor(mycolor);
    }

    public void Reset()
    {
        IsAlive = true;
        rbody.velocity = Vector3.zero;
    }

    public void Die()
    {
        if(!IsAlive)
        {
            IsAlive = false;
            //SoundManager.Instance.PlaySound(SoundManager.Instance.ac)
        }
    }

    static int TEMPVAR = 1;
    private void Start()
    {
        screenshaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Screenshake>();
        //transform.position += Vector3.right * PlayerId;
    }

    private void SetColor(PlayerColor mycolor)
    {
        playercolor = mycolor;
        switch (playercolor)
        {
            case PlayerColor.blue: MyRenderer.material.color = Color.blue; break;
            case PlayerColor.green: MyRenderer.material.color = Color.green; break;
            case PlayerColor.orange: MyRenderer.material.color = new Color(255, 176, 32); break;
            case PlayerColor.pink: MyRenderer.material.color = new Color(255, 170, 207); break;
            case PlayerColor.magenta: MyRenderer.material.color = Color.magenta; break;
            case PlayerColor.red: MyRenderer.material.color = Color.red; break;
            case PlayerColor.teal: MyRenderer.material.color = new Color(2, 250, 255); break;
            case PlayerColor.yellow: MyRenderer.material.color = Color.yellow; break;
        }
    }

    public void EnablePlayer()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }

    public void DisablePlayer()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        if (!GameController.IsGameStarted())
        {

            return;
        }
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

        if (state == MoveState.prepareStomp)
        {
            rbody.velocity = Vector3.zero;
        }

        if (player.GetButtonDown("Jump"))
        {
            if (isGrounded)
                //ChargeJump();
                DoJump();
            else
                DoStomp();
            //else
            //   ChargeStomp();
        }

        /*if (player.GetButtonUp("Jump"))
        {
            if (isGrounded)
                DoJump();
            //else
            //    DoStomp();
        }*/

        //cannot move while charging stuff
        if (/*state == MoveState.chargingJump ||*/ state == MoveState.chargingStomp)
            return;

        /*if (player.GetButtonDown("Stomp"))
        {
            DoStomp();
        }*/

        DoInput(player.GetAxis("Move Horizontal"), player.GetAxis("Move Vertically"), state == MoveState.chargingJump);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground")
        {
            groundedPoint = transform.position;
            groundedPoint.y = collision.collider.transform.FindChild("UpperPoint").transform.position.y;
            if (state == MoveState.stomping)
            {
                PerformStomp(groundedPoint);
                state = MoveState.afterStomp;
            }
            else
            {
                state = MoveState.none;
            }
            isGrounded = true;
            lastGroundedTime = Time.time;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isGrounded && collision.contacts[0].point.y < transform.position.y && collision.collider.tag == "Ground" && Time.time - lastJumpTimestamp > 0.25f)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            groundedPoint = transform.position;
            groundedPoint.y = collision.collider.transform.FindChild("UpperPoint").transform.position.y;
            //groundedPoint = collision.collider.ClosestPointOnBounds(collision.contacts[0].point);
            state = MoveState.none;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Stomp stomper = other.transform.parent.GetComponent<Stomp>();
        if (other.tag == "Stomp" && stomper != null && stomper.color != color && IsHittable)
        {
            PerformStompHit((transform.position - other.transform.position).normalized + Vector3.up * 0.3f, stomper.force);
        }
    }

    /*public void ChargeJump()
    {
        if(isGrounded && state != MoveState.chargingJump && state != MoveState.jumping)
        {
            state = MoveState.chargingJump;
            lastJumpTimestamp = Time.time;
        }
    }*/

    public void DoJump()
    {
        if (isGrounded)// && state == MoveState.chargingJump)
        {
            //currentJumpForce = jumpForce.y * Mathf.Clamp(Time.time - lastJumpTimestamp, minJumpCharge, maxJumpCharge);
            currentJumpForce = jumpForce.y * maxJumpCharge;
            rbody.AddForce(currentJumpForce * Vector3.up, ForceMode.VelocityChange);
            isGrounded = false;
            state = MoveState.jumping;
            lastJumpTimestamp = Time.time;
            StartCoroutine(SpawnDust(0.05f, transform.position));
            SoundManager.Instance.PlaySound(SoundManager.Instance.acJump);
        }
    }

    IEnumerator SpawnDust(float delay, Vector3 pos)
    {
        yield return new WaitForSeconds(delay);
        GameObject.Instantiate(DustPrefab, pos, DustPrefab.transform.rotation);
    }

    /*public void ChargeStomp()
    {
        if(!isGrounded && state != MoveState.chargingStomp && state != MoveState.stomping)
        {
            state = MoveState.chargingStomp;
            lastStompTimestamp = Time.time;
        }
    }*/

    public void NextHat()
    {
        // TODO
    }

    public void PrevHat()
    {
        // TODO
    }

    public void DoStomp()
    {
        if (!isGrounded && state == MoveState.jumping)// && state == MoveState.chargingStomp)
        {
            RaycastHit info;
            if (Physics.Raycast(transform.position, Vector3.down, out info, 50, 1 << LayerMask.NameToLayer("Ground")))
            {
                float range = Mathf.Abs(transform.position.y - info.point.y - 0.5f);
                //currentStompForce = stompForce.y * Mathf.Clamp(Time.time - lastStompTimestamp, minStompCharge, maxStompCharge);
                currentStompForce = range / maxStompRange * stompForce.y;
                state = MoveState.prepareStomp;
                StartCoroutine(StompAfterDelay());
            }
            SoundManager.Instance.PlaySound(SoundManager.Instance.acStompBegin);
        }
    }

    IEnumerator StompAfterDelay()
    {
        float timer = stompDelay;
        Transform t = transform;
        Vector3 tpos = t.position;
        Vector3 tposAdd = Vector3.up * 0.25f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            t.position = tpos + (1f - timer / stompDelay) * tposAdd;
            yield return null;
        }

        rbody.AddForce(currentStompForce * Vector3.up, ForceMode.VelocityChange);
        state = MoveState.stomping;
    }

    Vector3 tempInputV;
    public void DoInput(float x, float y, bool ischargingjump)
    {
        if (x != 0 || y != 0)
        {
            rbody.AddForce(new Vector3(x, 0, y) * movespeed * (isInTheAir ? 0.25f : 1f) * (ischargingjump ? 0.8f : 1f), ForceMode.VelocityChange);
            tempInputV = rbody.velocity;
            tempInputV.y = 0;
            if (tempInputV.magnitude > maxspeed)
            {
                tempInputV = tempInputV.normalized * maxspeed;
                tempInputV.y = rbody.velocity.y;
                rbody.velocity = tempInputV;
            }
            /*if (!isInTheAir && rbody.velocity.magnitude > maxspeed)
            {
                rbody.velocity = rbody.velocity.normalized * maxspeed;
            }*/
            if(state == MoveState.none)
                anim.SetBool("isWalking", true);
            else
                anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    void PerformStomp(Vector3 position)
    {
        lastStompTimestamp = Time.time;
        state = MoveState.afterStomp;
        screenshaker.Shake(currentStompForce / stompForce.y);
        if (OnStomp != null)
        {
            OnStomp(position, PlayerId, MyRenderer.material.color, currentStompForce / stompForce.y);
        }
        psystem.Emit(20);
        SoundManager.Instance.PlaySound(SoundManager.Instance.acStomp);
    }

    void PerformStompHit(Vector3 dir, float force)
    {
        rbody.velocity = Vector3.zero;
        rbody.AddForce(dir * force, ForceMode.VelocityChange);
        isGrounded = false;
        state = MoveState.hit;
        lastJumpTimestamp = Time.time;
        SoundManager.Instance.PlaySound(SoundManager.Instance.acHit);
        if(rbody.velocity.magnitude > 8)
            SoundManager.Instance.PlaySound(SoundManager.Instance.acHardHit);
    }
}

public enum MoveState { none, chargingJump, chargingStomp, stomping, jumping, hit, afterStomp, prepareStomp }
public enum PlayerColor { /*black, white,*/ green, red, yellow, blue, orange, magenta, teal, pink }