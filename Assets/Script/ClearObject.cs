using UnityEngine;

// ゴールオブジェクトにアタッチする
// Collider2D の IsTrigger をオンにすること
[RequireComponent(typeof(Collider2D))]
public class ClearObject : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance?.OnClear();
    }
}
