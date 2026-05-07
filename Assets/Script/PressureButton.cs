// =====================================================
// PressureButton.cs - 乗っている間だけ押された状態になるボタン
// 使い方: ボタンオブジェクトにアタッチする。Collider2D（IsTrigger=true）が必要。
//         PressureDoor の Inspector に登録して使う。
// =====================================================
using UnityEngine;
using System.Collections.Generic;

public class PressureButton : MonoBehaviour, IButtonState
{
    public bool IsPressed { get; private set; }

    private bool _isTouched;
    private bool _wasReversing;
    private bool _wasSandFull;
    private Dictionary<int, bool> _timeline = new Dictionary<int, bool>();

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;

        var tm = TimeManager.Instance;

        bool isSandFull = tm.IsSandFull;
        if (isSandFull && !_wasSandFull)
        {
            _timeline.Clear();
            _isTouched = false;
            IsPressed  = false;
        }
        _wasSandFull = isSandFull;

        int currentKey = SandKey(tm.CurrentSand);

        if (tm.BoardTimeScale > 0f)
        {
            // 逆行→順行の切り替わり瞬間: 未来の記録（現在より小さいキー）を削除
            if (_wasReversing)
            {
                var toDelete = new List<int>();
                foreach (int key in _timeline.Keys)
                    if (key < currentKey) toDelete.Add(key);
                foreach (int key in toDelete)
                    _timeline.Remove(key);
            }

            IsPressed  = _isTouched;
            _timeline[currentKey] = IsPressed;
        }
        else
        {
            // 逆行・停止: タイムラインから復元
            IsPressed = _timeline.TryGetValue(currentKey, out bool val) && val;
        }

        _wasReversing = tm.IsReversing;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (TimeManager.Instance == null || TimeManager.Instance.IsStartupLocked) return;
        if (TimeManager.Instance.IsReversing) return;

        _isTouched = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _isTouched = false;
    }

    private static int SandKey(float sand) => Mathf.RoundToInt(sand / 0.05f);
}
