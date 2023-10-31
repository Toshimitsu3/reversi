/// <summary>
/// 
/// プレイヤーの抽象クラス
/// 
/// </summary>
public abstract class Player 
{
    /// <summary>
    /// 
    /// プレイヤーの種類の列挙体
    /// 
    /// </summary>
    public enum PlayerKind
    {
        HUMAN_PLAYER,
        AI_PLAYER,
        MAX_PLAYER
    }

    protected PlayerKind playerKind; // プレイヤーの種類

    public abstract void onTurn(Board board); // 行動

    /// <summary>
    /// 
    /// プレイヤーの種類を返す
    /// 
    /// </summary>
    /// <returns></returns>
    public PlayerKind getKind()
    {
        return playerKind;
    }
}
