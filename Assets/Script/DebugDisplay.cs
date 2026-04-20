using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    void OnGUI()
    {
        // 背景用スタイル
        GUIStyle style = new GUIStyle();
        style.fontSize = 70;
        style.normal.textColor = Color.white;

        // 砂の残量（小数点2桁）
        float sandRatio = TimeManager.Instance.CurrentSand / TimeManager.Instance.MaxSand;
        GUI.Label(new Rect(10, 10, 300, 30),
            $"砂の残量：{TimeManager.Instance.CurrentSand:F2} / {TimeManager.Instance.MaxSand}  ({sandRatio * 100:F0}%)",
            style);

        // 現在の再生速度
        GUI.Label(new Rect(10, 80, 300, 30),
            $"再生速度：{TimeManager.Instance.BoardTimeScale:F2}",
            style);

        // 砂時計の角度
        GUI.Label(new Rect(10, 150, 300, 30),
            $"角度：{TimeManager.Instance.HourglassAngle:F1}°",
            style);

        // 砂切れ警告
        if (TimeManager.Instance.IsSandEmpty)
        {
            GUIStyle warnStyle = new GUIStyle();
            warnStyle.fontSize = 70;
            warnStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(10, 220, 300, 30), "⚠ 砂切れ", warnStyle);
        }
        if (TimeManager.Instance.IsSandFull)
        {
            GUIStyle warnStyle = new GUIStyle();
            warnStyle.fontSize = 70;
            warnStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, 290, 300, 30), "⚠ 満タン", warnStyle);
        }
    }
}