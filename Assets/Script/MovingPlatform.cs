// =====================================================
// MovingPlatform.cs - 時間操作に連動して動く足場
// 使い方: 動かしたい足場オブジェクトにアタッチし、moveOffset・moveDuration・rotationAngle・rotationDuration を設定する。
//         移動・回転ともに始点から目標値まで Duration 秒かけてピンポン往復する。
//         BoardDeltaTime を使うため、時間逆行中は自動的に逆再生される。
//         プレイヤーが乗ると自動でペアレントされ、足場と一緒に動く。
// =====================================================
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("平行移動")]
    [Tooltip("始点から終点への移動量 (ワールド座標の差分)")]
    public Vector2 moveOffset = Vector2.zero;
    [Tooltip("始点→終点の片道にかかる秒数")]
    [Min(0.01f)] public float moveDuration = 2f;

    [Header("回転")]
    [Tooltip("始点から終点への回転角度 (度、正=反時計回り)")]
    public float rotationAngle = 0f;
    [Tooltip("始点→終点の片道にかかる秒数")]
    [Min(0.01f)] public float rotationDuration = 2f;

    [Header("軌道プレビュー（エディタのみ）")]
    [Tooltip("回転時に軌道を追跡するローカル座標。Sceneビューでドラッグして位置を指定する。(0,0) でオブジェクト中心を追跡")]
    public Vector2 trajectoryPoint = Vector2.zero;

    private Vector3 _startPosition;
    private float _startRotationZ;
    private float _moveTime = 0f;
    private float _rotTime = 0f;

    private Transform _ridingPlayer = null;
    private bool _shouldDetach = false;

    void Start()
    {
        _startPosition = transform.position;
        _startRotationZ = transform.eulerAngles.z;
    }

    void Update()
    {
        float dt = TimeManager.Instance.BoardDeltaTime;

        if (moveOffset != Vector2.zero)
        {
            _moveTime += dt / moveDuration;
            transform.position = _startPosition + (Vector3)(moveOffset * PingPong01(_moveTime));
        }

        if (rotationAngle != 0f)
        {
            _rotTime += dt / rotationDuration;
            transform.rotation = Quaternion.Euler(0f, 0f, _startRotationZ + rotationAngle * PingPong01(_rotTime));
        }
    }

    // t を [0,1] のピンポン値に変換。負の t（逆行時）も正しく処理する
    private static float PingPong01(float t)
    {
        float n = ((t % 2f) + 2f) % 2f;
        return n <= 1f ? n : 2f - n;
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
