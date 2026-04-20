using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 moveDirection = Vector3.up;

    void Update()
    {
        float dt = TimeManager.Instance.BoardDeltaTime;
        transform.Translate(moveDirection * speed * dt);
    }

    // ─────────────────────────────
    // プレイヤーが乗ったら子オブジェクトにする
    // ─────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 上から乗ったときだけ追従（横や下からの接触は無視）
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    collision.transform.SetParent(transform);
                    break;
                }
            }
        }
    }

    // ─────────────────────────────
    // プレイヤーが離れたら子オブジェクトを解除
    // ─────────────────────────────
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}