/// <summary>
/// 
/// 人が動かすプレイヤークラス
/// 
/// </summary>
public class HumanPlayer : Player
{
    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    public HumanPlayer()
    {
        playerKind = PlayerKind.HUMAN_PLAYER;
    }

    /// <summary>
    /// 
    /// 行動
    /// 
    /// </summary>
    /// <param name="board"></param>
    public override void onTurn(Board board)
    {

    }
}
