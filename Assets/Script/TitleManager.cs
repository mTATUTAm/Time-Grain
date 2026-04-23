// =====================================================
// TitleManager.cs - タイトル画面のボタン処理
// 使い方: Title.unity の GameObject にアタッチし、ボタンの OnClick に各メソッドを登録する。
// =====================================================
using UnityEngine;

public class TitleManager : MonoBehaviour
{
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
