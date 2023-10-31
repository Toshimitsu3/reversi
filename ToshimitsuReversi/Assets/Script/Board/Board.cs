using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 盤面の状態を管理するクラス
///
/// </summary>
[System.Serializable]
public class Board 
{
    public const int BOARD_SIZE = 8; /// 盤面の目のサイズ
    public const int MAX_TURNS = 60; // 最大ターン数

    // 方向を表す列挙体
    enum Direction
    {
        NONE = 0,
        UPPER = 1,
        UPPER_LEFT = 2,
        LEFT = 4,
        LOWER_LEFT = 8,
        LOWER = 16,
        LOWER_RIGHT = 32,
        RIGHT = 64,
        UPPER_RIGHT = 128,
    };
    private DiscColor[,] RawBoard = new DiscColor[BOARD_SIZE + 2, BOARD_SIZE + 2]; // 盤面の色情報
    private uint Turns; // 現在ターン数
    private DiscColor CurrentColor; // 現在の手番の色
    private List<List<Disc>> UpdateLog = new List<List<Disc>>(); // 色の変更があった石を格納する
    private List<Point>[] MovablePos = new List<Point>[MAX_TURNS + 1];   // 配置可能な座標
    private uint[,,] MovableDir = new uint[MAX_TURNS + 1, BOARD_SIZE + 2, BOARD_SIZE + 2]; // ひっくり返すことができる方向
    private ColorStorage<uint> Discs = new ColorStorage<uint>(); // 盤面の石の色の数を数える
    private List<Disc> update = new List<Disc>();  //　更新される石情報を並べた配列
    private uint CheckDirection = (uint)Direction.NONE; // ひっくり返せる位置を探索する際に使用
    private int[,] liberty = new int[BOARD_SIZE + 2, BOARD_SIZE + 2]; //周囲の空きます

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    public Board()
    {
        Init();
    }

    /// <summary>
    /// 
    /// ボードをゲーム開始直後の状態にする。Boardクラスのインスタンスが生成された
    /// 直後は、コンストラクタによって同様の初期化処理がやばれているので、
    /// initを呼ぶ必要がない。
    /// 
    /// </summary>
    public void Init() 
    {
        for (int i = 0; i < MAX_TURNS + 1; i++)
        {
            MovablePos[i] = new List<Point>();
        }

        // 全マスを空きマスに設定
        for (int x = 1; x <= BOARD_SIZE; x++)
        {
            for(int y = 1; y < BOARD_SIZE; y++)
            {
                RawBoard[x, y] = DiscColor.EMPTY;
            }
        }

        // 壁の設定
        for(int y = 0; y < BOARD_SIZE + 2; y++)
        {
           RawBoard[0, y] = DiscColor.WALL;
           RawBoard[BOARD_SIZE + 1, y] = DiscColor.WALL;
        }

        for (int x = 0; x < BOARD_SIZE + 2; x++)
        {
           RawBoard[x, 0] = DiscColor.WALL;
           RawBoard[x,BOARD_SIZE + 1] = DiscColor.WALL;
        }

        // 初期位置
        RawBoard[4, 4] = DiscColor.WHITE;
        RawBoard[5, 5] = DiscColor.WHITE;
        RawBoard[4, 5] = DiscColor.BLACK;
        RawBoard[5, 4] = DiscColor.BLACK;

        // 石数の初期設定
        Discs[DiscColor.BLACK] = 2;
        Discs[DiscColor.WHITE] = 2;
        Discs[DiscColor.EMPTY] = BOARD_SIZE * BOARD_SIZE - 4;

        Turns = 0; // 手数は0から数える
        CurrentColor = DiscColor.BLACK; // 先手は黒

        UpdateLog.Clear();

        initMovable();
    }

    /// <summary>
    /// 
    /// pointで指定された位置に石を打つ。
    /// 処理が成功したらtrue、失敗したらfalseが返る。
    /// 
    /// </summary>
    /// <param name="point">座標情報</param>
    /// <returns>石を置けたらtrue、置けなければfalse</returns>
    public bool Move(in Point point)
    {
        // 石が打てるかの判定
        if (point.x < 0 || point.x >= BOARD_SIZE +1) return false;
        if (point.y < 0 || point.y >= BOARD_SIZE +1) return false;
        if (MovableDir[Turns, point.x, point.y] == (uint)Direction.NONE) return false;
       
        flipDiscs(point);

        Turns++;
        CurrentColor = getInverseCurrentColor();
       
        initMovable();

        return true;
    }

    /// <summary>
    /// 
    /// パスする。成功したらtrueが返る。パスができない場合(打つ手がある場合)はfalseが返る。
    /// 
    /// </summary>
    /// <returns>パスできたらtrue、できなければfalse</returns>
    public bool Pass()
    {
        // 打つ手があるなら、パスできない
        if (MovablePos[Turns].Count != 0) return false;

        // ゲームが終了しているなら、パスできない
        if (isGameOver()) return false;

        CurrentColor = getInverseCurrentColor();

        // 空のupdateを挿入しておく
        UpdateLog.Add(new List<Disc>());

        initMovable();

        return true;
    }

    /// <summary>
    /// 直前の一手を元に戻す。成功するとtrueが返る。
    /// 元に戻せない場合、すなわちまだ一手も打っていない場合はfalseが返る。
    /// </summary>
    /// <returns>成功したらtrue、失敗したらfalse</returns>
    public bool Undo()
    {
        // ゲーム開始地点ならもう戻れない
        if (Turns == 0) return false;

        CurrentColor = getInverseCurrentColor();

        List<Disc> update = new List<Disc>();
        update = UpdateLog[UpdateLog.Count - 1];
        // 前回がパスかどうかで場合分け
        if (update.Count == 0) // 前回はパス
        {
            // MovablePosとMovableDirを再構築
            MovablePos[Turns].Clear();
            for(uint x = 1; x <=BOARD_SIZE; x++)
            {
                for(uint y = 1; y <=BOARD_SIZE; y++)
                {
                    MovableDir[Turns, x, y] = (int)Direction.NONE;
                }
            }
        }
        else // 前回パスでない
        {
            Turns--;

            // 石を元に戻す
            RawBoard[update[0].x, update[0].y] = DiscColor.EMPTY;
            for(int i = 1; i < update.Count; i++)
            {
                RawBoard[update[i].x, update[i].y] = getInverseCurrentColor();
            }

            // 石の更新
            uint discdiff = (uint)update.Count;
            Discs[CurrentColor] -= discdiff;
            Discs[getInverseCurrentColor()] += discdiff - 1;
            Discs[DiscColor.EMPTY]--;
        }

        // 不要になったupdateを1つ削除
        UpdateLog.RemoveAt(UpdateLog.Count - 1);

        return true;
    }

    /// <summary>
    /// 
    /// ゲームが終了していればtrue、
    /// 終了していなければfalseを返す。
    /// 
    /// </summary>
    /// <returns>ゲーム終了でtrue、違えばfalse</returns>
    public bool isGameOver()
    {
        // 60手に達していたらゲーム終了
        if (Turns == MAX_TURNS) return true;

        // 打てる手があるならゲーム終了ではない
        if (MovablePos[Turns].Count != 0) return false;

        // 現在の手番と逆に色が打てるかどうか調べる
        
        for(int x = 1; x < BOARD_SIZE + 1; x++)
        {          
            for(int y = 1; y < BOARD_SIZE + 1; y++)
            {
                Disc disc = new Disc();
                disc.color = getInverseCurrentColor();
                disc.x = x;
                disc.y = y;
                // 置ける箇所が1つでもあればゲーム終了ではない
                if (checkMobility(disc) != (uint)Direction.NONE)
                {
                    return false;
                }                   
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// colorで指定された色の石の数を数える。色にはBLACK、WHITE、EMPTYを指定
    ///
    /// </summary>
    /// <param name="color">石の色</param>
    /// <returns>石の数</returns>
    public uint countDisc(DiscColor color)
    {
        return Discs[color];
    }

    /// <summary>
    /// 
    ///  pointで指定された位置の色を返す。
    /// 
    /// </summary>
    /// <param name="p">座標情報</param>
    /// <returns>石の色</returns>
    public DiscColor getColor(in Point p)
    {

        return RawBoard[p.x, p.y];
    }

    /// <summary>
    /// 
    /// 石を打てる座標が並んだlistを返す・
    /// 
    /// </summary>
    /// <returns>石を打てる座標が並んだlist</returns>
    public List<Point> getMovablePos()
    {
        return MovablePos[Turns];
    }
 
    /// <summary>
    /// 
    /// 直前の手で打った石と裏返した石が並んだlistを返す
    /// 
    /// </summary>
    /// <returns>直前で更新された石list</returns>
    public List<Disc> getUpdate()
    {
        if (UpdateLog.Count == 0) return new List<Disc>();
        else return UpdateLog[UpdateLog.Count - 1];
    }

    /// <summary>
    /// 
    /// 現在の手番の色を返す。
    /// 
    /// </summary>
    /// <returns>現在の手番の色</returns>
    public DiscColor getCurrentColor()
    {
        return CurrentColor;
    }

    /// <summary>
    /// 
    /// 現在の手番を返す。最初は0から始まる
    /// 
    /// </summary>
    /// <returns>現在の手番</returns>
    public uint getTurns()
    {
        return Turns;
    }

    /// <summary>
    /// 
    /// 現在の手番の色と逆の色を返す
    /// 
    /// </summary>
    /// <returns>現在の手番の色と逆の色</returns>
    public DiscColor getInverseCurrentColor()
    {
        if(CurrentColor == DiscColor.WHITE) return DiscColor.BLACK;
        else if(CurrentColor == DiscColor.BLACK) return DiscColor.WHITE;

        return DiscColor.EMPTY;
    }

    /// <summary>
    /// 
    /// 指定方向の石を探索しひっくり返せるものを探す
    /// 
    /// </summary>
    /// <param name="nDir">ひっくり返せる方向</param>
    /// <param name="dir">探索方向</param>
    /// <param name="point">座標情報</param>
    private void checkFlipDirection(int nDir, Direction dir,in Point point) 
    { 
        int x = point.x;
        int y = point.y;

        if((nDir & (int)dir) != 0)
        {
            while(true)
            {
                switch (dir)
                {
                    case Direction.UPPER:
                        AddIntXY(ref x, ref y, 0, -1); 
                        break;
                    case Direction.LOWER:
                        AddIntXY(ref x, ref y, 0,  1);
                        break;
                    case Direction.LEFT :
                        AddIntXY(ref x, ref y, -1, 0); 
                        break;
                    case Direction.RIGHT:
                        AddIntXY(ref x, ref y, 1, 0);
                        break;
                    case Direction.UPPER_RIGHT:
                        AddIntXY(ref x, ref y, 1, -1);
                        break;
                    case Direction.UPPER_LEFT:
                        AddIntXY(ref x, ref y, -1, -1);
                        break;
                    case Direction.LOWER_LEFT:
                        AddIntXY(ref x, ref y, -1,  1);                      
                        break;
                    case Direction.LOWER_RIGHT:
                        AddIntXY(ref x, ref y, 1, 1);                       
                        break;
                }
                if (RawBoard[x, y] != CurrentColor)
                {
                    RawBoard[x, y] = CurrentColor;
                    update.Add(new Disc(x, y, CurrentColor));
                }
                else
                {
                    break;
                }
            }            
        }
    }

    /// <summary>
    /// 
    /// pointで指定された位置に石を打ち、挟み込めるすべての石を裏返す。
    /// 「打った石」と「裏返した石」をUpdateLogに挿入する。
    /// 
    /// </summary>
    /// <param name="point"></param>
    private void flipDiscs(in Point point)
    {
        int dir = (int)MovableDir[Turns, point.x, point.y];

        update = new List<Disc>();
        RawBoard[point.x, point.y] = CurrentColor;
        update.Add(new Disc(point.x, point.y, CurrentColor));

        checkFlipDirection(dir,Direction.UPPER , point);
        checkFlipDirection(dir,Direction.LOWER , point);
        checkFlipDirection(dir,Direction.RIGHT , point);
        checkFlipDirection(dir,Direction.LEFT , point);
        checkFlipDirection(dir,Direction.UPPER_RIGHT , point);
        checkFlipDirection(dir,Direction.UPPER_LEFT , point);
        checkFlipDirection(dir,Direction.LOWER_RIGHT , point);
        checkFlipDirection(dir, Direction.LOWER_LEFT , point);

        int discdiff = update.Count;

        Discs[CurrentColor] += (uint)discdiff;
        Discs[getInverseCurrentColor()] -= (uint)discdiff - 1;
        Discs[DiscColor.EMPTY]--;

        UpdateLog.Add(update);
    }

    /// <summary>
    /// 
    /// 指定した座標からどの方向にひっくり返せるかを調べる
    /// 
    /// </summary>
    /// <param name="disc">石情報</param>
    /// <param name="dir">探索方向</param>
    private void checkMobilityDirection(in Disc disc, Direction dir)
    {
        int x, y;
        x = disc.x;
        y = disc.y;
        int xDir, yDir;
        xDir = 0;
        yDir = 0;
        switch (dir)
        {
            case Direction.UPPER:
                SetIntXY(ref xDir, ref yDir, 0, -1);
                break;
            case Direction.LOWER:
                SetIntXY(ref xDir, ref yDir, 0, 1);
                break;
            case Direction.LEFT:
                SetIntXY(ref xDir, ref yDir, -1, 0);
                break;
            case Direction.RIGHT:
                SetIntXY(ref xDir, ref yDir, 1, 0);
                break;
            case Direction.UPPER_RIGHT:
                SetIntXY(ref xDir, ref yDir, 1, -1);
                break;
            case Direction.UPPER_LEFT:
                SetIntXY(ref xDir, ref yDir, -1, -1);
                break;
            case Direction.LOWER_LEFT:
                SetIntXY(ref xDir, ref yDir, -1,  1);
                break;
            case Direction.LOWER_RIGHT:
                SetIntXY(ref xDir, ref yDir, 1, 1);
                break;
            default: 
                break;
        }

        if ((int)RawBoard[disc.x + xDir, disc.y + yDir] == -(int)disc.color)
        {
            x = disc.x + 2 * xDir;
            y = disc.y + 2 * yDir;
            while ((int)RawBoard[x, y] == -(int)disc.color)
            {
                x += xDir;
                y += yDir;
            }
            if ((int)RawBoard[x, y] == (int)disc.color) CheckDirection |= (uint)dir;
        }
    }

    /// <summary>
    /// 
    /// discで指定された座標に、disc.colorの石を打てるかどうか、また、
    /// どの方向に石を裏返されるか判定する。
    /// 石を裏返せる方向にフラグが立った整数値を返る。
    /// 
    /// </summary>
    /// <param name="disc">石情報</param>
    /// <returns>ひっくり返せる方向</returns>
    private uint checkMobility(in Disc disc)
    {
        // 既に石があったら置けない
        if (RawBoard[disc.x,disc.y] != DiscColor.EMPTY)
        {
            return (uint)Direction.NONE;
        }
        CheckDirection = (uint)Direction.NONE;

        checkMobilityDirection(disc, Direction.UPPER);
        checkMobilityDirection(disc, Direction.LOWER);
        checkMobilityDirection(disc, Direction.RIGHT);
        checkMobilityDirection(disc, Direction.LEFT);
        checkMobilityDirection(disc, Direction.UPPER_RIGHT);
        checkMobilityDirection(disc, Direction.UPPER_LEFT);
        checkMobilityDirection(disc, Direction.LOWER_RIGHT);
        checkMobilityDirection(disc, Direction.LOWER_LEFT);

        return CheckDirection;
    }

    /// <summary>
    /// 
    /// MovablePos[Turns]とMovableDir[Turns]を再計算する
    /// 
    /// </summary>
    private void initMovable()
    {
        Disc disc;
        int dir;

        if (MovablePos[Turns] != null)
        MovablePos[Turns].Clear();

        for(int x = 1; x <= BOARD_SIZE ; x++)
        {
            for(int y = 1; y <= BOARD_SIZE ; y++)
            {
                disc = new Disc(x, y, CurrentColor);
                dir = (int)checkMobility(disc);
                if(dir != (int)Direction.NONE)
                {
                    MovablePos[Turns].Add(disc);
                }
                MovableDir[Turns, x, y] = (uint)dir;
            }
        }
    }

    /// <summary>
    /// 
    /// 引数で渡したint型二つをそれぞれ引数で指定した値に変更する
    /// checkMobilityDirectionにてコード整理のため使用
    /// 
    /// </summary>
    /// <param name="x">更新したいint型1</param>
    /// <param name="y">更新したいint型2</param>
    /// <param name="setX">更新先1</param>
    /// <param name="setY">更新先2</param>
    public void SetIntXY(ref int x, ref int y, int setX, int setY)
    {
        x = setX;
        y = setY;
    }
    /// <summary>
    /// 
    /// 引数で渡したint型二つをそれぞれ引数で指定した分加算する
    /// checkFlipDirectionにてコード整理のため使用
    /// 
    /// </summary>
    /// <param name="x">加算したいint型1</param>
    /// <param name="y">加算したいint型2</param>
    /// <param name="addX">加算する値1</param>
    /// <param name="addY">加算する値2</param>
    public void AddIntXY(ref int x, ref int y, int addX, int addY)
    {
        x += addX;
        y += addY;
    }

    /// <summary>
    /// 
    /// 周囲の空きマスの数を返す
    /// 
    /// </summary>
    /// <param name="p">座標</param>
    /// <returns>空きマスの数</returns>
    public int getLiberty(Point p)
    {
        return liberty[p.x, p.y];
    }

    /// <summary>
    /// 
    /// これまでに打たれてきた手を並べlistを返す
    /// 
    /// </summary>
    /// <returns>これまでに打たれてきた手のList</returns>
    public List<Point> getHistory()
    {
        List<Point> history = new List<Point>();

        for(int i = 0; i < UpdateLog.Count; i++)
        {
            List<Disc> update = UpdateLog[i];
            if (update.Count == 0) continue;
            history.Add(update[0]);
        }

        return history;
    }
}
