using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 
/// 定石探索を行うクラス
/// 
/// </summary>
[System.Serializable]
public class BookManager
{
    // private string path = "Resources_moved/reversibook.txt"; // ファイルパス
    private static Node root = null; // 定石木の根
    private static bool bOnce = false; // 最初に呼ばれたときのみ実行するフラグ

    public BookManager()
    { 
        if(!bOnce)
        {
            root = new Node();
            root.point = new Point("f5");
            string[] line = null;
            line = GameManager.Instance.GetLoadedText().Split("\r\n");
          
            foreach (var text in line)
            {
                List<Point> book = new List<Point>();
                for (int i = 0; i < text.Length; i += 2)
                {
                    Point p;
                    try
                    {
                        p = new Point(text.Substring(i));
                    }
                    catch
                    {
                        UnityEngine.Debug.Log("文字列の取得に失敗");
                        break;
                    }
                    book.Add(p);
                }
                add(book);
            }
            bOnce = true;
        }    
    }

    /// <summary>
    /// 
    /// 定石手があるならその手のみ、
    /// ないなら配置できる手が入ったListを返す
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    /// <returns>打てる手を並べたList</returns>
    public List<Point> find(in Board board)
    {
        Node node = root;
        List<Point> history = board.getHistory();

        if (history.Count == 0) return board.getMovablePos();
        Point first = history[0];
        CoordinatesTransformer transformer = new CoordinatesTransformer(first);

        // 座標を変換してからf5から始まるようにする
        List<Point> normalized = new List<Point>();
        for(int i = 0; i < history.Count;i++) 
        { 
            Point p = history[i];
            p = transformer.normalize(p);

            normalized.Add(p);
        }

        // 現在までの棋譜リストと定石の対応をとる
        for(int i = 1; i < normalized.Count;i++) 
        { 
            Point p = normalized[i];

            node = node.child;
            while(node != null)
            {
                if(node.point.x == p.x && node.point.y == p.y) break;

                node = node.sibling;
            }
            if(node == null)
            {
                // 定石を外れている
                return board.getMovablePos();
            }
        }

        // 履歴と定石の終わりが一致していた場合
        if (node.child == null) return board.getMovablePos();
        Point next_move = getNextMove(node);

        // 座標を元の形に変換する
        next_move = transformer.denormalize(next_move);

        List<Point> v = new List<Point>();
        v.Add(next_move);

        return v;
    }

    /// <summary>
    /// 
    /// 次の一手を決める
    /// 
    /// </summary>
    /// <param name="node">現在の手数の位置に対応するノード</param>
    /// <returns>次の一手</returns>
    private Point getNextMove(Node node)
    {
        List<Point> candidates = new List<Point>();

        for(Node p = node.child; p != null; p = p.sibling)
        {
            candidates.Add(p.point);
        }

        System.Random rand = new System.Random();
        int index = rand.Next() % candidates.Count;
        
        return candidates[index];
    }

    /// <summary>
    /// 
    /// bookで指定された定石を定石木に追加する
    /// 
    /// </summary>
    /// <param name="book">定石の座標が並んだList</param>
    public void add(in List<Point> book)
    {
        Node node = root;

        for(int i = 1; i < book.Count; i++)
        {
            Point p = book[i];
            
            if(node.child == null)
            {
                // 新しい定石手
                node.child = new Node();
                node = node.child;
                node.point.x = p.x;
                node.point.y = p.y;
            }
            else
            {
                // 兄弟ノードの探索に移る
                node = node.child;

                while(true)
                {
                    // 既にこの手はデータベース中にあり、その枝を見つけた
                    if (node.point.x == p.x && node.point.y == p.y) break;

                    // 定石木の新しい枝
                    if(node.sibling == null) 
                    {
                        node.sibling = new Node();

                        node = node.sibling;
                        node.point.x = p.x;
                        node.point.y = p.y;
                        break;
                    }

                    node = node.sibling;
                }
            }
        }
    }
}
