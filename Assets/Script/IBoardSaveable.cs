// =====================================================
// IBoardSaveable.cs - 盤面セーブロード対象オブジェクトのインターフェース
// 使い方: セーブ対象の MonoBehaviour に実装する
// =====================================================
public interface IBoardSaveable
{
    object SaveState();
    void LoadState(object state);
}
