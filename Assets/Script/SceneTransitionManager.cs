// =====================================================
// SceneTransitionManager.cs - フェード演出付きシーン遷移のシングルトン
// 使い方: SceneTransitionManager.Instance.FadeToScene("シーン名") を呼ぶだけ。
//         Inspector 設定を有効にするには Title.unity にこのコンポーネントを配置すること。
//         配置しない場合は自動生成されコードの初期値が使われる。
// =====================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TransitionType { BlackFade, BrushStroke }

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneTransitionManager");
                _instance = go.AddComponent<SceneTransitionManager>();
            }
            return _instance;
        }
    }

    [Header("デフォルト設定")]
    [SerializeField] private TransitionType defaultTransitionType = TransitionType.BrushStroke;
    [SerializeField] private float fadeOutDuration  = 0.7f;
    [SerializeField] private float fadeInDuration   = 0.7f;
    [SerializeField] private float blackHoldDuration = 0.3f;

    private RawImage _fadeImage;
    private Material _defaultMaterial;
    private Material _brushMaterial;
    private TransitionType _activeType;
    private bool isTransitioning = false;
    public static bool IsTransitioning => _instance != null && _instance.isTransitioning;

    private const float NoiseScale   = 0.10f;
    private const float EdgeSoftness = 0.025f;
    private static readonly float BrushProgressStart = -(NoiseScale + EdgeSoftness);
    private static readonly float BrushProgressEnd   =  1f + NoiseScale + EdgeSoftness;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);

        _fadeImage = imgObj.AddComponent<RawImage>();
        _fadeImage.raycastTarget = false;

        Texture2D whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
        _fadeImage.texture = whiteTex;
        _fadeImage.color   = new Color(0f, 0f, 0f, 0f);

        RectTransform rt = _fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _defaultMaterial = _fadeImage.material;

        Shader brushShader = Resources.Load<Shader>("BrushStrokeFade");
        if (brushShader != null)
        {
            _brushMaterial = new Material(brushShader);
            _brushMaterial.SetFloat("_NoiseScale",   NoiseScale);
            _brushMaterial.SetFloat("_EdgeSoftness", EdgeSoftness);
        }
    }

    // ─────────────────────────────────────────
    // 公開 API
    // ─────────────────────────────────────────

    // Inspector の defaultTransitionType を使う
    public void FadeToScene(string sceneName)
    {
        FadeToScene(sceneName, defaultTransitionType);
    }

    // 呼び出し元が TransitionType・時間を明示する場合（-1 で Inspector のデフォルト値を使用）
    public void FadeToScene(string sceneName, TransitionType type, float fadeOut = -1f, float fadeIn = -1f, float hold = -1f)
    {
        if (!isTransitioning)
            StartCoroutine(FadeTransition(sceneName, type, fadeOut, fadeIn, hold));
    }

    // ─────────────────────────────────────────
    // 内部コルーチン
    // ─────────────────────────────────────────
    private IEnumerator FadeTransition(string sceneName, TransitionType type, float fadeOut, float fadeIn, float hold = -1f)
    {
        isTransitioning = true;
        _activeType = type;

        float outDur  = fadeOut >= 0f ? fadeOut : fadeOutDuration;
        float inDur   = fadeIn  >= 0f ? fadeIn  : fadeInDuration;
        float holdDur = hold    >= 0f ? hold    : blackHoldDuration;

        Time.timeScale = 0f;

        ApplyMaterial(type);
        SetVisualProgress(0f);

        yield return StartCoroutine(Fade(0f, 1f, outDur)); // 暗転

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        yield return op;

        if (holdDur > 0f)
            yield return new WaitForSecondsRealtime(holdDur); // 真っ黒ホールド

        yield return StartCoroutine(Fade(1f, 0f, inDur)); // 明転

        Time.timeScale = 1f;
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetVisualProgress(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetVisualProgress(to);
    }

    private void ApplyMaterial(TransitionType type)
    {
        if (type == TransitionType.BrushStroke && _brushMaterial != null)
        {
            _fadeImage.material = _brushMaterial;
            _fadeImage.color    = Color.white;
        }
        else
        {
            _fadeImage.material = _defaultMaterial;
            _fadeImage.color    = new Color(0f, 0f, 0f, 0f);
        }
    }

    private void SetVisualProgress(float progress)
    {
        if (_activeType == TransitionType.BrushStroke && _brushMaterial != null)
        {
            float mapped = Mathf.Lerp(BrushProgressStart, BrushProgressEnd, progress);
            _brushMaterial.SetFloat("_Progress", mapped);
        }
        else
        {
            _fadeImage.color = new Color(0f, 0f, 0f, progress);
        }
    }
}
