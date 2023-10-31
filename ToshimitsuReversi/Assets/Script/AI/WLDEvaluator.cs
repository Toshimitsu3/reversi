/// <summary>
/// 
/// 終盤の必勝読み用評価関数クラス
/// 
/// </summary>
[System.Serializable]
public class WLDEvaluator : Evaluator
{
    public const int WIN  =  1; // 勝ちを表す定数
    public const int DRAW =  0; // 引き分けを表す定数
    public const int LOSE = -1; // 負けを表す定数

    /// <summary>
    /// 
    /// 必勝読みを行う際の評価関数
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>石が多いならWIN、少ないならLOSE、同じならDRAW</returns>
    public override int evaluate(in Board board)
    {
        int discdiff = (int)board.getCurrentColor()
            * ((int)board.countDisc(DiscColor.BLACK)
            - (int)board.countDisc(DiscColor.WHITE));

        if (discdiff > 0) return WIN;
        else if (discdiff < 0) return LOSE;
        else return DRAW;
    }
}
