// =====================================================
// MovingPlatformTrajectory.cs - MovingPlatform の軌道をゲーム中にドット列で表示する
// 使い方: MovingPlatform と同じ GameObject にアタッチするだけ。
//         赤ドット = プラットフォームがこれから通る軌道。
//         青ドット = すでに通過済みの軌道。
// =====================================================
using UnityEngine;

[RequireComponent(typeof(MovingPlatform))]
public class MovingPlatformTrajectory : MonoBehaviour
{
    [Header("ドット")]
    [SerializeField, Min(4)]     private int   dotCount  = 40;
    [SerializeField, Min(0.01f)] private float dotRadius = 0.04f;

    [Header("色")]
    [SerializeField] private Color forwardColor = Color.red;
    [SerializeField] private Color reverseColor = new Color(0.3f, 0.6f, 1f);

    [Header("描画")]
    [Tooltip("SpriteRenderer の sortingOrder。プラットフォームより小さい値にすると後ろに描画される")]
    [SerializeField] private int sortingOrder = -1;

    private MovingPlatform   _mp;
    private SpriteRenderer[] _dotRenderers;
    private float[]          _dotTimes;
    private float            _half;
    private float            _timeAccum;
    private GameObject       _container;

    private void Start()
    {
        _mp = GetComponent<MovingPlatform>();
        BuildDots();
    }

    private void Update()
    {
        if (_dotRenderers == null || _half <= 0f) return;
        if (TimeManager.Instance.IsStartupLocked) return;
        _timeAccum += TimeManager.Instance.BoardDeltaTime;
        UpdateDotColors();
    }

    private void OnDestroy()
    {
        if (_container != null) Destroy(_container);
    }

    // ── 初期化 ───────────────────────────────────────────────────────────

    private void BuildDots()
    {
        bool hasMove = _mp.moveDirection != Vector2.zero && _mp.moveSpeed > 0f;
        bool hasRot  = _mp.rotationSpeed != 0f;
        if (!hasMove && !hasRot)
        {
            Debug.LogWarning("[Trajectory] moveDirection/moveSpeed と rotationSpeed が両方ゼロなので軌道を生成しません。Inspector を確認してください。");
            return;
        }

        _half = TimeManager.Instance != null ? TimeManager.Instance.MaxSand : 10f;
        if (_half <= 0f) return;

        Vector3 startPos = transform.position;
        Sprite  dotSprite = CreateCircleSprite();

        _container    = new GameObject($"[Trajectory] {gameObject.name}");
        _dotRenderers = new SpriteRenderer[dotCount];
        _dotTimes     = new float[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            float t = (float)i / dotCount * _half;
            _dotTimes[i]     = t;
            _dotRenderers[i] = SpawnDot(SamplePos(startPos, t), dotSprite);
        }

        UpdateDotColors();
    }

    private SpriteRenderer SpawnDot(Vector3 worldPos, Sprite dotSprite)
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

    private void UpdateDotColors()
    {
        // _half でループして通過済み（reverseColor）と前方（forwardColor）を切り替える
        float cycleT = ((_timeAccum % _half) + _half) % _half;

        for (int i = 0; i < _dotRenderers.Length; i++)
            _dotRenderers[i].color = _dotTimes[i] < cycleT ? reverseColor : forwardColor;
    }

    // ── ヘルパー ─────────────────────────────────────────────────────────

    private Vector3 SamplePos(Vector3 startPos, float t)
    {
        Vector3 pos = startPos;
        if (_mp.moveDirection != Vector2.zero)
            pos += (Vector3)(_mp.moveDirection.normalized * _mp.moveSpeed * t);
        return pos;
    }

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
