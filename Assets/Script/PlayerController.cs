// =====================================================
// PlayerController.cs - プレイヤーの移動・ジャンプ制御
// 使い方: Player オブジェクトにアタッチする。
//         Rigidbody2D と、着地判定用の groundCheck 子オブジェクトが必要。
//         GameManager.IsPlaying が false の間はジャンプ入力を受け付けない。
// =====================================================
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 6f;
    public float acceleration = 40f;
    public float deceleration = 40f;

    [Header("ジャンプ設定（ジャンプキング方式）")]
    public float jumpVelocityX = 5f;
    public float jumpVelocityY = 12f;

    [Header("着地判定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isJumping;   // ジャンプ開始〜着地まで true。Update と FixedUpdate のタイミング差で
                              // 生じる「isGrounded が遅れて false になる」問題を回避するために使う
    private int facingDirection = 1; // +1: 右, -1: 左

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 下降中に着地したらジャンプ終了とみなす
        if (isJumping && isGrounded && rb.linearVelocity.y <= 0f)
            isJumping = false;

        HandleJump();
    }

    // 物理演算と同じ更新タイミングにするため FixedUpdate で移動処理する
    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleMove()
    {
        if (SceneTransitionManager.IsTransitioning) return;
        // isJumping は HandleJump と同フレームで立つため、FixedUpdate の AddForce を確実にブロックできる
        if (!isGrounded || isJumping) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        if (Mathf.Abs(input) > 0.01f)
            facingDirection = (int)Mathf.Sign(input);

        float targetSpeed = input * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x; // 目標速度との差分をもとに力を算出

        // 入力があれば加速、なければ減速
        float force = (Mathf.Abs(input) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(Vector2.right * speedDiff * force, ForceMode2D.Force);
    }

    void HandleJump()
    {
        if (SceneTransitionManager.IsTransitioning) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (!isGrounded || isJumping) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        isJumping = true;
        // 向いている方向に固定 velocity をセット（ジャンプキング方式）
        rb.linearVelocity = new Vector2(facingDirection * jumpVelocityX, jumpVelocityY);
    }
}
