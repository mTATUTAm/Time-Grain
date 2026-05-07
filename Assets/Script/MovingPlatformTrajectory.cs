// =====================================================
// MovingPlatformTrajectory.cs - MovingPlatform の軌道をゲーム中にドット列で表示する
// 使い方: MovingPlatform と同じ GameObject にアタッチするだけ。
//         赤ドット = これから通る軌道 / 青ドット = 通過済みの軌道。
// =====================================================
using UnityEngine;

[RequireComponent(typeof(MovingPlatform))]
public class MovingPlatformTrajectory : MonoBehaviour
{
    [Header("ドット")]
    [SerializeField, Min(2)]      private int   dotCount  = 40;
    [SerializeField, Min(0.01f)]  private float dotRadius = 0.04f;

    [Header("色")]
    [SerializeField] private Color forwardColor = Color.red;
    [SerializeField] private Color reverseColor = new Color(0.3f, 0.6f, 1f);

    [Header("追跡点")]
    [Tooltip("軌道を表示するローカル座標オフセット。(0,0) = 中心、(1,0) = 中心から右に1unit")]
    [SerializeField] private Vector2 pivotOffset = Vector2.zero;

    [Header("描画")]
    [Tooltip("SpriteRenderer の sortingOrder。プラットフォームより小さい値にすると後ろに描画される")]
    [SerializeField] private int sortingOrder = -1;

    private MovingPlatform   _mp;
    private SpriteRenderer[] _dotRenderers;
    private float[]          _dotTs;
    private GameObject       _container;
    private bool             _dotsBuilt;

    void Start()
    {
        _mp = GetComponent<MovingPlatform>();
    }

    void Update()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;

        // TimeManager が準備できた最初のフレームでドットを生成する
        if (!_dotsBuilt)
        {
            BuildDots();
            _dotsBuilt = true;
            return;
        }

        if (_dotRenderers == null) return;
        UpdateDotColors(_mp.GetT());
    }

    void OnDestroy()
    {
        if (_container != null) Destroy(_container);
    }

    // ── 初期化 ───────────────────────────────────────────────────────────

    private void BuildDots()
    {
        if (_mp.waypoints == null || _mp.waypoints.Length == 0)
        {
            Debug.LogWarning("[Trajectory] waypoints が未設定なので軌道を生成しません。");
            return;
        }

        Sprite dotSprite  = CreateCircleSprite();
        _container        = new GameObject($"[Trajectory] {gameObject.name}");
        _dotRenderers     = new SpriteRenderer[dotCount];
        _dotTs            = new float[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            float t      = (float)i / (dotCount - 1);
            _dotTs[i]        = t;
            _dotRenderers[i] = SpawnDot(GetDotWorldPos(t), dotSprite);
        }

        UpdateDotColors(0f);
    }

    private SpriteRenderer SpawnDot(Vector2 worldPos, Sprite dotSprite)
    {
        var go = new GameObject();
        go.transform.SetParent(_container.transform, false);
        go.transform.position   = worldPos;
        go.transform.localScale = Vector3.one * (dotRadius * 2f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = dotSprite;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    // ── 毎フレームの色更新 ────────────────────────────────────────────────

    private void UpdateDotColors(float currentT)
    {
        for (int i = 0; i < _dotRenderers.Length; i++)
            _dotRenderers[i].color = _dotTs[i] <= currentT ? reverseColor : forwardColor;
    }

    // ── ドット位置計算 ────────────────────────────────────────────────────

    // 追跡点のワールド座標 = 実際の中心位置 + pivotOffset を回転角で回したもの
    private Vector2 GetDotWorldPos(float t)
    {
        Vector2 center = _mp.EvaluateActualWorldPos(t);
        if (pivotOffset == Vector2.zero) return center;

        float maxSand = TimeManager.Instance != null ? TimeManager.Instance.MaxSand : 0f;
        float rad     = _mp.rotationSpeed * maxSand * t * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        return center + new Vector2(
            cos * pivotOffset.x - sin * pivotOffset.y,
            sin * pivotOffset.x + cos * pivotOffset.y);
    }

    // ── ヘルパー ─────────────────────────────────────────────────────────

    private static Sprite CreateCircleSprite(int size = 32)
    {
        var   tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        float h   = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx    = x + 0.5f - h, dy = y + 0.5f - h;
            float alpha = Mathf.Clamp01((h - Mathf.Sqrt(dx * dx + dy * dy)) * 2f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}
