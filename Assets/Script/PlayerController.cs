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

    [Header("ジャンプ設定")]
    public float jumpForce = 12f;

    [Header("着地判定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        HandleJump();
    }

    // 物理演算と同じ更新タイミングにするため FixedUpdate で移動処理する
    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleMove()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        float targetSpeed = input * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x; // 目標速度との差分をもとに力を算出

        // 入力があれば加速、なければ減速
        float force = (Mathf.Abs(input) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(Vector2.right * speedDiff * force, ForceMode2D.Force);
    }

    void HandleJump()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // AddForce ではなく直接代入して毎回一定の跳躍力を保証する
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}
