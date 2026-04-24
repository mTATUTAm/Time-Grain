// =====================================================
// GameManager.cs - ゲーム状態（Playing / Paused / Dead / Cleared）を管理するシングルトン
// 使い方: プレイヤー入力前に GameManager.Instance.IsPlaying で状態を確認する。
//         ゴール到達時は OnClear()、死亡判定は Update 内で自動実行される。
// =====================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Dead, Cleared }
    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;

    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject retryPanel;

    [Header("ポーズパネルボタン (Resume, Retry, StageSelect の順)")]
    [SerializeField] private Button[] pauseButtons;

    [Header("クリアパネルボタン (Next, Select の順)")]
    [SerializeField] private Button[] clearButtons;

    [Header("設定")]
    [SerializeField] private float deathY = -10f;

    private int _pauseCursor = 0;
    private int _clearCursor = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        clearPanel.SetActive(false);
        retryPanel.SetActive(false);
        Time.timeScale = 1f;
        State = GameState.Playing;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        if (State == GameState.Playing && player != null && player.position.y < deathY)
            OnDeath();

        if (State == GameState.Paused)
            HandlePauseInput();
        else if (State == GameState.Cleared)
            HandleClearInput();
    }

    // ─────────────────────────────
    // キーボードナビゲーション
    // ─────────────────────────────
    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _pauseCursor = Mathf.Max(0, _pauseCursor - 1);
            UpdatePauseHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _pauseCursor = Mathf.Min(pauseButtons.Length - 1, _pauseCursor + 1);
            UpdatePauseHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            pauseButtons[_pauseCursor].onClick.Invoke();
        }
    }

    private void HandleClearInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _clearCursor = Mathf.Max(0, _clearCursor - 1);
            UpdateClearHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _clearCursor = Mathf.Min(clearButtons.Length - 1, _clearCursor + 1);
            UpdateClearHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            clearButtons[_clearCursor].onClick.Invoke();
        }
    }

    // Button.Select() は EventSystem 経由で Enter 長押し中に onClick を繰り返すため、
    // image.color を直接操作してハイライトを管理する
    private void UpdatePauseHighlight()
    {
        for (int i = 0; i < pauseButtons.Length; i++)
        {
            var cb = pauseButtons[i].colors;
            pauseButtons[i].image.color = (i == _pauseCursor) ? cb.highlightedColor : cb.normalColor;
        }
    }

    private void UpdateClearHighlight()
    {
        for (int i = 0; i < clearButtons.Length; i++)
        {
            var cb = clearButtons[i].colors;
            clearButtons[i].image.color = (i == _clearCursor) ? cb.highlightedColor : cb.normalColor;
        }
    }

    // ─────────────────────────────
    // ゲームイベント
    // ─────────────────────────────
    public void OnClear()
    {
        if (State != GameState.Playing) return;
        State = GameState.Cleared;
        Time.timeScale = 0f; // UIアニメーションには Animator の Update Mode を Unscaled Time にすること
        _clearCursor = 0;
        clearPanel.SetActive(true);
        UpdateClearHighlight();
    }

    public void OnDeath()
    {
        if (State != GameState.Playing) return;
        State = GameState.Dead;
        Time.timeScale = 0f;
        retryPanel.SetActive(true);
    }

    // ─────────────────────────────
    // ポーズ
    // ─────────────────────────────
    public void Pause()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        _pauseCursor = 0;
        pausePanel.SetActive(true);
        UpdatePauseHighlight();
    }

    public void Resume()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    // ─────────────────────────────
    // ボタンから呼ぶ
    // ─────────────────────────────
    public void Retry()
    {
        SceneTransitionManager.Instance.FadeToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToStageSelect()
    {
        SceneTransitionManager.Instance.FadeToScene("StageSelect");
    }

    public void GoToNextStage()
    {
        if (!GameStageData.HasNextStage()) return;
        GameStageData.AdvanceToNextStage();
        SceneTransitionManager.Instance.FadeToScene(GameStageData.GetGameSceneName());
    }
}
