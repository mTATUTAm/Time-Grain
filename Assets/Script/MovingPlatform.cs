// =====================================================
// MovingPlatform.cs - 時間操作に連動して動く足場
// 使い方: 動かしたい足場オブジェクトにアタッチし、speed と moveDirection を設定する。
//         BoardDeltaTime を使うため、時間逆行中は逆方向に動く。
//         プレイヤーが乗ると自動でペアレントされ、足場と一緒に動く。
// =====================================================
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 moveDirection = Vector3.up;

    private Transform _ridingPlayer = null;
    private bool _shouldDetach = false;

    void Update()
    {
        float dt = TimeManager.Instance.BoardDeltaTime;
        transform.Translate(moveDirection * speed * dt);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 法線が下向き = プレイヤーが上から乗った場合のみペアレントする
                if (contact.normal.y < -0.5f)
                {
                    _ridingPlayer = collision.transform;
                    _shouldDetach = false;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _shouldDetach = true;
        }
    }

    // LateUpdate でペアレント操作することで物理演算との競合を避ける
    void LateUpdate()
    {
        if (_shouldDetach && _ridingPlayer != null)
        {
            _ridingPlayer.SetParent(null);
            _ridingPlayer = null;
            _shouldDetach = false;
        }
        else if (_ridingPlayer != null && _ridingPlayer.parent != transform)
        {
            _ridingPlayer.SetParent(transform);
        }
    }
}