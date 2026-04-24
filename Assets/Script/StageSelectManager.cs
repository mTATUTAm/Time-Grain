// =====================================================
// StageSelectManager.cs - ステージ選択画面の制御
// 使い方: StageSelect.unity の GameObject にアタッチし、各ボタンの OnClick に対応メソッドを登録する。
//         Phase1（ワールド選択）→ Phase2（ステージ選択）の2段階UI。
//         選択結果は GameStageData に書き込まれ、シーン遷移後も参照される。
// =====================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectManager : MonoBehaviour
{
    [Header("パネル")]
    [SerializeField] private GameObject worldPanel;
    [SerializeField] private GameObject stagePanel;

    [Header("左上の戻るボタン")]
    [SerializeField] private Button backToTitleButton;   // WorldPanel 左上
    [SerializeField] private Button backToWorldButton;   // StagePanel 左上

    [Header("ワールドボタン (W1, W2, W3, W4 の順)")]
    [SerializeField] private Button[] worldButtons;

    [Header("ステージボタン (3つ)")]
    [SerializeField] private Button[] stageButtons;

    [Header("ステージボタンのテキスト")]
    [SerializeField] private TextMeshProUGUI[] stageButtonTexts;

    private int _selectedWorld = 1;

    // ワールド選択: 2×2グリッド + 左上の戻るボタン
    // [← Title]
    // [W1(0,0)] [W2(0,1)]
    // [W3(1,0)] [W4(1,1)]
    private int _worldRow = 0;
    private int _worldCol = 0;
    private bool _worldOnBack = false;

    // ステージ選択: 横1列 + 左上の戻るボタン
    // [← WorldSelect]
    // [S1]  [S2]  [S3]
    private int _stageCursor = 0;
    private bool _stageOnBack = false;

    private int WorldCursorIndex => _worldRow * 2 + _worldCol;

    private void Start()
    {
        ShowWorldPanel();
    }

    private void Update()
    {
        if (worldPanel.activeSelf)
            HandleWorldInput();
        else if (stagePanel.activeSelf)
            HandleStageInput();
    }

    // ─────────────────────────────
    // キーボード入力
    // ─────────────────────────────
    private void HandleWorldInput()
    {
        if (_worldOnBack)
        {
            // 戻るボタンにいるとき: ↓か→でグリッドへ、Enter で Title へ
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)
             || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                _worldOnBack = false;
                UpdateWorldHighlight();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)
                  || Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackToTitle();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _worldCol = Mathf.Max(0, _worldCol - 1);
            UpdateWorldHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _worldCol = Mathf.Min(1, _worldCol + 1);
            UpdateWorldHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (_worldRow > 0)
            {
                _worldRow--;
                UpdateWorldHighlight();
            }
            else
            {
                // グリッド最上段から↑で戻るボタンへ
                _worldOnBack = true;
                UpdateWorldHighlight();
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            _worldRow = Mathf.Min(1, _worldRow + 1);
            UpdateWorldHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnWorldSelected(WorldCursorIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToTitle();
        }
    }

    private void HandleStageInput()
    {
        if (_stageOnBack)
        {
            // 戻るボタンにいるとき: ↓か→でグリッドへ、Enter で WorldSelect へ
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)
             || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                _stageOnBack = false;
                UpdateStageHighlight();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)
                  || Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackFromStagePanel();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _stageCursor = Mathf.Max(0, _stageCursor - 1);
            UpdateStageHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _stageCursor = Mathf.Min(stageButtons.Length - 1, _stageCursor + 1);
            UpdateStageHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // グリッドから↑で戻るボタンへ
            _stageOnBack = true;
            UpdateStageHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnStageSelected(_stageCursor + 1);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackFromStagePanel();
        }
    }

    // ─────────────────────────────
    // ハイライト
    // ─────────────────────────────

    // Button.Select() は EventSystem 経由で Enter 長押し中に onClick を繰り返すため、
    // image.color を直接操作してハイライトを管理する
    private void UpdateWorldHighlight()
    {
        var backCb = backToTitleButton.colors;
        backToTitleButton.image.color = _worldOnBack ? backCb.highlightedColor : backCb.normalColor;

        for (int i = 0; i < worldButtons.Length; i++)
        {
            var cb = worldButtons[i].colors;
            worldButtons[i].image.color = (!_worldOnBack && i == WorldCursorIndex)
                ? cb.highlightedColor : cb.normalColor;
        }
    }

    private void UpdateStageHighlight()
    {
        var backCb = backToWorldButton.colors;
        backToWorldButton.image.color = _stageOnBack ? backCb.highlightedColor : backCb.normalColor;

        for (int i = 0; i < stageButtons.Length; i++)
        {
            var cb = stageButtons[i].colors;
            stageButtons[i].image.color = (!_stageOnBack && i == _stageCursor)
                ? cb.highlightedColor : cb.normalColor;
        }
    }

    // ─────────────────────────────
    // ワールド選択
    // ─────────────────────────────
    public void OnWorldSelected(int worldIndex)
    {
        _selectedWorld = worldIndex;
        _stageCursor = 0;
        _stageOnBack = false;
        ShowStagePanel();
    }

    private void ShowWorldPanel()
    {
        worldPanel.SetActive(true);
        stagePanel.SetActive(false);
        _worldOnBack = false;
        UpdateWorldHighlight();
    }

    private void ShowStagePanel()
    {
        worldPanel.SetActive(false);
        stagePanel.SetActive(true);

        for (int i = 0; i < stageButtonTexts.Length; i++)
        {
            if (stageButtonTexts[i] != null)
                stageButtonTexts[i].text = $"W{_selectedWorld} - {i + 1}";
        }

        UpdateStageHighlight();
    }

    // ─────────────────────────────
    // ステージ選択
    // ─────────────────────────────
    public void OnStageSelected(int stageIndex)
    {
        GameStageData.SelectedWorld = _selectedWorld;
        GameStageData.SelectedStage = stageIndex;
        SceneTransitionManager.Instance.FadeToScene(GameStageData.GetGameSceneName());
    }

    // ─────────────────────────────
    // 戻る
    // ─────────────────────────────
    public void OnBackFromStagePanel()
    {
        _worldOnBack = false;
        ShowWorldPanel();
    }

    public void OnBackToTitle()
    {
        SceneTransitionManager.Instance.FadeToScene("Title");
    }
}
