using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // ゲーム中どこからでもアクセスできるようにする（シングルトン）
    public static TimeManager Instance { get; private set; }

    [Header("角度設定")]
    // 砂時計の現在角度（0〜180度）
    public float HourglassAngle = 0f;

    // 角度が変化するスピード（Inspectorで調整可能）
    public float AngleChangeSpeed = 90f;

    [Header("長押し設定")]
    // 上下キーの長押し判定時間（秒）
    public float LongPressTime = 0.5f;

    [Header("砂の設定")]
    // 砂の最大量
    public float MaxSand = 10f;

    // 現在の砂の量（UIから参照する）
    public float CurrentSand { get; private set; }

    // 砂切れかどうか（外部から参照）
    public bool IsSandEmpty { get; private set; } = false;
    // 砂が満タンかどうか（外部から参照）
    public bool IsSandFull => CurrentSand >= MaxSand && !IsReversing;


    // 盤面の時間スケール（外部から参照）
    public float BoardTimeScale { get; private set; } = 1f;

    // 盤面用deltaTime（足場などがこれを使う）
    public float BoardDeltaTime => Time.deltaTime * BoardTimeScale;

    // 逆行中かどうか（外部から参照）
    public bool IsReversing => BoardTimeScale < 0f;

    // 長押しタイマー（上下キー用）
    private float upKeyTimer = 0f;
    private float downKeyTimer = 0f;
    private float bothKeyTimer = 0f;

    // 目標角度（アナログ変化の目的地）
    private float targetAngle = 0f;

    void Awake()
    {
        Instance = this;
        CurrentSand = MaxSand; // 砂を満タンにする
    }

    void Update()
    {
        HandleInput();
        UpdateAngle();
        UpdateBoardTimeScale();
        UpdateSand();
    }

    // ─────────────────────────────
    // 入力処理
    // ─────────────────────────────
    void HandleInput()
    {
        // 満タン中は逆行方向への入力を無効化
        if (IsSandFull)
        {
            if (Input.GetKey(KeyCode.RightArrow))
                targetAngle -= AngleChangeSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                upKeyTimer += Time.deltaTime;
                if (upKeyTimer >= LongPressTime)
                {
                    targetAngle = 0f;
                    HourglassAngle = 0f;
                }
            }
            else
            {
                upKeyTimer = 0f;
            }

            targetAngle = Mathf.Clamp(targetAngle, 0f, 90f);
            return;
        }

        // 砂切れ中は順行方向への入力を無効化
        if (IsSandEmpty)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                targetAngle += AngleChangeSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.DownArrow))
            {
                downKeyTimer += Time.deltaTime;
                if (downKeyTimer >= LongPressTime)
                {
                    targetAngle = 180f;
                    HourglassAngle = 180f;
                }
            }
            else
            {
                downKeyTimer = 0f;
            }

            targetAngle = Mathf.Clamp(targetAngle, 90f, 180f);
            return;
        }

        // 左キー：逆行方向へ
        if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            targetAngle += AngleChangeSpeed * Time.deltaTime;
        }

        // 右キー：順行方向へ
        if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            targetAngle -= AngleChangeSpeed * Time.deltaTime;
        }

        // ←→同時押し長押し：停止（90度）へ即座に
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            bothKeyTimer += Time.deltaTime;
            if (bothKeyTimer >= LongPressTime)
            {
                targetAngle = 90f;
                HourglassAngle = 90f;
            }
        }
        else
        {
            bothKeyTimer = 0f;
        }

        // 上キー長押し：等倍（0度）へ即座に
        if (Input.GetKey(KeyCode.UpArrow))
        {
            upKeyTimer += Time.deltaTime;
            if (upKeyTimer >= LongPressTime)
            {
                targetAngle = 0f;
                HourglassAngle = 0f;
            }
        }
        else
        {
            upKeyTimer = 0f;
        }

        // 下キー長押し：最大逆行（180度）へ即座に
        if (Input.GetKey(KeyCode.DownArrow))
        {
            downKeyTimer += Time.deltaTime;
            if (downKeyTimer >= LongPressTime)
            {
                targetAngle = 180f;
                HourglassAngle = 180f;
            }
        }
        else
        {
            downKeyTimer = 0f;
        }

        targetAngle = Mathf.Clamp(targetAngle, 0f, 180f);
    }

    // ─────────────────────────────
    // 現在角度を目標角度に向けてシームレスに動かす
    // ─────────────────────────────
    void UpdateAngle()
    {
        HourglassAngle = Mathf.MoveTowards(
            HourglassAngle,
            targetAngle,
            AngleChangeSpeed * Time.deltaTime
        );
    }

    // ─────────────────────────────
    // 角度からBoardTimeScaleを計算
    // ─────────────────────────────
    void UpdateBoardTimeScale()
    {
        if (HourglassAngle <= 90f)
        {
            // 0度→1.0、90度→0.0
            BoardTimeScale = 1f - (HourglassAngle / 90f);
        }
        else
        {
            // 90度→0.0、180度→-1.0
            BoardTimeScale = -((HourglassAngle - 90f) / 90f);
        }
    }

    // ─────────────────────────────
    // 砂の増減処理
    // ─────────────────────────────
    void UpdateSand()
    {
        float sandDelta = Mathf.Abs(BoardTimeScale) * Time.deltaTime;

        if (IsReversing)
        {
            IsSandEmpty = false;
            CurrentSand += sandDelta;
            CurrentSand = Mathf.Min(CurrentSand, MaxSand);

            if (CurrentSand >= MaxSand)
            {
                ForceMovToForward(); // IsSandFullの記述を削除
            }
        }
        else
        {
            CurrentSand -= sandDelta;
            CurrentSand = Mathf.Max(CurrentSand, 0f);

            if (CurrentSand <= 0f)
            {
                IsSandEmpty = true;
                ForceMoveToReverse();
            }
        }
    }

    // 砂切れ時：90度（停止）へ即座に
    void ForceMoveToReverse()
    {
        targetAngle = 90f;
        HourglassAngle = 90f;
    }

    // 満タン時：90度（停止）へ即座に
    void ForceMovToForward()
    {
        targetAngle = 90f;
        HourglassAngle = 90f;
    }
}