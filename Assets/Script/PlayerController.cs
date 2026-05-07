// =====================================================
// PlayerController.cs - プレイヤーの移動・ジャンプ制御（ジャンプキング方式）
// 使い方: Player オブジェクトにアタッチする。Rigidbody2D が必要。
//         地上のみ左右移動可。空中は移動・方向転換不可。
//         ジャンプは向いている方向に初速を直接セットする。
// =====================================================
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 6f;
    public float acceleration = 40f;
    public float deceleration = 40f;

    [Header("ジャンプ設定")]
    public float jumpVelocityX = 5f;
    public float jumpVelocityY = 12f;

    [Header("接触判定")]
    [Tooltip("この値以上の法線Y成分を持つ面のみ着地判定とする（0.7≒45度）")]
    public float minGroundNormalY = 0.7f;

    private Rigidbody2D rb;
    private bool  isGrounded;
    private float _facingX = 1f;  // 向き: +1=右, -1=左

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleMove()
    {
        if (!isGrounded) return;
        if (SceneTransitionManager.IsTransitioning) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A)) { input = -1f; _facingX = -1f; }
        if (Input.GetKey(KeyCode.D)) { input =  1f; _facingX =  1f; }

        float targetSpeed = input * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float force = (Mathf.Abs(input) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(Vector2.right * speedDiff * force, ForceMode2D.Force);
    }

    void HandleJump()
    {
        if (SceneTransitionManager.IsTransitioning) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (!isGrounded) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        rb.linearVelocity = new Vector2(_facingX * jumpVelocityX, jumpVelocityY);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y >= minGroundNormalY)
                isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
