// =====================================================
// MovingPlatformEditor.cs - MovingPlatform の軌道を Scene ビューで編集・プレビューする
// 使い方: Editor フォルダに置くだけで自動適用される。
//         各ウェイポイントを Pivot ハンドルでドラッグ編集できる。
// =====================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    private const float DotPx = 5f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        MovingPlatform mp = (MovingPlatform)target;
        if (mp.waypoints == null || mp.waypoints.Length == 0) return;

        int     n      = mp.waypoints.Length;
        Vector2 origin = mp.transform.position;

        // ── ピボットハンドル ─────────────────────────────────────────────
        if (mp.rotationSpeed != 0f)
        {
            Vector3 basePos    = (Vector3)(origin + mp.waypoints[0]);
            Vector3 worldPivot = basePos + (Vector3)mp.pivotOffset;

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(basePos, worldPivot, DotPx);
            Handles.DrawSolidDisc(worldPivot, Vector3.forward, 0.08f);

            EditorGUI.BeginChangeCheck();
            Vector3 movedPivot = Handles.PositionHandle(worldPivot, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mp, "Move Pivot");
                mp.pivotOffset = (Vector2)(movedPivot - basePos);
            }
        }

        // ── ウェイポイントハンドル ──────────────────────────────────────
        for (int i = 0; i < n; i++)
        {
            Vector3 worldPos = (Vector3)(origin + mp.waypoints[i]);

            // 砂消費 % ラベル
            float pct = n > 1 ? (float)i / (n - 1) * 100f : 0f;
            Handles.Label(worldPos + Vector3.up * 0.3f, $"{pct:0}%", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mp, "Move Waypoint");
                mp.waypoints[i] = (Vector2)moved - origin;
            }
        }

        // ── 軌道プレビュー（ウェイポイント間を点線で繋ぐ） ────────────────
        Handles.color = Color.red;
        for (int i = 0; i < n - 1; i++)
        {
            Vector3 a = (Vector3)(origin + mp.waypoints[i]);
            Vector3 b = (Vector3)(origin + mp.waypoints[i + 1]);
            Handles.DrawDottedLine(a, b, DotPx);
        }

        // 始点・終点マーカー
        Handles.color = Color.white;
        Handles.DrawSolidDisc((Vector3)(origin + mp.waypoints[0]),     Vector3.forward, 0.05f);
        Handles.DrawSolidDisc((Vector3)(origin + mp.waypoints[n - 1]), Vector3.forward, 0.05f);
    }
}
