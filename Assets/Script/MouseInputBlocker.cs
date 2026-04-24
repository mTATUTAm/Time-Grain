// =====================================================
// MouseInputBlocker.cs - Canvas のマウス入力を無効化する
// 使い方: UI を持つ Canvas オブジェクトにアタッチするだけ。
//         キーボードナビゲーション（onClick.Invoke 経由）には影響しない。
// =====================================================
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GraphicRaycaster))]
public class MouseInputBlocker : MonoBehaviour
{
    void Awake()
    {
        GetComponent<GraphicRaycaster>().enabled = false;
    }
}
