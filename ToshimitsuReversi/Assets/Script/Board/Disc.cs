/// <summary>
/// 
/// 石を表現するクラス
/// 
/// </summary>
[System.Serializable]
public class Disc : Point
{
    public DiscColor color; // 石の色情報

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    public Disc() : base(0, 0)
    {
        color = DiscColor.EMPTY;
    }

    /// <summary>
    /// 
    /// 引数付きコンストラクタ
    /// 
    /// </summary>
    /// <param name="x">座標情報x</param>
    /// <param name="y">座標情報y</param>
    /// <param name="color">石の色</param>
    public Disc(int x, int y, DiscColor color) : base(x,y)
    {
        this.color = color;
    }
}
