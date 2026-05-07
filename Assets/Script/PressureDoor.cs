// =====================================================
// PressureDoor.cs - PressureButton が押されている間だけ開く扉
// 使い方: 扉オブジェクトにアタッチする。Collider2D と SpriteRenderer が必要。
//         Inspector の _button に対応する PressureButton を登録する。
// =====================================================
using UnityEngine;

public class PressureDoor : MonoBehaviour, IBoardSaveable
{
    [SerializeField] private PressureButton _button;

    private Collider2D     _col;
    private SpriteRenderer _sr;
    private bool           _loadedOpen;
    private bool           _hasLoadedState;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _sr  = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (TimeManager.Instance == null) return;

        bool timeStopped = TimeManager.Instance.IsStartupLocked
                        || TimeManager.Instance.BoardTimeScale == 0f;

        if (_hasLoadedState && timeStopped)
        {
            // 時間停止中はロードした開閉状態を維持する
            SetOpen(_loadedOpen);
            return;
        }

        // 時間が動き始めたらロード状態を破棄して通常処理へ
        _hasLoadedState = false;
        if (timeStopped || _button == null) return;
        SetOpen(_button.IsPressed);
    }

    void SetOpen(bool open)
    {
        _col.enabled = !open;
        _sr.enabled  = !open;
    }

    public object SaveState() => !_col.enabled; // true = 現在開いている

    public void LoadState(object state)
    {
        _loadedOpen     = (bool)state;
        _hasLoadedState = true;
        SetOpen(_loadedOpen);
    }
}
