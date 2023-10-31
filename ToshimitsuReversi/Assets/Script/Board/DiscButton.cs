using UnityEngine;
using UnityEngine.UI;

public class DiscButton : MonoBehaviour
{
    private Disc disc = new Disc(); // ボタンに付随する石情報
    [SerializeField] private ReversiBoard reversiBoard; // 盤面を操作するためのクラス
    [SerializeField] private Image discObj; // 石を表すイメージ
    [SerializeField] private Image buttonImage; // ボタン（マス）を表すイメージ
    [SerializeField] private Image movableHighLightImage; // 次を置ける石をハイライトするためのイメージ

    // Start is called before the first frame update
    void Start()
    {
        if(disc.color == DiscColor.WALL)
        {
            buttonImage.color = Color.clear;
        }
    }

    /// <summary>
    /// 
    /// ボタンクリック時に呼ばれるメソッド
    /// 
    /// </summary>
    public void OnClick()
    {
        // 石の設置を試みる
        GameManager.Instance.HumanPlayerMove(disc);
    }

    /// <summary>
    /// 
    /// Discの情報を格納する
    /// 
    /// </summary>
    /// <param name="x">座標情報x</param>
    /// <param name="y">座標情報y</param>
    /// <param name="color">石の色</param>
    public void SetDisc(int x, int y, DiscColor color)
    {
        disc.x = x;
        disc.y = y;
        disc.color = color;
    }

    /// <summary>
    /// 
    /// 石の色の変更をする
    /// 
    /// </summary>
    /// <param name="color">石の色</param>
    public void SetColor(DiscColor color)
    {
        disc.color = color;
        switch (disc.color)
        {
            case DiscColor.WHITE:
                discObj.color = Color.white;
                break;
            case DiscColor.BLACK:
                discObj.color = Color.black;
                break;
            case DiscColor.EMPTY:
                discObj.color = Color.clear;
                break;
            case DiscColor.WALL:
                discObj.color = Color.clear;
                break;
        }
        movableHighLightImage.color = Color.clear;
    }

    /// <summary>
    /// 
    /// 次打てる位置をハイライトする。
    /// 
    /// </summary>
    /// <param name="currentColor">現在の手番の色</param>
    public void MovableHighLightColor(DiscColor currentColor)
    {
        if(currentColor == DiscColor.BLACK)
        {
            movableHighLightImage.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        }
        else if(currentColor == DiscColor.WHITE)
        {
            movableHighLightImage.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        }
    }
}
