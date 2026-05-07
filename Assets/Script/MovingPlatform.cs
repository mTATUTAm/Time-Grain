// =====================================================
// MovingPlatform.cs - 砂時計の進み具合にのみ連動して動く足場
// 使い方: 足場オブジェクトにアタッチし、waypoints を Inspector または Scene ビューで設定する。
//         砂残量から直接位置・回転を計算するためセーブロードに影響されず常に正確な状態を返す。
// =====================================================
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Tooltip("通過する位置をシーン配置座標からのオフセットで定義する（砂 0% → 100% の順）")]
    public Vector2[] waypoints = { Vector2.zero, new Vector2(5f, 0f) };

    [Header("回転")]
    [Tooltip("回転速度 (度/秒、正=反時計回り)")]
    public float rotationSpeed = 0f;
    [Tooltip("回転の中心をローカル座標でオフセット (0,0 = 基準位置の中心)")]
    public Vector2 pivotOffset = Vector2.zero;

    private Vector2     _origin;
    private float       _prevRotationZ;
    private Rigidbody2D _ridingPlayerRb;
    private bool        _shouldDetach;
    private Vector3     _playerOffsetBeforeMove;

    void Awake()
    {
        _origin = transform.position;
    }

    void Update()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;
        if (waypoints == null || waypoints.Length == 0) return;

        if (_ridingPlayerRb != null)
            _playerOffsetBeforeMove = (Vector3)_ridingPlayerRb.position - transform.position;
        _prevRotationZ = transform.eulerAngles.z;

        float t     = GetT();
        float angle = rotationSpeed * TimeManager.Instance.MaxSand * t;
        ApplyTransform(EvaluateWorldPath(t), angle);
    }

    void ApplyTransform(Vector2 basePos, float angle)
    {
        if (pivotOffset != Vector2.zero)
        {
            // 基準位置から pivotOffset だけ離れた点を回転中心として軌道を計算する
            Vector2 pivot = basePos + pivotOffset;
            Vector2 arm   = basePos - pivot; // = -pivotOffset
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            transform.position = new Vector3(
                pivot.x + cos * arm.x - sin * arm.y,
                pivot.y + sin * arm.x + cos * arm.y,
                0f);
        }
        else
        {
            transform.position = (Vector3)basePos;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 砂消費率 (0 = 満タン, 1 = 空)
    public float GetT()
    {
        var tm = TimeManager.Instance;
        return tm != null && tm.MaxSand > 0f ? 1f - tm.CurrentSand / tm.MaxSand : 0f;
    }

    // t (0〜1) に対応する実際のワールド座標を返す（pivot・回転を含む）
    public Vector2 EvaluateActualWorldPos(float t)
    {
        Vector2 basePos = EvaluateWorldPath(t);
        if (rotationSpeed == 0f || pivotOffset == Vector2.zero) return basePos;

        float maxSand = TimeManager.Instance != null ? TimeManager.Instance.MaxSand : 0f;
        float angle   = rotationSpeed * maxSand * t;
        Vector2 pivot = basePos + pivotOffset;
        Vector2 arm   = basePos - pivot;
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        return new Vector2(pivot.x + cos * arm.x - sin * arm.y, pivot.y + sin * arm.x + cos * arm.y);
    }

    // t (0〜1) に対応する基準ワールド座標を返す（pivot・回転を含まない）
    public Vector2 EvaluateWorldPath(float t)
    {
        t = Mathf.Clamp01(t);
        int n = waypoints.Length;
        if (n == 1) return _origin + waypoints[0];
        float s = t * (n - 1);
        int   i = Mathf.Min(Mathf.FloorToInt(s), n - 2);
        return _origin + Vector2.Lerp(waypoints[i], waypoints[i + 1], s - i);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                _ridingPlayerRb = collision.rigidbody;
                _shouldDetach   = false;
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            _shouldDetach = true;
    }

    void LateUpdate()
    {
        if (_shouldDetach)
        {
            _ridingPlayerRb = null;
            _shouldDetach   = false;
            return;
        }
        if (_ridingPlayerRb == null) return;

        float angleDelta    = Mathf.DeltaAngle(_prevRotationZ, transform.eulerAngles.z);
        Vector3 rotatedOffset = Quaternion.Euler(0f, 0f, angleDelta) * _playerOffsetBeforeMove;
        _ridingPlayerRb.position = (Vector2)(transform.position + rotatedOffset);
    }
}
