# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Time-Grain** is a 2D platformer puzzle game built with **Unity 6000.3.13f1**. The core mechanic is hourglass-based time manipulation: players tilt an hourglass to control time flow (forward, paused, or reversed) to solve platform puzzles. A sand resource caps how long time can be reversed, preventing softlocks.

## Build & Development

- **Build**: Open in Unity Editor → File > Build Settings. No CLI build scripts exist.
- **Play in editor**: Open a scene in Unity Editor and press Play. Start from `Title.unity` for full flow, or `Test.unity` for isolated testing.
- **Unity version**: 6000.3.13f1 (see `ProjectSettings/ProjectVersion.txt`).

There are no automated tests; testing is done manually in the editor.

## Architecture

All game scripts live in `Assets/Script/`. The project uses a **Manager singleton pattern** where major systems are persistent singletons accessed statically.

### Core Systems

**`TimeManager`** — The central mechanic. Tracks hourglass angle (0°→180°) which maps to time scale (+1x → 0 → −1x). Exposes `BoardDeltaTime` which all time-affected objects (moving platforms, etc.) must use instead of `Time.deltaTime`. Manages the sand resource (0–10 units) that depletes on reverse and refills on forward. Key properties: `HourglassAngle`, `CurrentSand`, `BoardTimeScale`, `IsReversing`, `IsSandEmpty`, `IsSandFull`.

**`GameManager`** — Game state machine (`Playing`, `Paused`, `Dead`, `Cleared`). Detects player death (Y < −10), handles pause/resume via ESC, controls `Time.timeScale` for pause, and activates/deactivates UI panels. Check `GameManager.IsPlaying` before accepting player input.

**`SceneTransitionManager`** — Persistent singleton (`DontDestroyOnLoad`) that auto-generates itself. Handles fade transitions between scenes. Call it from any scene; it creates its own canvas at runtime. `Time.timeScale` はフェードアウト中は 0 のまま保持され、シーン読み込み完了後にリセットされる（暗転中にゲームが動かないようにするため）。

**`PlayerController`** — Physics-based 2D platformer movement using `Rigidbody2D.AddForce`. A/D for movement, Space for jump. Ground detection uses `OverlapCircle`. Respects `GameManager.IsPlaying`.

**`MovingPlatform`** — Uses `TimeManager.BoardDeltaTime` so platform movement responds to time manipulation. Player is parented to the platform on landing (detected via collision normal) and unparented on exit.

**`StageSelectManager`** — Two-phase UI: world selection (1–4) then stage selection (1–3 per world). Stores selection in the static `GameStageData` class (`GameStageData.SelectedWorld`, `GameStageData.SelectedStage`).

**`ClearObject`** — ゴールオブジェクトにアタッチ。`Collider2D`（IsTrigger=ON）と組み合わせて使用。Player タグのオブジェクトが触れると `GameManager.OnClear()` を呼ぶ。

**`DebugDisplay`** — `OnGUI` overlay showing sand %, time scale, angle, and warnings. Active during development.

### Scene Flow

```
Title.unity → StageSelect.unity → Scenes/Stages/Stage{W}-{S}.unity (gameplay)
    → [retry: 同ステージ再読み込み]
    → [clear: 次ステージ or StageSelect へ]
Credit.unity (from Title)
Test.unity (dev only)
```

- ステージシーンは `Scenes/Stages/` 以下に `Stage1-1` 〜 `Stage4-3` の12シーン構成（ワールド1〜4、各3ステージ）。
- `Main.unity` は使用しない。
- シーン名は `GameStageData.GetGameSceneName()` が `"Stage{SelectedWorld}-{SelectedStage}"` の形式で返す。

### ステージシーン ヒエラルキー構成

```
Stage{W}-{S}.unity
├── GameManager          ← GameManager.cs
├── TimeManager          ← TimeManager.cs
├── Player               ← Tag: "Player"、PlayerController.cs
├── Stage
│   ├── Tilemap (Ground)
│   ├── MovingPlatform   ← MovingPlatform.cs（必要に応じて）
│   └── GoalObject       ← Collider2D(IsTrigger=ON)、ClearObject.cs
└── UI (Canvas)
    ├── HUD
    ├── PausePanel       ← 初期非表示
    ├── ClearPanel       ← 初期非表示（NextStageButton / StageSelectButton を持つ）
    └── RetryPanel       ← 初期非表示
```

- ClearPanel のボタン OnClick: `GameManager.GoToNextStage()` / `GameManager.GoToStageSelect()`
- UI アニメーションを使う場合は Animator の **Update Mode を Unscaled Time** にすること（`Time.timeScale = 0` で停止するため）。

### Key Design Rules

- Any object that should respond to time manipulation must use `TimeManager.BoardDeltaTime` instead of `Time.deltaTime`.
- Player input should be gated on `GameManager.IsPlaying` (returns false when paused, dead, or cleared).
- `SceneTransitionManager` and `GameManager` are persistent singletons — do not add multiple instances.
- Stage progress is stored in the static `GameStageData`; it is not persisted across sessions.
- 全12ステージシーンを Build Settings に登録すること。
- `FadeToScene()` を呼ぶ前に `Time.timeScale = 1f` しないこと。暗転中にゲームが動いてしまう。timeScale のリセットは `SceneTransitionManager` がシーン読み込み完了後に内部で行う。

## Planned Features

以下は今後実装予定の機能（未着手）：

- **フェード演出の改善** — 現在は単純な黒フェードのみ。方向ワイプ・円形アイリス・ノイズディゾルブなどへの拡張を検討中。`SceneTransitionManager` に複数のエフェクト種別を持たせる形で実装する予定。
- **レベルデザイン** — 全12ステージのシーンファイルは存在するが、中身はまだ未実装。
- **サウンド** — BGM・SE の仕組みがまだない。`AudioManager` の追加が必要。

## Coding Conventions

### Comment Style

全スクリプトに以下の形式のファイルヘッダーを付ける：

```
// =====================================================
// FileName.cs - 一行の要約
// 使い方: アタッチ先や呼び出し方を簡潔に
// =====================================================
```

インラインコメントは **WHY が非自明な箇所のみ** 記述する。コードを読めばわかる WHAT は書かない。

### File Encoding

`Assets/Script/` 以下の `.cs` ファイルはすべて **UTF-8** で保存すること。Shift-JIS など他のエンコーディングで保存すると日本語コメントが文字化けする。

### `BoardDeltaTime` の扱い

`TimeManager.BoardDeltaTime` は逆行中に**負の値**を返す。これを使うオブジェクトは逆方向への動作を前提として設計すること。`Time.deltaTime`（常に正）と混在させないよう注意。

## Dependencies

Key Unity packages (see `Packages/manifest.json`):
- Universal Render Pipeline (URP) 17.3.0
- Input System 1.19.0 (new input system; `InputSystem_Actions.inputactions` is the main asset)
- 2D packages: Tilemap, SpriteShape, Animation, PSD Importer, Aseprite
- TextMeshPro, UGUI 2.0.0
