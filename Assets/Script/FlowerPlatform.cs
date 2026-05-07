// =====================================================
// FlowerPlatform.cs - 花と茎が一体の上下移動ギミック
// 使い方: 花オブジェクトにアタッチし、_stemTransform・_stemCollider・_stemAnchorY を設定する。
//         BoardDeltaTime を積算して直線的に伸縮する。逆行時は自動で縮む。
// =====================================================
using UnityEngine;

public class FlowerPlatform : MonoBehaviour, IBoardSaveable
{
    [SerializeField, Min(0f)] private float         _moveSpeed     = 1f;
    [SerializeField]          private Transform     _stemTransform;
    [SerializeField]          private BoxCollider2D _stemCollider;
    [SerializeField]          private float         _stemAnchorY  = 0f;
    [Tooltip("花の上限。設定した Transform の Y 座標を天井として使う。null = 天井なし")]
    [SerializeField]          private Transform     _ceilingTransform;

    private float _startY;
    private float _time;

    private Rigidbody2D _ridingPlayerRb;
    private bool        _shouldDetach;
    private Vector3     _playerOffsetBeforeMove;

    void Awake()
    {
        _startY = transform.position.y;
        UpdateStem(); // 物理演算が始まる前に初期サイズを確定する
    }

    void Update()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;

        if (_ridingPlayerRb != null)
            _playerOffsetBeforeMove = (Vector3)_ridingPlayerRb.position - transform.position;

        _time += TimeManager.Instance.BoardDeltaTime;

        // 内部時間は自由に積算し、見た目だけ天井でクランプする。
        // 天井に張り付いた分だけ逆行させないと縮み始めない挙動はこれで実現される。
        float rawY   = _startY + _time * _moveSpeed;
        float upperY = _ceilingTransform != null ? _ceilingTransform.position.y : float.MaxValue;
        float visY   = Mathf.Clamp(rawY, _stemAnchorY, upperY);

        Vector3 pos = transform.position;
        pos.y = visY;
        transform.position = pos;

        UpdateStem();
    }

    void UpdateStem()
    {
        if (_stemTransform == null || _stemCollider == null) return;

        float stemHeight = Mathf.Max(0f, transform.position.y - _stemAnchorY);

        // localScale.y = stemHeight でスプライトを伸縮するため、
        // コライダーの Y サイズは 1 固定にする（ワールド高さ = size.y × localScale.y = 1 × stemHeight）
        _stemCollider.enabled = stemHeight > 0f;
        _stemCollider.size    = new Vector2(_stemCollider.size.x, 1f);
        _stemCollider.offset  = new Vector2(_stemCollider.offset.x, 0f);

        _stemTransform.position = new Vector3(
            _stemTransform.position.x,
            _stemAnchorY + stemHeight * 0.5f,
            _stemTransform.position.z);
        // scale=0 は Transform に問題を起こすため最小値を設ける
        _stemTransform.localScale = new Vector3(
            _stemTransform.localScale.x,
            Mathf.Max(stemHeight, 0.0001f),
            _stemTransform.localScale.z);
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

        _ridingPlayerRb.position = (Vector2)(transform.position + _playerOffsetBeforeMove);
    }

    private struct State
    {
        public Vector3 position;
        public float   time;
    }

    public object SaveState() => new State { position = transform.position, time = _time };

    public void LoadState(object state)
    {
        var s = (State)state;
        transform.position = s.position;
        _time = s.time;
        UpdateStem();
    }
}
