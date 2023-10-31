/// <summary>
/// 
/// 探索ルーチンのクラス
/// 
/// </summary>
public abstract class AI
{
    public uint presearch_depth; // alpha-beta法やNegaScout法において、
                                 // 事前に手を調べて探索順序を決めるための先読み手数
    public uint normal_depth;    // 序盤・中盤の探索における先読み手数
    public uint wld_depth;       // 終盤において、必勝読みを始める残り手数
    public uint perfect_depth;   // 終盤において、完全読みを始める残り手数

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public AI(int level)
    {
        if(level == 0)
        {
            presearch_depth = 3;
            normal_depth = 3;
            wld_depth = 10;
            perfect_depth = 8;
        }
        else if(level == 1)
        {
            presearch_depth = 3;
            normal_depth = 5;
            wld_depth = 12;
            perfect_depth = 10;
        }
        else if(level == 2)
        {
            presearch_depth = 4;
            normal_depth = 7;
            wld_depth = 15;
            perfect_depth = 13;
        }
    }

    /// <summary>
    /// 
    /// 石の設置を行う
    /// 
    /// </summary>
    /// <param name="board"></param>
    public abstract void move(in Board board);
}
