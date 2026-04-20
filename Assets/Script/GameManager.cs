using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("設定")]
    [SerializeField] private float deathY = -10f;

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
            if (State == GameState.Playing)
                Pause();
            else if (State == GameState.Paused)
                Resume();
        }

        if (State == GameState.Playing && player != null && player.position.y < deathY)
            OnDeath();
    }

    // ─────────────────────────────
    // ゲームイベント
    // ─────────────────────────────
    public void OnClear()
    {
        if (State != GameState.Playing) return;
        State = GameState.Cleared;
        Time.timeScale = 0f;
        clearPanel.SetActive(true);
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
        pausePanel.SetActive(true);
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
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.FadeToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.FadeToScene("StageSelect");
    }

    public void GoToNextStage()
    {
        if (!GameStageData.HasNextStage()) return;
        Time.timeScale = 1f;
        GameStageData.AdvanceToNextStage();
        SceneTransitionManager.Instance.FadeToScene(GameStageData.GetGameSceneName());
    }
}
