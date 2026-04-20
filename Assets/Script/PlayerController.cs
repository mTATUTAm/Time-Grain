using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 6f;        // 最高速度
    public float acceleration = 40f;    // 加速力
    public float deceleration = 40f;    // 減速力

    [Header("ジャンプ設定")]
    public float jumpForce = 12f;       // ジャンプ力

    [Header("接地判定")]
    public Transform groundCheck;       // 足元の空オブジェクト
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;       // 地面のレイヤー

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 接地判定
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    // ─────────────────────────────
    // 左右移動（加速・減速あり）
    // ─────────────────────────────
    void HandleMove()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        float targetSpeed = input * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        // 入力があれば加速、なければ減速
        float force = (Mathf.Abs(input) > 0.01f) ? acceleration : deceleration;

        // velocityに直接加算して滑らかに変化させる
        rb.AddForce(Vector2.right * speedDiff * force, ForceMode2D.Force);
    }

    // ─────────────────────────────
    // ジャンプ
    // ─────────────────────────────
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}