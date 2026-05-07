// =====================================================
// PressureDoor.cs - PressureButton が押されている間だけ開く扉
// 使い方: 扉オブジェクトにアタッチする。Collider2D と SpriteRenderer が必要。
//         Inspector の _button に対応する PressureButton を登録する。
// =====================================================
using UnityEngine;

public class PressureDoor : MonoBehaviour
{
    [SerializeField] private PressureButton _button;

    private Collider2D     _col;
    private SpriteRenderer _sr;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _sr  = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;
        if (_button == null) return;

        SetOpen(_button.IsPressed);
    }

    void SetOpen(bool open)
    {
        _col.enabled = !open;
        _sr.enabled  = !open;
    }
}
