// =====================================================
// MovingPlatformEditor.cs - MovingPlatform の軌道プレビューを Scene ビューに描画する
// 使い方: Editor フォルダに置くだけで MovingPlatform の Custom Editor として自動適用される。
//         順行（往路）は赤、逆行（復路）は青の点線で1サイクル分の軌道を表示する。
//         回転がある場合は、黄色いハンドルを Scene ビューでドラッグして追跡点を指定できる。
// =====================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    private const int   Samples = 80;
    private const float DotPx   = 5f;

    private static readonly Color ColorForward = Color.red;
    private static readonly Color ColorReverse = new Color(0.3f, 0.6f, 1f); // 視認しやすい青

    private SerializedProperty _trajectoryPoint;

    private void OnEnable()
    {
        _trajectoryPoint = serializedObject.FindProperty("trajectoryPoint");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // trajectoryPoint は回転ありのときのみ表示する
        DrawPropertiesExcluding(serializedObject, "trajectoryPoint");

        MovingPlatform mp = (MovingPlatform)target;
        if (mp.rotationAngle != 0f)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_trajectoryPoint, new GUIContent("軌道追跡点（ローカル座標）"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        MovingPlatform mp = (MovingPlatform)target;

        bool hasMove = mp.moveOffset    != Vector2.zero;
        bool hasRot  = mp.rotationAngle != 0f;
        if (!hasMove && !hasRot) return;

        Vector3 startPos  = mp.transform.position;
        float   startRotZ = mp.transform.eulerAngles.z;

        // ── 追跡点ハンドル（回転ありのとき） ──────────────────────────
        if (hasRot)
        {
            Vector3 handlePos = startPos + (Vector3)RotateVec(mp.trajectoryPoint, startRotZ);
            float   size      = HandleUtility.GetHandleSize(handlePos) * 0.12f;

            // 中心 → 追跡点の補助線
            Handles.color = Color.yellow;
            Handles.DrawDottedLine(startPos, handlePos, 3f);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(handlePos, size, Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mp, "Move Trajectory Point");
                Vector2 delta      = new Vector2(moved.x - startPos.x, moved.y - startPos.y);
                mp.trajectoryPoint = RotateVec(delta, -startRotZ);
                EditorUtility.SetDirty(mp);
            }
        }

        // ── 軌道サンプリング ──────────────────────────────────────────
        // half = 往路の長さ（秒）、full = 往路 + 復路
        float half = Mathf.Max(
            hasMove ? mp.moveDuration     : 0f,
            hasRot  ? mp.rotationDuration : 0f
        );
        if (half <= 0f) return;
        float full = half * 2f;

        Vector3 prev = SamplePos(mp, startPos, startRotZ, 0f);
        for (int i = 1; i <= Samples; i++)
        {
            float t    = (float)i / Samples * full;
            float tMid = ((float)i - 0.5f) / Samples * full;

            Vector3 curr = SamplePos(mp, startPos, startRotZ, t);

            // 往路（順行）: 赤　復路（逆行）: 青
            Handles.color = tMid < half ? ColorForward : ColorReverse;
            Handles.DrawDottedLine(prev, curr, DotPx);
            prev = curr;
        }

        // 始点・折り返し点マーカー
        Handles.color = Color.white;
        Handles.DrawSolidDisc(SamplePos(mp, startPos, startRotZ, 0f),   Vector3.forward, 0.05f);
        Handles.DrawSolidDisc(SamplePos(mp, startPos, startRotZ, half), Vector3.forward, 0.05f);
    }

    // ── ヘルパー ─────────────────────────────────────────────────────

    private static Vector3 SamplePos(MovingPlatform mp, Vector3 startPos, float startRotZ, float t)
    {
        Vector3 pos = startPos;
        if (mp.moveOffset != Vector2.zero)
            pos += (Vector3)(mp.moveOffset * PingPong01(t / mp.moveDuration));

        float rotZ = startRotZ;
        if (mp.rotationAngle != 0f)
            rotZ += mp.rotationAngle * PingPong01(t / mp.rotationDuration);

        return pos + (Vector3)RotateVec(mp.trajectoryPoint, rotZ);
    }

    // MovingPlatform 本体と同じ実装（エディタからは private フィールドにアクセスできないため複製）
    private static float PingPong01(float t)
    {
        float n = ((t % 2f) + 2f) % 2f;
        return n <= 1f ? n : 2f - n;
    }

    private static Vector2 RotateVec(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
