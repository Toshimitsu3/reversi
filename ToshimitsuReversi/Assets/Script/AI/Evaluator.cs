/// <summary>
/// 
/// 評価関数を司る親クラス
/// 
/// </summary>
public abstract class Evaluator
{
    /// <summary>
    /// 
    /// 次の手の評価値を返す
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>評価値</returns>
    public abstract int evaluate(in Board board);
}

