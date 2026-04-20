using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ステージ選択画面
// Phase1: ワールド1〜4を表示
// Phase2: 選んだワールドのステージ1〜3を表示
public class StageSelectManager : MonoBehaviour
{
    [Header("パネル")]
    [SerializeField] private GameObject worldPanel;
    [SerializeField] private GameObject stagePanel;

    [Header("ワールドボタン (4つ)")]
    [SerializeField] private Button[] worldButtons;

    [Header("ステージボタン (3つ)")]
    [SerializeField] private Button[] stageButtons;

    [Header("ステージボタンのテキスト")]
    [SerializeField] private TextMeshProUGUI[] stageButtonTexts;

    private int selectedWorld = 1;

    private void Start()
    {
        ShowWorldPanel();
    }

    // ─────────────────────────────
    // ワールド選択
    // ─────────────────────────────
    public void OnWorldSelected(int worldIndex)
    {
        selectedWorld = worldIndex;
        ShowStagePanel();
    }

    private void ShowWorldPanel()
    {
        worldPanel.SetActive(true);
        stagePanel.SetActive(false);
    }

    private void ShowStagePanel()
    {
        worldPanel.SetActive(false);
        stagePanel.SetActive(true);

        for (int i = 0; i < stageButtonTexts.Length; i++)
        {
            if (stageButtonTexts[i] != null)
                stageButtonTexts[i].text = $"W{selectedWorld} - {i + 1}";
        }
    }

    // ─────────────────────────────
    // ステージ選択
    // ─────────────────────────────
    public void OnStageSelected(int stageIndex)
    {
        GameStageData.SelectedWorld = selectedWorld;
        GameStageData.SelectedStage = stageIndex;
        SceneTransitionManager.Instance.FadeToScene(GameStageData.GetGameSceneName());
    }

    // ─────────────────────────────
    // ボタン
    // ─────────────────────────────
    public void OnBackFromStagePanel()
    {
        ShowWorldPanel();
    }

    public void OnBackToTitle()
    {
        SceneTransitionManager.Instance.FadeToScene("Title");
    }
}
