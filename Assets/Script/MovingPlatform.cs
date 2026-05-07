// =====================================================
// MovingPlatform.cs - 時間操作に連動して動く足場
// 使い方: 動かしたい足場オブジェクトにアタッチし、moveDirection・moveSpeed・rotationSpeed を設定する。
//         BoardDeltaTime を毎フレーム加算するため、逆行中は自動で逆方向に動く。
//         プレイヤーが乗った際は子オブジェクト化せず位置追従させるため、回転しても変形しない。
// =====================================================
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IBoardSaveable
{
    [Header("平行移動")]
    [Tooltip("移動方向 (正規化不要)")]
    public Vector2 moveDirection = Vector2.zero;
    [Tooltip("移動速度 (units/秒)")]
    [Min(0f)] public float moveSpeed = 2f;

    [Header("回転")]
    [Tooltip("回転速度 (度/秒、正=反時計回り)")]
    public float rotationSpeed = 0f;

    private Rigidbody2D _ridingPlayerRb;
    private bool _shouldDetach = false;

    // 移動前のプラットフォーム状態とプレイヤーオフセット
    private float _prevRotationZ;
    private Vector3 _playerOffsetBeforeMove;

    void Update()
    {
        if (TimeManager.Instance.IsStartupLocked) return;

        float dt = TimeManager.Instance.BoardDeltaTime;

        // 移動前にプレイヤーのオフセット（プラットフォーム基準のワールド座標差）を記録
        if (_ridingPlayerRb != null)
            _playerOffsetBeforeMove = (Vector3)_ridingPlayerRb.position - transform.position;

        _prevRotationZ = transform.eulerAngles.z;

        if (moveDirection != Vector2.zero)
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * dt);

        if (rotationSpeed != 0f)
            transform.Rotate(0f, 0f, rotationSpeed * dt);
    }

    public object SaveState() => new State
    {
        position = transform.position,
        rotationZ = transform.eulerAngles.z
    };

    public void LoadState(object state)
    {
        var s = (State)state;
        transform.position = s.position;
        transform.rotation = Quaternion.Euler(0f, 0f, s.rotationZ);
    }

    private struct State
    {
        public Vector3 position;
        public float rotationZ;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 法線が下向き = プレイヤーが上から乗った場合のみ追従させる
                if (contact.normal.y < -0.5f)
                {
                    _ridingPlayerRb = collision.rigidbody;
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

    void LateUpdate()
    {
        if (_shouldDetach)
        {
            _ridingPlayerRb = null;
            _shouldDetach = false;
            return;
        }

        if (_ridingPlayerRb == null) return;

        // 回転差分だけオフセットを回し、プラットフォームの新位置に足してプレイヤーを動かす
        // 子オブジェクト化しないのでプレイヤー自身の rotation は変化しない
        float angleDelta = Mathf.DeltaAngle(_prevRotationZ, transform.eulerAngles.z);
        Vector3 rotatedOffset = Quaternion.Euler(0f, 0f, angleDelta) * _playerOffsetBeforeMove;
        _ridingPlayerRb.position = (Vector2)(transform.position + rotatedOffset);
    }
}
