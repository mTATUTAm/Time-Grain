// =====================================================
// MovingPlatformTrajectory.cs - MovingPlatform の軌道をゲーム中にドット列で表示する
// 使い方: MovingPlatform と同じ GameObject にアタッチするだけ。
//         赤ドット = プラットフォームがこれから通る順行軌道。
//         青ドット = すでに通過済み、または復路の軌道。
//         時間逆行中は青になったドットが赤に戻る。
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
    private float            _full;
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
        bool hasMove = _mp.moveOffset    != Vector2.zero;
        bool hasRot  = _mp.rotationAngle != 0f;
        if (!hasMove && !hasRot)
        {
            Debug.LogWarning("[Trajectory] moveOffset と rotationAngle が両方ゼロなので軌道を生成しません。Inspector を確認してください。");
            return;
        }

        _half = Mathf.Max(
            hasMove ? _mp.moveDuration    : 0f,
            hasRot  ? _mp.rotationDuration : 0f
        );
        if (_half <= 0f) return;
        _full = _half * 2f;

        Vector3 startPos  = transform.position;
        float   startRotZ = transform.eulerAngles.z;
        Sprite  dotSprite = CreateCircleSprite();

        _container    = new GameObject($"[Trajectory] {gameObject.name}");
        _dotRenderers = new SpriteRenderer[dotCount];
        _dotTimes     = new float[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            float t = (float)i / dotCount * _half; // 往路のみ（0 → _half）
            _dotTimes[i]     = t;
            _dotRenderers[i] = SpawnDot(SamplePos(startPos, startRotZ, t), dotSprite);
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
        // PingPong01 で往路・復路を折り返した進捗を [0, _half] で表現
        // 往路: 0→_half（増加）、復路: _half→0（減少）
        // _timeAccum が負でも PingPong01 が正しく処理するため初期フレームの誤表示が出ない
        float cycleT = PingPong01(_timeAccum / _half) * _half;

        for (int i = 0; i < _dotRenderers.Length; i++)
            _dotRenderers[i].color = _dotTimes[i] < cycleT ? reverseColor : forwardColor;
    }

    // ── ヘルパー ─────────────────────────────────────────────────────────

    private Vector3 SamplePos(Vector3 startPos, float startRotZ, float t)
    {
        Vector3 pos = startPos;
        if (_mp.moveOffset != Vector2.zero)
            pos += (Vector3)(_mp.moveOffset * PingPong01(t / _mp.moveDuration));

        float rotZ = startRotZ;
        if (_mp.rotationAngle != 0f)
            rotZ += _mp.rotationAngle * PingPong01(t / _mp.rotationDuration);

        return pos + (Vector3)RotateVec(_mp.trajectoryPoint, rotZ);
    }

    private static float PingPong01(float t)
    {
        float n = ((t % 2f) + 2f) % 2f;
        return n <= 1f ? n : 2f - n;
    }

    private static Vector2 RotateVec(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
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
