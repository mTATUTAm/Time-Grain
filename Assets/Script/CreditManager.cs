// =====================================================
// CreditManager.cs - クレジット画面のボタン処理
// 使い方: Credit.unity の GameObject にアタッチし、戻るボタンの OnClick に OnBackButton を登録する。
// =====================================================
using UnityEngine;

public class CreditManager : MonoBehaviour
{
    public void OnBackButton()
    {
        SceneTransitionManager.Instance.FadeToScene("Title");
    }
}
