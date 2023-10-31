/// <summary>
/// 
/// 座標ごとの評価値を保持するクラス
/// 
/// </summary>
[System.Serializable]
public class Move : Point
{
    public int eval = 0; // 評価値

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    public Move() : base(0, 0)
    {
        eval = 0;
    }

    /// <summary>
    /// 
    /// 引数付きコンストラクタ
    /// 
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <param name="e">評価値</param>
    public Move(int x, int y, int e) : base(x, y)
    {
        eval = e;
    }
}