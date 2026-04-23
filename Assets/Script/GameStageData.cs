// =====================================================
// GameStageData.cs - 選択中のワールド・ステージ番号を保持する静的クラス
// 使い方: StageSelectManager が選択後に書き込み、GameManager が読み取ってシーンを決定する。
//         セッション中のみ保持され、アプリ再起動で初期化される。
// =====================================================
public static class GameStageData
{
    public static int SelectedWorld = 1; // 1〜4
    public static int SelectedStage = 1; // 1〜3

    public static bool HasNextStage()
    {
        return !(SelectedWorld >= 4 && SelectedStage >= 3);
    }

    public static void AdvanceToNextStage()
    {
        SelectedStage++;
        if (SelectedStage > 3)
        {
            SelectedStage = 1;
            SelectedWorld++;
        }
    }

    public static string GetGameSceneName()
    {
        return $"Stage{SelectedWorld}-{SelectedStage}";
    }
}
