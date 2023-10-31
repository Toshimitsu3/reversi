using static MidEvaluator;
/// <summary>
/// 
/// 中盤の評価関数クラス
/// 
/// </summary>
[System.Serializable]
public class MidEvaluator : Evaluator
{
    /// <summary>
    /// 
    /// 辺に関するパラメータ
    /// 
    /// </summary>
    public class EdgeParam
    { 
        /// <summary>
        /// 
        /// パラメーターを加算する
        /// 
        /// </summary>
        /// <param name="src">加算する数</param>
        /// <returns>加算後のパラメータ</returns>
        public EdgeParam add(EdgeParam src)
        {
            stableNum += src.stable;
            wingNum += src.wing;
            mountainNum += src.mountain;
            cMoveNum += src.cMove;

            return this;
        }

        /// <summary>
        /// 
        /// パラメータを格納する
        /// 
        /// </summary>
        /// <param name="e">格納するパラメータ</param>
        public void set(EdgeParam e)
        {
            stableNum = e.stable;
            wingNum = e.wing;
            mountainNum = e.mountain;
            cMoveNum = e.cMove;
        }

        private byte stable = 0;   // 確定石の個数
        private byte wing   = 0;   // ウィングの個数
        private byte mountain = 0; // 山の個数
        private byte cMove    = 0; // 危険なC打ちの個数

        /// <summary>
        /// 
        /// 確定石の個数を表す変数のゲッターセッター
        /// 
        /// </summary>
        public byte stableNum
        {
            get
            {
                return stable;
            }
            set
            {
                stable = value;
            }
        }

        /// <summary>
        /// 
        /// ウィングの個数を表す変数のゲッターセッター
        /// 
        /// </summary>
        public byte wingNum
        {
            get
            {
                return wing;
            }
            set
            {
                wing = value;
            }
        }

        /// <summary>
        /// 
        /// 山の個数を表す変数のゲッターセッター
        /// 
        /// </summary>
        public byte mountainNum
        {
            get
            {
                return mountain;
            }
            set
            {
                mountain = value;
            }
        }

        /// <summary>
        /// 
        /// 危険なC打ちの個数を表す変数のゲッターセッター
        /// 
        /// </summary>
        public byte cMoveNum
        {
            get
            {
                return cMove;
            }
            set
            {
                cMove = value;
            }
        }
    }

    /// <summary>
    /// 
    /// 隅周辺に関するパラメータを集めたクラス
    /// 
    /// </summary>
    public class CornerParam
    {
        public byte corner = 0; // 隅にある石の数
        public byte xMove = 0;  // 危険なX打ちの個数
    }

    /// <summary>
    /// 
    /// 色別のEdgeParamオブジェクトを管理するクラス
    /// 
    /// </summary>
    public class EdgeStat
    {
        private EdgeParam[] data = new EdgeParam[3]; // 辺に関するパラメータのクラスの配列
        
        /// <summary>
        /// 
        /// コンストラクタ
        /// 
        /// </summary>
        public EdgeStat()
        {
            for(int i = 0; i < 3; i++) data[i] = new EdgeParam();
        }

        /// <summary>
        /// 
        /// パラメータの加算
        /// 
        /// </summary>
        /// <param name="e">パラメータ配列に加える</param>
        public void add(EdgeStat e)
        {
            for(int i = 0; i < 3; i++) data[i].add(e.data[i]);
        }

        /// <summary>
        /// 
        /// 辺に関するパラメータの取得
        /// 
        /// </summary>
        /// <param name="color">現在の手番の色</param>
        /// <returns>辺に関するパラメータ</returns>
        public EdgeParam get(DiscColor color)
        {
            return data[(int)color + 1];
        }
    }

    /// <summary>
    /// 
    /// 色別のCornerParamオブジェクトを管理するクラス
    /// 
    /// </summary>
    public class CornerStat
    {
        private CornerParam[] data = new CornerParam[3]; // 隅に関するパラメータのクラスの配列 

        /// <summary>
        /// 
        /// コンストラクタ
        /// 
        /// </summary>
        public CornerStat()
        {
            for(int i = 0; i < 3; i++) data[i] = new CornerParam();
        }

        /// <summary>
        /// 
        /// 隅に関するパラメータの取得
        /// 
        /// </summary>
        /// <param name="color">石の色</param>
        /// <returns>隅に関するパラメータ</returns>
        public CornerParam get(DiscColor color)
        {
            return data[(int)color + 1];
        }
    }

    /// <summary>
    /// 
    /// 重みの係数を規定する
    /// 
    /// </summary>
    public class Weight
    {
        public int mobility_w; // 着手可能手数の重み
        public int liberty_w;  // 開放度の重み
        public int stable_w;   // 確定石の重み
        public int wing_w;     // ウィングの重み
        public int xMove_w;    // X打ちの重み
        public int cMove_w;    // C打ちの重み
    }

    private Weight evalWeight = new Weight(); // 重み係数をまとめたクラス
    private const int TABLE_SIZE = 6561; // 3の8乗
    private static EdgeStat[] edgeTable = new EdgeStat[TABLE_SIZE]; // 色別の辺情報をまとめた配列
    private static bool tableInit = false; // 初回起動用フラグ

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    public MidEvaluator()
    {
        if(tableInit == false)
        {
            DiscColor[] line = new DiscColor[Board.BOARD_SIZE];
            generateEdge(line, 0);

            tableInit = true;
        }

        // 重み係数の設定(全局面共通)
        evalWeight = new Weight();
        evalWeight.mobility_w = 67;
        evalWeight.liberty_w = -13;
        evalWeight.stable_w = 101;
        evalWeight.wing_w = -308;
        evalWeight.xMove_w = -449;
        evalWeight.cMove_w = -552;
    }

    /// <summary>
    /// 
    /// 中盤の評価関数
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>評価</returns>
    public override int evaluate(in Board board)
    {
        EdgeStat edgeStat = new EdgeStat();
        CornerStat cornerStat = new CornerStat();
        int result;

        // 辺の評価
        edgeStat.add(edgeTable[idxTop(board)]);
        edgeStat.add(edgeTable[idxBottom(board)]);
        edgeStat.add(edgeTable[idxRight(board)]);
        edgeStat.add(edgeTable[idxLeft(board)]);

        // 隅の評価
        cornerStat = evalCorner(board);

        // 確定石に関して。隅の石を２回数えてしまっているので補正
        edgeStat.get(DiscColor.BLACK).stableNum -= cornerStat.get(DiscColor.BLACK).corner;
        edgeStat.get(DiscColor.WHITE).stableNum -= cornerStat.get(DiscColor.WHITE).corner;

        // パラメータの線形結合
        result =
            edgeStat.get(DiscColor.BLACK).stableNum * evalWeight.stable_w
          - edgeStat.get(DiscColor.WHITE).stableNum * evalWeight.stable_w
          + edgeStat.get(DiscColor.BLACK).wingNum * evalWeight.wing_w
          - edgeStat.get(DiscColor.WHITE).wingNum * evalWeight.wing_w
          + cornerStat.get(DiscColor.BLACK).xMove * evalWeight.xMove_w
          - cornerStat.get(DiscColor.WHITE).xMove * evalWeight.xMove_w
          + edgeStat.get(DiscColor.BLACK).cMoveNum * evalWeight.cMove_w
          - edgeStat.get(DiscColor.WHITE).cMoveNum * evalWeight.cMove_w;

        // 開放度・着手可能手数の評価
        if(evalWeight.liberty_w != 0)
        {
            ColorStorage<uint> liberty = countLiberty(board);
            result += (int)liberty[DiscColor.BLACK] * evalWeight.liberty_w;
            result -= (int)liberty[DiscColor.WHITE] * evalWeight.liberty_w;
        }

        // 現在の手番の色についてのみ、着手可能手数を数える
        result += (int)board.getCurrentColor() * board.getMovablePos().Count * evalWeight.mobility_w;
        return (int)board.getCurrentColor() * result;
    }

    /// <summary>
    /// 
    /// 評価値のテーブルの作成
    /// 
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="count"></param>
    private void generateEdge(DiscColor[] edge, int count)
    {
        if(count == Board.BOARD_SIZE)
        {
            // このパターンは完成したので、局面のカウント
            EdgeStat stat = new EdgeStat();

            stat.get(DiscColor.BLACK).set(evalEdge(edge, DiscColor.BLACK));
            stat.get(DiscColor.WHITE).set(evalEdge(edge, DiscColor.WHITE));

            edgeTable[idxLine(edge)] = stat;

            return;
        }

        // 再帰的にすべてのパターンを網羅
        edge[count] = DiscColor.EMPTY;
        generateEdge(edge, count + 1);

        edge[count] = DiscColor.BLACK;
        generateEdge(edge, count + 1);

        edge[count] = DiscColor.WHITE;
        generateEdge(edge, count + 1);

        return;
    }

    /// <summary>
    /// 
    /// 辺のパラメータを数える
    /// 
    /// </summary>
    /// <param name="line">辺の石の色</param>
    /// <param name="color">石の色</param>
    /// <returns>辺のパラメータ</returns>
    public EdgeParam evalEdge(DiscColor[] line, DiscColor color)
    {
        EdgeParam edgeParam = new EdgeParam();
        
        int x;

        //ウィング等のカウント
        if (line[0] == DiscColor.EMPTY && line[7] == DiscColor.EMPTY)
        {
            x = 2;
            while(x <= 5)
            {
                if (line[x] != color) break;
                x++;
            }
            if(x == 6) // 少なくともブロックができている
            {
                if (line[1] == color && line[6] == DiscColor.EMPTY)
                    edgeParam.wingNum = 1;
                else if (line[1] == DiscColor.EMPTY && line[6] == color)
                    edgeParam.wingNum = 1;
                else if (line[1] == color && line[6] == color)
                    edgeParam.mountainNum = 1;
            }
            else // それ以外の場合に、隅に隣接する位置に置いていたら
            {
                if (line[1] == color)
                    edgeParam.cMoveNum++;
                if (line[6] == color)
                    edgeParam.cMoveNum++; 
            }
        }

        // 確定石のカウント
        // 左から右方向に走査
        for(x = 0; x < 8; x++)
        {
            if (line[x] != color) break;
            edgeParam.stableNum++;
        }

        if(edgeParam.stableNum < 8)
        {
            // 右側からの走査も必要
            for(x = 7; x > 0; x--)
            {
                if (line[x] != color) break;
                edgeParam.stableNum++;
            }
        }

        return edgeParam;
    }

    /// <summary>
    /// 
    /// 隅のパラメータを調べる。この関数は各評価に使用
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>隅のパラメータ</returns>
    public CornerStat evalCorner(in Board board)
    {
        CornerStat cornerStat = new CornerStat();

        cornerStat.get(DiscColor.BLACK).corner = 0;
        cornerStat.get(DiscColor.BLACK).xMove  = 0;
        cornerStat.get(DiscColor.WHITE).corner = 0;
        cornerStat.get(DiscColor.WHITE).xMove  = 0;

        Point p = new Point();

        // 左上
        p.x = 1;
        p.y = 1;
        AddCornerParam(board, ref cornerStat, p);

        // 左下
        p.x = 1;
        p.y = 8;
        AddCornerParam(board, ref cornerStat, p);

        // 右下
        p.x = 8;
        p.y = 8;
        AddCornerParam(board, ref cornerStat, p);

        // 右上
        p.x = 8;
        p.y = 1;
        AddCornerParam(board, ref cornerStat, p);

        return cornerStat;
    }

    /// <summary>
    /// 
    /// 隅の評価のパラメータを足す
    /// 
    /// 
    /// </summary>
    /// <param name="board"></param>
    /// <param name="stat"></param>
    /// <param name="point"></param>
    public void AddCornerParam(in Board board, ref CornerStat stat, Point point)
    {
        stat.get(board.getColor(point)).corner++;
        if (board.getColor(point) == DiscColor.EMPTY)
        {
            point = ChangeCornerPoint(point);
            stat.get(board.getColor(point)).xMove++;
        }
    }

    /// <summary>
    /// 
    /// X打ちの座標に変える
    /// 
    /// </summary>
    /// <param name="point">隅の座標</param>
    /// <returns>X打ちの座標</returns>
    public Point ChangeCornerPoint(Point point)
    {
        if (point.x == 1) point.x = 2;
        else if (point.x == 7) point.x = 8;
        if (point.y == 1) point.y = 2;
        else if (point.y == 7) point.y = 8;
        return point;
    }

    /// <summary>
    /// 
    /// 最上段のインデックス計算
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>最上段辺のインデックス</returns>
    public int idxTop(in Board board)
    {
        int index = 0;

        index =  2187 * (int)(board.getColor(new Point(1, 1)) + 1)
                + 729 * (int)(board.getColor(new Point(2, 1)) + 1)
                + 243 * (int)(board.getColor(new Point(3, 1)) + 1)
                +  81 * (int)(board.getColor(new Point(4, 1)) + 1)
                +  27 * (int)(board.getColor(new Point(5, 1)) + 1)
                +   9 * (int)(board.getColor(new Point(6, 1)) + 1)
                +   3 * (int)(board.getColor(new Point(7, 1)) + 1)
                +   1 * (int)(board.getColor(new Point(8, 1)) + 1);

        return index;
    }

    /// <summary>
    /// 
    /// 最下段のインデックス計算
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>最下段辺のインデックス</returns>
    public int idxBottom(in Board board)
    {
        int index = 0;

        index =  2187 * (int)(board.getColor(new Point(1, 8)) + 1)
                + 729 * (int)(board.getColor(new Point(2, 8)) + 1)
                + 243 * (int)(board.getColor(new Point(3, 8)) + 1)
                +  81 * (int)(board.getColor(new Point(4, 8)) + 1)
                +  27 * (int)(board.getColor(new Point(5, 8)) + 1)
                +   9 * (int)(board.getColor(new Point(6, 8)) + 1)
                +   3 * (int)(board.getColor(new Point(7, 8)) + 1)
                +   1 * (int)(board.getColor(new Point(8, 8)) + 1);

        return index;
    }

    /// <summary>
    /// 
    /// 最右列のインデックス計算
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>最右列のインデックス</returns>
    public int idxRight(in Board board)
    {
        int index = 0;

        index =  2187 * (int)(board.getColor(new Point(8, 1)) + 1)
                + 729 * (int)(board.getColor(new Point(8, 2)) + 1)
                + 243 * (int)(board.getColor(new Point(8, 3)) + 1)
                +  81 * (int)(board.getColor(new Point(8, 4)) + 1)
                +  27 * (int)(board.getColor(new Point(8, 5)) + 1)
                +   9 * (int)(board.getColor(new Point(8, 6)) + 1)
                +   3 * (int)(board.getColor(new Point(8, 7)) + 1)
                +   1 * (int)(board.getColor(new Point(8, 8)) + 1);

        return index;
    }

    /// <summary>
    /// 
    /// 最左列のインデックス計算
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>最左列のインデックス</returns>
    public int idxLeft(in Board board)
    {
        int index = 0;

        index =  2187 * (int)(board.getColor(new Point(1, 1)) + 1)
                + 729 * (int)(board.getColor(new Point(1, 2)) + 1)
                + 243 * (int)(board.getColor(new Point(1, 3)) + 1)
                +  81 * (int)(board.getColor(new Point(1, 4)) + 1)
                +  27 * (int)(board.getColor(new Point(1, 5)) + 1)
                +   9 * (int)(board.getColor(new Point(1, 6)) + 1)
                +   3 * (int)(board.getColor(new Point(1, 7)) + 1)
                +   1 * (int)(board.getColor(new Point(1, 8)) + 1);

        return index;
    }

    /// <summary>
    /// 
    /// 開放度を調べる
    /// 
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private ColorStorage<uint> countLiberty(in Board board)
    {
        ColorStorage<uint> liberty = new ColorStorage<uint>();
        liberty[DiscColor.BLACK] = 0;
        liberty[DiscColor.WHITE] = 0;
        liberty[DiscColor.EMPTY] = 0;

        for(int x = 1; x <= Board.BOARD_SIZE; x++)
        {
            for(int y = 1; y <= Board.BOARD_SIZE; y++)
            {
                Point p = new Point(x, y);
                liberty[board.getColor(p)] += (uint)board.getLiberty(p);
            }
        }
        return liberty;
    }

    /// <summary>
    /// 
    /// 引数の辺のインデックスを返す
    /// 
    /// </summary>
    /// <param name="l">辺の色情報</param>
    /// <returns>引数の辺のインデックス</returns>
    private int idxLine(DiscColor[] l)
    {
        int i = 3 * (3 * (3 * (3 * (3 * (3 * (3 * ((int)l[0] + 1) + (int)l[1] + 1) + (int)l[2] + 1) + (int)l[3] + 1)
            + (int)l[4] + 1) + (int)l[5] + 1) + (int)l[6] + 1) + (int)l[7] + 1;
        return i;
    }
}
