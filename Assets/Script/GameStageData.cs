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
