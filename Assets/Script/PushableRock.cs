// =====================================================
// PushableRock.cs - プレイヤーが押せる石オブジェクト
// 使い方: 石オブジェクトに Rigidbody2D・BoxCollider2D と共にアタッチする。
//         groundLayer を設定すること。
//         砂残量を 0.05 単位に丸めた整数キーで位置を管理するタイムライン方式。
// =====================================================
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableRock : MonoBehaviour, IBoardSaveable
{
    [Header("移動")]
    [Tooltip("等倍（1.0x）時の横移動速度 (units/秒)")]
    [SerializeField, Min(0f)] private float pushSpeed = 3f;
    [Tooltip("重力加速度 (units/秒²)")]
    [SerializeField, Min(0f)] private float gravityAccel = 20f;

    [Header("床の検知")]
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D              _rb;
    private BoxCollider2D            _col;
    private Vector2                  _startPos;
    private float                    _fallVelocity;
    private bool                     _isGrounded;
    private bool                     _wasGrounded;
    private bool                     _isFalling;
    private float                    _lockedX;
    private bool                     _wasSandFull;
    private Dictionary<int, Vector2> _timeline  = new Dictionary<int, Vector2>();
    private int                      _pushDir   = 0;
    private bool                     _isPushing = false;

    // プレイヤー追従（逆行時の慣性吹き飛び防止）
    private Rigidbody2D _ridingPlayerRb;
    private bool        _playerShouldDetach;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _rb.isKinematic   = true;
        _rb.gravityScale  = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _startPos     = _rb.position;
        _wasGrounded  = true;
        _wasSandFull  = true;

        // 摩擦をゼロにしてプレイヤーが側面に張り付いて浮かないようにする
        _col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    // BoxCollider2D の底面で地面との接触を判定する
    bool CheckGrounded()
    {
        Bounds b = _col.bounds;
        // 左右端を 0.05f ずつ縮めて壁際の誤検知を防ぐ
        Vector2 size = new Vector2(b.size.x - 0.1f, b.size.y);

        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false;

        var hits = new RaycastHit2D[4];
        int count = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, filter, hits, 0.05f);

        for (int i = 0; i < count; i++)
        {
            // 法線が上向き（下にある地面）のみ true。天井への反応を除外する。
            if (hits[i].normal.y > 0.5f) return true;
        }
        return false;
    }

    // 押す方向に壁があるか確認する（高さは BoxCollider2D の高さをそのまま使用）
    bool WallAhead()
    {
        Bounds b = _col.bounds;
        Vector2 size = new Vector2(b.size.x, b.size.y);

        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false;

        var hits = new RaycastHit2D[4];
        int count = Physics2D.BoxCast(b.center, size, 0f, new Vector2(_pushDir, 0f), filter, hits, 0.05f);

        for (int i = 0; i < count; i++)
        {
            // 法線が押す方向と逆向きのヒットのみ壁と判定（地面の上向き法線を除外）
            if (hits[i].normal.x * _pushDir < -0.5f) return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        // OnCollisionExit2D はフィジクスステップ後に呼ばれるため、ここで解除を反映する
        if (_playerShouldDetach)
        {
            _ridingPlayerRb     = null;
            _playerShouldDetach = false;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (TimeManager.Instance == null) return;
        if (TimeManager.Instance.IsStartupLocked) return;

        var tm = TimeManager.Instance;

        // 砂が満タンに戻った瞬間: タイムラインをクリアして初期位置に戻す
        bool isSandFull = tm.IsSandFull;
        if (isSandFull && !_wasSandFull)
        {
            _timeline.Clear();
            _rb.position  = _startPos;
            _fallVelocity = 0f;
            _isFalling    = false;
            _wasGrounded  = true;
        }
        _wasSandFull = isSandFull;

        _isGrounded = CheckGrounded();

        // 落下開始: isGrounded が true→false になった瞬間に X をピン止め
        if (!_isGrounded && _wasGrounded)
        {
            _isFalling = true;
            _lockedX   = _rb.position.x;
        }
        // 着地: isGrounded が false→true になった瞬間にピン止め解除
        else if (_isGrounded && !_wasGrounded)
        {
            _isFalling = false;
        }
        _wasGrounded = _isGrounded;

        if (tm.BoardTimeScale > 0f)
        {
            // ── 順行: 物理シミュレーション後に記録 ───────────────────────
            Vector2 pos = _rb.position;

            if (!_isGrounded)
            {
                _fallVelocity += -gravityAccel * tm.BoardDeltaTime;
                pos.y += _fallVelocity * tm.BoardDeltaTime;
                if (_isFalling) pos.x = _lockedX;
            }
            else
            {
                _fallVelocity = 0f;
                if (_isPushing && !WallAhead())
                    pos.x += _pushDir * pushSpeed * tm.BoardDeltaTime;
            }

            _rb.MovePosition(pos);
            _timeline[SandKey(tm.CurrentSand)] = pos;
        }
        else
        {
            // ── 逆行・停止: 砂残量キーに対応する位置を復元 ──────────────
            if (_timeline.TryGetValue(SandKey(tm.CurrentSand), out Vector2 saved))
            {
                Vector2 delta = saved - _rb.position;
                _rb.MovePosition(saved);

                // 石が上方向に動く場合、Kinematic の押し上げでプレイヤーに慣性が乗る。
                // 位置を直接追従させて付与された上向き速度をキャンセルする。
                if (_ridingPlayerRb != null && delta.y > 0f)
                {
                    _ridingPlayerRb.position += delta;
                    _ridingPlayerRb.linearVelocity = new Vector2(
                        _ridingPlayerRb.linearVelocity.x,
                        Mathf.Min(_ridingPlayerRb.linearVelocity.y, 0f)
                    );
                }
            }
        }

        _isPushing = false;
        _pushDir   = 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 法線が下向き = プレイヤーが上から乗った場合のみ追従対象にする
            if (contact.normal.y < -0.5f)
            {
                _ridingPlayerRb     = collision.rigidbody;
                _playerShouldDetach = false;
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            _playerShouldDetach = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (TimeManager.Instance == null || TimeManager.Instance.BoardTimeScale <= 0f) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                _pushDir   = contact.normal.x > 0 ? 1 : -1;
                _isPushing = true;
                break;
            }
        }
    }

    // CurrentSand を 0.05 単位に丸めた整数キーに変換（float 精度問題を回避）
    private static int SandKey(float sand) => Mathf.RoundToInt(sand / 0.05f);

    private struct RockSaveData
    {
        public Dictionary<int, Vector2> timeline;
        public Vector2 startPos;
        public float   fallVelocity;
        public bool    isFalling;
        public float   lockedX;
        public bool    wasGrounded;
    }

    public object SaveState()
    {
        return new RockSaveData
        {
            timeline     = new Dictionary<int, Vector2>(_timeline),
            startPos     = _startPos,
            fallVelocity = _fallVelocity,
            isFalling    = _isFalling,
            lockedX      = _lockedX,
            wasGrounded  = _wasGrounded,
        };
    }

    public void LoadState(object state)
    {
        var data     = (RockSaveData)state;
        _timeline    = new Dictionary<int, Vector2>(data.timeline);
        _startPos    = data.startPos;
        _fallVelocity = data.fallVelocity;
        _isFalling   = data.isFalling;
        _lockedX     = data.lockedX;
        _wasGrounded = data.wasGrounded;

        if (TimeManager.Instance != null)
        {
            _wasSandFull = TimeManager.Instance.IsSandFull;
            if (_timeline.TryGetValue(SandKey(TimeManager.Instance.CurrentSand), out Vector2 pos))
                _rb.position = pos;
        }
    }
}
