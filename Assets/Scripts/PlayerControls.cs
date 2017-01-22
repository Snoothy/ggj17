using System;
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
    public float maxspeed = 3, maxspeed_vertical = 50;

    private MoveState _stateInner = MoveState.none;
    private GameController GameController;
    private int Wins = 0;
    private bool Ready = false;
    public int GetWins { get { return Wins; } }
    public bool IsReady { get { return Ready; } }

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
    private Hat Hats;
    private Transform readyContainer;
    public Renderer MyRenderer;
    public SpriteRenderer MyFace;
    public Sprite FaceNormal, FaceStunned, FacePound, FaceCharging, FaceJump;
    public ParticleSystem psystem;

    public Screenshake screenshaker;

    public Color color { get { return MyRenderer.material.color; } }
    public PlayerColor playercolor = PlayerColor.green;

    public Animator anim;

    public bool IsAlive = true;

    public Color c1 = Color.white;
    public Color c2 = new Color(1, 1, 1, 0);

        LineRenderer lineRenderer;

    public void Setup(int playerid, PlayerColor mycolor, GameController gameController)
    {
        DisablePlayer();
        GameController = gameController;
        PlayerId = playerid;
        SetColor(mycolor);

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.startColor = MyRenderer.material.color;
        lineRenderer.endColor = MyRenderer.material.color;
        lineRenderer.numPositions = 2;

        readyContainer = transform.FindChild("Ready");

        // Hats 
        var hatRes = Resources.Load("prefabs/Hat");
        var hatGo = (GameObject)Instantiate(hatRes);
        var playerSprite = gameObject.transform.FindChild("PlayerSprite");
        hatGo.transform.parent = playerSprite;
        hatGo.transform.localPosition = new Vector3(0.0f, 2.5f, -0.01f);
        Hats = hatGo.GetComponent<Hat>();
        Hats.SetHat(playerid + 1);
        ShowReady();
        SetReady(false);

        ResetWins();
        
    }

    public void Reset()
    {
        IsAlive = true;
        rbody.velocity = Vector3.zero;
        state = MoveState.hit;

        var playerSprite = gameObject.transform.FindChild("PlayerSprite");
        playerSprite.FindChild("winnerflare").GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Die()
    {
        if (IsAlive)
        {
            IsAlive = false;
            SoundManager.Instance.PlaySound(SoundManager.Instance.scExplode);
        }
    }

    public void SetReady(bool state)
    {
        Ready = state;
        var aSprite = readyContainer.FindChild("a").GetComponent<SpriteRenderer>();
        var readySprite = readyContainer.FindChild("ready").GetComponent<SpriteRenderer>();
        if (Ready)
        {
            readySprite.enabled = true;
            aSprite.enabled = false;
        }
        else
        {
            readySprite.enabled = false;
            aSprite.enabled = true;
        }
    }

    public void HideReady()
    {
        readyContainer.gameObject.SetActive(false);
    }

    public void ShowReady()
    {
        readyContainer.gameObject.SetActive(true);
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

    public void Win()
    {
        Wins++;
        var playerSprite = gameObject.transform.FindChild("PlayerSprite");
        playerSprite.FindChild("winnerflare").GetComponent<SpriteRenderer>().enabled = true;
        switch (Wins)
        {
            case 1:
                playerSprite.FindChild("trophy1").GetComponent<SpriteRenderer>().enabled = true;
                break;
            case 2:
                playerSprite.FindChild("trophy2").GetComponent<SpriteRenderer>().enabled = true;
                break;
            case 3:
                playerSprite.FindChild("crown").GetComponent<SpriteRenderer>().enabled = true;
                break;
        }
    }

    public void ResetWins()
    {
        Wins = 0;
        var playerSprite = gameObject.transform.FindChild("PlayerSprite");
        playerSprite.FindChild("trophy1").GetComponent<SpriteRenderer>().enabled = false;
        playerSprite.FindChild("trophy2").GetComponent<SpriteRenderer>().enabled = false;
        playerSprite.FindChild("crown").GetComponent<SpriteRenderer>().enabled = false;
        playerSprite.FindChild("winnerflare").GetComponent<SpriteRenderer>().enabled = false;
    }

    public void EnablePlayer()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        HideReady();
    }

    public void DisablePlayer()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        if (!GameController.IsGameStarted || !IsAlive)
        {

            return;
        }
        player = Rewired.ReInput.players.GetPlayer("Player" + PlayerId); // get the player by id

        // linerenderer
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            RaycastHit info;
            float range = maxStompRange;
            if (Physics.Raycast(transform.position, Vector3.down, out info, 50, 1 << LayerMask.NameToLayer("Ground")))
            {
                lineRenderer.SetPosition(1, info.point);
            }
            else
            {
                lineRenderer.SetPosition(1, transform.position + Vector3.down * 20.0f);
            }
            

        }

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
        else if (collision.collider.tag == "Bumper")
        {
            Bumper bump = collision.transform.GetComponent<Bumper>();
            if (bump != null)
            {
                Vector3 val = (transform.position - collision.contacts[0].point).normalized + Vector3.up * 0.3f + new Vector3(0, -rbody.velocity.y / 10, 0);
                rbody.velocity = Vector3.zero;
                if (isGrounded || state == MoveState.hit)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.scHit);
                    val += val * 0.5f + Vector3.up;
                    rbody.AddForce(val * bump.force, ForceMode.VelocityChange);
                    state = MoveState.hit;
                    lastJumpTimestamp = Time.time;
                }
                else
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.scBack);
                    rbody.AddForce(val * bump.force, ForceMode.VelocityChange);
                    state = MoveState.jumping;
                    lastJumpTimestamp = Time.time;
                }
            }
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
        if (other.tag == "Stomp")
        {
            Stomp stomper = other.transform.parent.GetComponent<Stomp>();
            if (stomper != null && stomper.color != color && IsHittable && AmIOnRim(other, stomper))
            {
                PerformStompHit((transform.position - other.transform.position).normalized + Vector3.up * 0.3f, stomper.force);
            }
        }
        else if (other.tag == "KillZone")
        {
            Die();
        }
    }

    bool AmIOnRim(Collider other, Stomp stomper)
    {
        Vector3 centerStomp = other.transform.position;
        float stompRadius = other.transform.parent.localScale.z / 2f; //or x for that matter
        Vector3 distanceToCenter = (centerStomp - transform.position);
        distanceToCenter.y = 0;
        float actualDistToCenter = distanceToCenter.magnitude;
        float actualDistToEdge = stompRadius - actualDistToCenter;
        if (actualDistToEdge > 1.5f)
        {
            Debug.Log("OMG actualDistToCenter: " + actualDistToCenter + "  actualDistToEdge:" + actualDistToEdge);
            return false;
        }
        Debug.Log("actualDistToCenter: " + actualDistToCenter + "  actualDistToEdge:" + actualDistToEdge);
        return true;
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
            SoundManager.Instance.PlaySound(SoundManager.Instance.scJump);
        }
    }

    IEnumerator SpawnDust(float delay, Vector3 pos)
    {
        yield return new WaitForSeconds(delay);
        GameObject.Instantiate(DustPrefab, pos + Vector3.down * 0.3f, DustPrefab.transform.rotation);
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
        if (Hats != null) Hats.NextHat();
    }

    public void PrevHat()
    {
        if (Hats != null) Hats.PrevHat();
    }

    public void DoStomp()
    {
        if (!isGrounded && state == MoveState.jumping)// && state == MoveState.chargingStomp)
        {
            RaycastHit info;
            float range = maxStompRange;
            if (Physics.Raycast(transform.position, Vector3.down, out info, 50, 1 << LayerMask.NameToLayer("Ground")))
            {
                range = Mathf.Abs(transform.position.y - info.point.y - 0.5f);
                //currentStompForce = stompForce.y * Mathf.Clamp(Time.time - lastStompTimestamp, minStompCharge, maxStompCharge);
            }
            currentStompForce = (range / maxStompRange) * stompForce.y;
            state = MoveState.prepareStomp;
            StartCoroutine(StompAfterDelay());
            SoundManager.Instance.PlaySound(SoundManager.Instance.scStompBegin);
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
            tempInputV = rbody.velocity;
            if (Mathf.Abs(tempInputV.y) > maxspeed_vertical)
            {
                tempInputV.y = tempInputV.y > 0 ? maxspeed_vertical : -maxspeed_vertical;
                rbody.velocity = tempInputV;
            }
            /*if (!isInTheAir && rbody.velocity.magnitude > maxspeed)
            {
                rbody.velocity = rbody.velocity.normalized * maxspeed;
            }*/
            if (state == MoveState.none)
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
        SoundManager.Instance.PlaySound(SoundManager.Instance.scStomp);
    }

    void PerformStompHit(Vector3 dir, float force)
    {
        rbody.velocity = Vector3.zero;
        rbody.AddForce(dir * force, ForceMode.VelocityChange);
        isGrounded = false;
        state = MoveState.hit;
        lastJumpTimestamp = Time.time;
        SoundManager.Instance.PlaySound(SoundManager.Instance.scHit);
        if (rbody.velocity.magnitude > 6)
            SoundManager.Instance.PlaySound(SoundManager.Instance.scHardHit);
    }
}

public enum MoveState { none, chargingJump, chargingStomp, stomping, jumping, hit, afterStomp, prepareStomp }
public enum PlayerColor { /*black, white,*/ green, red, yellow, blue, orange, magenta, teal, pink }