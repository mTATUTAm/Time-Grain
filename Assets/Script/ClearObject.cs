// =====================================================
// ClearObject.cs - ゴール判定オブジェクト
// 使い方: ゴールオブジェクトにアタッチし、Collider2D の IsTrigger をオンにする。
//         Player タグのオブジェクトが触れると GameManager.OnClear() を呼び出す。
// =====================================================
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClearObject : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance?.OnClear();
    }
}
