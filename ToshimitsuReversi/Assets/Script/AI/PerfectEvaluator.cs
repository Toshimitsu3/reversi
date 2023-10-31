/// <summary>
/// 
/// 終盤の完全読み切り用評価関数クラス
/// 
/// </summary>
[System.Serializable]
public class PerfectEvaluator : Evaluator
{
    /// <summary>
    /// 
    /// 石の差分を評価値として返す
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>石の差分</returns>
    public override int evaluate(in Board board)
    {
        int discdiff = (int)board.getCurrentColor() 
            * ((int)board.countDisc(DiscColor.BLACK)  - (int)board.countDisc(DiscColor.WHITE));
        return discdiff;
    }
}
