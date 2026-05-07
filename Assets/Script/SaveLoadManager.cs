// =====================================================
// SaveLoadManager.cs - Q長押しでセーブ、E長押しでロード
// 使い方: ステージシーンの任意オブジェクトにアタッチ。IBoardSaveable を実装したオブジェクトのみ対象。
// =====================================================
using UnityEngine;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    [Header("設定")]
    public float holdDuration = 0.5f;

    private float _saveTimer = 0f;
    private float _loadTimer = 0f;
    private Dictionary<IBoardSaveable, object> _snapshot = new Dictionary<IBoardSaveable, object>();

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        HandleSave();
        HandleLoad();
    }

    private void HandleSave()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            _saveTimer += Time.unscaledDeltaTime;
            if (_saveTimer >= holdDuration)
            {
                PerformSave();
                _saveTimer = 0f;
            }
        }
        else { _saveTimer = 0f; }
    }

    private void HandleLoad()
    {
        if (Input.GetKey(KeyCode.E))
        {
            _loadTimer += Time.unscaledDeltaTime;
            if (_loadTimer >= holdDuration)
            {
                PerformLoad();
                _loadTimer = 0f;
            }
        }
        else { _loadTimer = 0f; }
    }

    private void PerformSave()
    {
        _snapshot.Clear();
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb is IBoardSaveable saveable)
                _snapshot[saveable] = saveable.SaveState();
        }
        Debug.Log($"セーブ完了: {_snapshot.Count}件");
    }

    private void PerformLoad()
    {
        if (_snapshot.Count == 0)
        {
            Debug.LogWarning("セーブデータがないためロードできません");
            return;
        }

        int count = 0;
        foreach (var item in _snapshot)
        {
            if (item.Key is MonoBehaviour mb && mb == null) continue;
            item.Key.LoadState(item.Value);
            count++;
        }
        Debug.Log($"<color=yellow>{count}件のオブジェクトをロードしました</color>");
    }
}
