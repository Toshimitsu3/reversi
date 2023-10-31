using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// ボタンを盤面に敷き詰めるためのクラス
/// 
/// </summary>
public class SetButton : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid; // ボタンを並べて生成するために使用
    [SerializeField] private DiscButton discButton; //  複製を作るためのボタン
    private DiscButton[,] buttonArray = new DiscButton[Board.BOARD_SIZE + 2, Board.BOARD_SIZE + 2]; // 生成したボタンの配列
    
    /// <summary>
    /// 
    /// ボタンを盤面に敷き詰める
    /// 
    /// </summary>
    public void SetBottonAll() 
    {
        for (int y = 0; y < Board.BOARD_SIZE + 2; y++)
        {
            for (int x = 0; x < Board.BOARD_SIZE + 2;x ++)
            {
                DiscButton clonebutton = Instantiate(discButton, grid.transform);
                buttonArray[x, y] = clonebutton;
            }
        }
    }

    /// <summary>
    /// 
    /// 生成したボタンの配列の情報を渡す
    /// 
    /// </summary>
    /// <returns>ボタン（升目）の情報の配列</returns>
    public DiscButton[,] GetButtonArray()
    {
        return buttonArray;
    }
}
