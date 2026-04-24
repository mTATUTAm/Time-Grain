// =====================================================
// TitleManager.cs - タイトル画面の制御
// 使い方: Title.unity の GameObject にアタッチ。
//         menuButtons に Play / Credit / Quit の順でボタンを設定する。
// =====================================================
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("メニューボタン (上から Play, Credit, Quit の順)")]
    [SerializeField] private Button[] menuButtons;

    private int _cursor = 0;

    private void Start()
    {
        UpdateHighlight();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            _cursor = (_cursor - 1 + menuButtons.Length) % menuButtons.Length;
            UpdateHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            _cursor = (_cursor + 1) % menuButtons.Length;
            UpdateHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[_cursor].onClick.Invoke();
        }
    }

    // Button.Select() は EventSystem 経由で Enter 長押し中に onClick を繰り返すため、
    // image.color を直接操作してハイライトを管理する
    private void UpdateHighlight()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var cb = menuButtons[i].colors;
            menuButtons[i].image.color = (i == _cursor) ? cb.highlightedColor : cb.normalColor;
        }
    }

    public void OnStageSelectButton()
    {
        SceneTransitionManager.Instance.FadeToScene("StageSelect");
    }

    public void OnCreditButton()
    {
        SceneTransitionManager.Instance.FadeToScene("Credit");
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
