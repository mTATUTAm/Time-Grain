// =====================================================
// SceneTransitionManager.cs - フェードイン/アウト付きシーン遷移のシングルトン
// 使い方: SceneTransitionManager.Instance.FadeToScene("シーン名") を呼ぶだけ。
//         どのシーンにも配置不要。初回アクセス時に自動生成され DontDestroyOnLoad される。
// =====================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    // どのシーンから再生しても自動生成されるシングルトン
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

    [SerializeField] private float fadeDuration = 0.5f;

    private Image fadeImage;
    private bool isTransitioning = false;

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
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 全UIより前面にしてフェード画像を常に最前面に描画する

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void FadeToScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(FadeTransition(sceneName));
    }

    private IEnumerator FadeTransition(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(Fade(0f, 1f)); // Fade は unscaledDeltaTime なので timeScale=0 でも動く

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        yield return op;

        Time.timeScale = 1f; // シーン読み込み完了後に戻す（StageSelect など GameManager のないシーン向け）

        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // timeScale=0 のポーズ中もフェードできるよう Unscaled を使う
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
