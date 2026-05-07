// =====================================================
// IButtonState.cs - ボタン押下状態の共通インターフェース
// 使い方: ToggleButton・PressureButton に実装する
// =====================================================
public interface IButtonState
{
    bool IsPressed { get; }
}
