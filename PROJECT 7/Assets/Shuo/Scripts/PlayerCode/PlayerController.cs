using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float runSpeed = 6f;
    public float jumpSpeed = 12f;
    public float doubleJumpSpeed = 11f;

    [Header("SFX (One-Shots)")]
    public AudioSource sfx;                  // 跳跃/二段跳/落地用的OneShot
    public AudioClip jumpClip;
    public AudioClip doubleJumpClip;
    public AudioClip landClip;

    [Header("Footstep Loop (在地面移动时循环)")]
    public AudioSource footstepSource;       // 脚步循环专用（单独的AudioSource）
    public AudioClip footstepLoop;           // 一条循环脚步音（或环境步声）
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    [Tooltip("输入阈值：超过才算在移动")]
    public float moveThreshold = 0.1f;
    [Tooltip("速度阈值：超过才算在移动（避免轻微速度误判）")]
    public float minSpeedForSteps = 0.05f;
    [Tooltip("基础音调")]
    public float footstepBasePitch = 1.0f;
    [Tooltip("随速度浮动的音调范围（±）")]
    public float footstepPitchRange = 0.15f;

    [Header("Visual (只翻这个)")]
    public Transform gfx;
    private bool facingRight = true;

    [Header("Ground Check（勾选 Ground / OneWayPlatform 等）")]
    public LayerMask groundLayers;

    private Rigidbody2D myRigidbody;
    private Animator myAnim;
    private BoxCollider2D myFeet;

    private bool isGround;
    private bool canDoubleJump;
    private bool wasGround;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnim = GetComponentInChildren<Animator>();
        myFeet = GetComponent<BoxCollider2D>();

        if (!gfx)
        {
            var t = transform.Find("GFX");
            gfx = t ? t : transform;
        }

        // Ground 层兜底
        if (groundLayers.value == 0)
            groundLayers = LayerMask.GetMask("Ground", "OneWayPlatform");

        // ===== 配置脚步循环的 AudioSource =====
        if (!footstepSource)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
        }
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.spatialBlend = 0f; // 2D
        footstepSource.volume = footstepVolume;
        footstepSource.clip = footstepLoop; // 可以先不赋，下面逻辑会检查

        wasGround = true;
    }

    void Update()
    {
        Run();
        Flip();
        Jump();
        CheckGrounded();
        HandleLandingSFX();
        UpdateFootstepLoop();   // ← 改成循环控制
        SwitchAnimation();
    }

    void CheckGrounded()
    {
        isGround = myFeet.IsTouchingLayers(groundLayers);
    }

    void Flip()
    {
        float vx = myRigidbody.velocity.x;
        const float threshold = 0.05f;
        if (vx > threshold && !facingRight) SetFacing(true);
        else if (vx < -threshold && facingRight) SetFacing(false);
    }

    void SetFacing(bool right)
    {
        facingRight = right;
        var s = gfx.localScale;
        s.x = Mathf.Abs(s.x) * (right ? 1f : -1f);
        gfx.localScale = s;
    }

    void Run()
    {
        float moveDir = Input.GetAxis("Horizontal");
        float moveDirRaw = Input.GetAxisRaw("Horizontal");
        myRigidbody.velocity = new Vector2(moveDir * runSpeed, myRigidbody.velocity.y);

        bool playerHasXAxisSpeed = Mathf.Abs(moveDirRaw) > Mathf.Epsilon;
        myAnim.SetBool("Run", playerHasXAxisSpeed);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGround)
            {
                myAnim.SetBool("Jump", true);
                myAnim.SetBool("Ground", true);
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpSpeed);
                canDoubleJump = true;
                if (sfx && jumpClip) sfx.PlayOneShot(jumpClip);
            }
            else if (canDoubleJump)
            {
                myAnim.SetBool("DoubleJump", true);
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, doubleJumpSpeed);
                canDoubleJump = false;
                if (sfx && doubleJumpClip) sfx.PlayOneShot(doubleJumpClip);
            }
        }
    }

    void HandleLandingSFX()
    {
        if (isGround && !wasGround)
        {
            if (sfx && landClip) sfx.PlayOneShot(landClip);
        }
        wasGround = isGround;
    }

    // ========= 关键：脚步“循环播放/停止 + 随速度调音高” =========
    void UpdateFootstepLoop()
    {
        if (!footstepSource) return;

        float inputX = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        float speedX = Mathf.Abs(myRigidbody.velocity.x);
        bool movingOnGround = isGround && inputX > moveThreshold && speedX > minSpeedForSteps;

        if (movingOnGround && footstepLoop)
        {
            if (!footstepSource.isPlaying)
            {
                // 确保设置了clip再播放
                if (footstepSource.clip != footstepLoop) footstepSource.clip = footstepLoop;
                footstepSource.volume = footstepVolume;
                footstepSource.Play();
            }

            // 根据速度微调音高（更快=更高）
            float t = Mathf.Clamp01(speedX / Mathf.Max(0.001f, runSpeed));
            footstepSource.pitch = footstepBasePitch + (t - 0.5f) * 2f * footstepPitchRange;
        }
        else
        {
            // 停止或暂停（用Pause避免反复从头触发的咔嗒感）
            if (footstepSource.isPlaying) footstepSource.Pause();
        }
    }

    void SwitchAnimation()
    {
        myAnim.SetBool("Idle", false);

        if (myAnim.GetBool("Jump"))
        {
            if (myRigidbody.velocity.y < 0.0f)
            {
                myAnim.SetBool("Jump", false);
                myAnim.SetBool("Fall", true);
            }
        }
        else if (isGround)
        {
            myAnim.SetBool("Fall", false);
            myAnim.SetBool("Idle", true);
        }

        if (myAnim.GetBool("DoubleJump"))
        {
            if (myRigidbody.velocity.y < 0.0f)
            {
                myAnim.SetBool("DoubleJump", false);
                myAnim.SetBool("DoubleFall", true);
            }
        }
        else if (isGround)
        {
            myAnim.SetBool("DoubleFall", false);
            myAnim.SetBool("Idle", true);
        }
    }
}
