using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// オブジェクトとしての盤面を管理するクラス
/// 
/// </summary>
public class BoardObject : MonoBehaviour
{
    [SerializeField] private SetButton setbotton; // ボタン設置クラス
    private DiscButton[,] buttonArray = new DiscButton[Board.BOARD_SIZE + 2, Board.BOARD_SIZE + 2]; // 盤面と合わせたボタンの配列

    /// <summary>
    /// 
    /// 盤面の初期化
    /// 
    /// </summary>
    /// <param name="board">盤面の情報</param>
    public void Init(Board board)
    {
        setbotton.SetBottonAll();
        buttonArray = setbotton.GetButtonArray();
        // 石の情報を保持
        for (int y = 0; y < Board.BOARD_SIZE + 2; y++)
        {
            for (int x = 0; x < Board.BOARD_SIZE + 2; x++)
            {
                // DiscButtonに初期値を与える
                buttonArray[x, y].SetDisc(x, y, board.getColor(new Point(x, y)));
                buttonArray[x, y].SetColor(board.getColor(new Point(x, y)));
            }
        }
        UpdateDiscColor(board);
    }

    /// <summary>
    /// 
    /// 石の描画する色を更新する
    /// 
    /// </summary>
    public void UpdateDiscColor(Board board)
    {
        for (int y = 0; y < Board.BOARD_SIZE + 2; y++)
        {
            for (int x = 0; x < Board.BOARD_SIZE + 2; x++)
            {
                buttonArray[x, y].SetColor(board.getColor(new Point(x, y)));
            }
        }

        List<Point> movablePos = board.getMovablePos(); // 設置可能な座標情報をを格納
        for (int i = 0; i < movablePos.Count; i++)
        {
            Point[] p = new Point[movablePos.Count];
            p[i] = movablePos[i];
            buttonArray[p[i].x, p[i].y].MovableHighLightColor(board.getCurrentColor());
        }
    }
}
