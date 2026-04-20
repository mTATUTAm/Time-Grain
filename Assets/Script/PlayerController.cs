using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    public float moveSpeed = 6f;        // �ō����x
    public float acceleration = 40f;    // ������
    public float deceleration = 40f;    // ������

    [Header("�W�����v�ݒ�")]
    public float jumpForce = 12f;       // �W�����v��

    [Header("�ڒn����")]
    public Transform groundCheck;       // �����̋�I�u�W�F�N�g
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;       // �n�ʂ̃��C���[

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // �ڒn����
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

    // ����������������������������������������������������������
    // ���E�ړ��i�����E��������j
    // ����������������������������������������������������������
    void HandleMove()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        float targetSpeed = input * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        // ���͂�����Ή����A�Ȃ���Ό���
        float force = (Mathf.Abs(input) > 0.01f) ? acceleration : deceleration;

        // velocity�ɒ��ډ��Z���Ċ��炩�ɕω�������
        rb.AddForce(Vector2.right * speedDiff * force, ForceMode2D.Force);
    }

    // ����������������������������������������������������������
    // �W�����v
    // ����������������������������������������������������������
    void HandleJump()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}