// =====================================================
// MovingPlatformEditor.cs - MovingPlatform の軌道プレビューを Scene ビューに描画する
// 使い方: Editor フォルダに置くだけで MovingPlatform の Custom Editor として自動適用される。
//         赤い点線で Inspector の displayDuration 秒分の順行軌道を表示する。
// =====================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    private const int   Samples = 80;
    private const float DotPx   = 5f;

    private static readonly Color ColorForward = Color.red;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        MovingPlatform mp = (MovingPlatform)target;

        if (!(mp.moveDirection != Vector2.zero && mp.moveSpeed > 0f)) return;

        var tm = Object.FindObjectOfType<TimeManager>();
        float previewDuration = tm != null ? tm.MaxSand : 10f;

        Vector3 startPos = mp.transform.position;

        // ── 軌道サンプリング ──────────────────────────────────────────
        Handles.color = ColorForward;
        Vector3 prev = SamplePos(mp, startPos, 0f);
        for (int i = 1; i <= Samples; i++)
        {
            float   t    = (float)i / Samples * previewDuration;
            Vector3 curr = SamplePos(mp, startPos, t);
            Handles.DrawDottedLine(prev, curr, DotPx);
            prev = curr;
        }

        // 始点・終端マーカー
        Handles.color = Color.white;
        Handles.DrawSolidDisc(SamplePos(mp, startPos, 0f),            Vector3.forward, 0.05f);
        Handles.DrawSolidDisc(SamplePos(mp, startPos, previewDuration), Vector3.forward, 0.05f);
    }

    // ── ヘルパー ─────────────────────────────────────────────────────

    private static Vector3 SamplePos(MovingPlatform mp, Vector3 startPos, float t)
    {
        Vector3 pos = startPos;
        if (mp.moveDirection != Vector2.zero)
            pos += (Vector3)(mp.moveDirection.normalized * mp.moveSpeed * t);
        return pos;
    }
}
