using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// alphabeta法を用いたAIクラス
/// 
/// </summary>
public class AlphaBetaAI : AI
{
    private Evaluator evaluator; // 使用する評価関数クラス

    public AlphaBetaAI(int level): base(level)
    {

    }

    /// <summary>
    /// 
    /// 石の設置を行う
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    public override void move(in Board board)
    {
        BookManager book = new BookManager();
        List<Point> movables = book.find(board);

        if (movables.Count == 0)
        {
            // 打てる箇所がなければパスする
            board.Pass();
            return;
        }
        if (movables.Count == 1)
        {
            // 打てる箇所が一か所だけなら探索は行わず、即座に打って返る。
            board.Move(movables[0]);
            return;
        }

        int limit;
        evaluator = new MidEvaluator();
        Sort(in board, ref movables, (int)presearch_depth); // 事前に手をよさそうな順でソート
        
        if (Board.MAX_TURNS - board.getTurns() <= wld_depth)
        {
            limit = int.MaxValue;
            if (Board.MAX_TURNS - board.getTurns() <= perfect_depth)
                evaluator = new PerfectEvaluator();
            else
                evaluator = new WLDEvaluator();
        }
        else
        {
            limit = (int)normal_depth;
        }

        int eval, alpha;
        eval = int.MinValue;
        alpha = int.MinValue;
        Point p = null;
        for (int i = 0; i < movables.Count; i++)
        {
            board.Move(movables[i]);
            eval = -AlphaBeta(board, limit - 1, int.MinValue, int.MaxValue);
            board.Undo();
            
            if (eval > alpha)
            {
                alpha = eval;
                p = movables[i];
            }
        }
        Debug.Log(evaluator);
        board.Move(p);
    }

    /// <summary>
    /// 
    /// 評価値の大きい順にソートする
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <param name="movables">設置可能位置</param>
    /// <param name="limit">深さ制限</param>
    private void Sort(in Board board, ref List<Point> movables, int limit)
    {
        List<Move> moves = new List<Move>();

        for (int i = 0; i < movables.Count; i++)
        {
            int eval;

            board.Move(movables[i]);
            eval = -AlphaBeta(board, limit - 1, -int.MaxValue, int.MaxValue);
            board.Undo();

            Move move = new Move(movables[i].x, movables[i].y, eval);
            moves.Add(move);
        }

        // 評価値の大きい順にソート
        int begin, current;
        for(begin = 0; begin < moves.Count - 1;begin++)
        {
            for(current = begin + 1; current < moves.Count; current++)
            {
                Move b = moves[begin];
                Move c = moves[current];
                if(b.eval < c.eval)
                {
                    moves[begin] = c;
                    moves[current] = b;
                }
            }
        }

        // 結果の巻き戻し
        movables.Clear();
        for(int i = 0; i < moves.Count; i++)
        {
            movables.Add(moves[i]);
        }

        return;
    }

    /// <summary>
    /// 
    /// alphabeta法を用いた探索を行う
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <param name="limit">深さ制限</param>
    /// <param name="alpha">下限値</param>
    /// <param name="beta">上限値</param>
    /// <returns>評価値</returns>
    private int AlphaBeta(Board board, int limit, int alpha, int beta) 
    {
        // 深さ制限に達したら評価値を返す
        if(board.isGameOver() || limit == 0)
        {
            return evaluator.evaluate(board);
        }

        int score;
        List<Point> movables = board.getMovablePos();

        if (movables.Count == 0)
        {
            board.Pass();
            score = -AlphaBeta(board, limit, -beta, -alpha);
            board.Undo();
            return score;
        }

        for (int i = 0; i < movables.Count; i++)
        {
            board.Move(movables[i]);
            score = -AlphaBeta(board, limit - 1, -beta, -alpha);
            board.Undo();
            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
            {
                return alpha;
            }
        } 
        return alpha;
    }
}
