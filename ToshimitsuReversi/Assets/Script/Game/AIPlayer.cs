/// <summary>
/// 
/// AIが動かすプレイヤーのクラス
/// 
/// </summary>
public class AIPlayer : Player
{
    private AI ai = null; // 石を配置する場所を考えるAIクラス

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    /// <param name="level">AIの強さレベル</param>
    public AIPlayer(int level)
    {
        ai = new AlphaBetaAI(level);
        playerKind = PlayerKind.AI_PLAYER;
    }

    /// <summary>
    /// 
    /// AIの石を設置する行動
    /// 
    /// </summary>
    /// <param name="board"></param>
    public override void onTurn(Board board)
    {
        ai.move(in board);
    }
}