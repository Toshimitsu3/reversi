using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReversiBoard : MonoBehaviour
{
    static bool playerturn = true;

    private Board board;// 盤面の情報を持つクラス 
    private DiscButton[,] buttonArray = new DiscButton[Board.BOARD_SIZE + 2, Board.BOARD_SIZE + 2]; // 盤面と合わせたボタンの配列
    private List<Point> movablePos = new List<Point>(); // 設置可能な座標情報をを格納
    private AI ai;

    [SerializeField] private SetButton setButton; // ボタンを配置するクラス
    [SerializeField] private Text countBlackText; // 黒石の数を表示するテキスト
    [SerializeField] private Text countWhiteText; // 白石の数を表示するテキスト
    [SerializeField] private Text winnerText;     // 勝者を表示するテキスト
    [SerializeField] private Image winnerImage;   // 勝者を表示するテキストの背景
    [SerializeField] private Image blackTurnImage;// 黒のターンであることを示すイメージ
    [SerializeField] private Image whiteTurnImage;// 白のターンであることを示すイメージ

    // Start is called before the first frame update
    void Start()
    {
        board = new Board();
        setButton.SetBottonAll();
        buttonArray = setButton.GetButtonArray();

       // ai = new AlphaBetaAI();

        // UIの初期化
        winnerText.enabled = false;
        winnerImage.enabled = false;
        blackTurnImage.enabled = true;
        whiteTurnImage.enabled = false;

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
        ChangeDiscColor();
        UpdateUI();
    }

    /// <summary>
    /// 
    /// 石を設置しその後の処理をする。
    /// 
    /// </summary>
    /// <param name="point"></param>    
    public void SetDisc(in Point point)
    {
        if(playerturn)
        {
            if(board.Move(point))
            {
                ChangeDiscColor();
                UpdateUI();

                // ゲーム終了チェック
                if (board.isGameOver())
                {
                    winnerImage.enabled = true;
                    winnerText.enabled = true;

                    uint blackDisc = board.countDisc(DiscColor.BLACK);
                    uint whiteDisc = board.countDisc(DiscColor.WHITE);
                    if (blackDisc > whiteDisc)
                    {
                        winnerText.text = ("BLACK WIN!");
                        Debug.Log("黒の勝ち" + blackDisc);
                    }
                    else if (blackDisc < whiteDisc)
                    {
                        winnerText.text = ("WHITE WIN!");
                        Debug.Log("白の勝ち" + whiteDisc);
                    }
                    else
                    {
                        winnerText.text = ("DRAW!");
                        Debug.Log("引き分け");
                    }
                }
                playerturn = false;
            }
        }
        else
        {
            ai.move(in board);
            ChangeDiscColor();
            UpdateUI();

            // ゲーム終了チェック
            if (board.isGameOver())
            {
                winnerImage.enabled = true;
                winnerText.enabled = true;

                uint blackDisc = board.countDisc(DiscColor.BLACK);
                uint whiteDisc = board.countDisc(DiscColor.WHITE);
                if (blackDisc > whiteDisc)
                {
                    winnerText.text = ("BLACK WIN!");
                    Debug.Log("黒の勝ち" + blackDisc);
                }
                else if (blackDisc < whiteDisc)
                {
                    winnerText.text = ("WHITE WIN!");
                    Debug.Log("白の勝ち" + whiteDisc);
                }
                else
                {
                    winnerText.text = ("DRAW!");
                    Debug.Log("引き分け");
                }
            }
            playerturn = true;
        } 
    }

    /// <summary>
    /// 
    /// 呼ばれたらパスを試す
    /// 
    /// </summary>
    public void Pass()
    {
        if(board.Pass())
        {
            ai.move(board);
            ChangeDiscColor();
            UpdateUI();
        }
    }

    /// <summary>
    /// 
    /// 呼ばれたら巻き戻しを試す
    /// 
    /// </summary>
    public void Undo()
    {
        if(board.Undo())
        {
            ChangeDiscColor();
            UpdateUI();          
        }
    }

    /// <summary>
    /// 
    /// 石の描画する色を変える
    /// 
    /// </summary>
    public void ChangeDiscColor()
    {
        for (int y = 0; y < Board.BOARD_SIZE + 2; y++)
        {
            for (int x = 0; x < Board.BOARD_SIZE + 2; x++)
            {
                buttonArray[x, y].SetColor(board.getColor(new Point(x, y)));
            }
        }

        movablePos = board.getMovablePos();
        for (int i = 0; i < movablePos.Count; i++)
        {
            Point[] p = new Point[movablePos.Count];
            p[i] = movablePos[i];
            buttonArray[p[i].x, p[i].y].MovableHighLightColor(board.getCurrentColor());
        }
    }

    /// <summary>
    /// 
    /// 盤面の石の数を表示するUIを更新する
    /// 
    /// </summary>
    public void UpdateUI()
    {
        countBlackText.text = ("BLACK " + board.countDisc(DiscColor.BLACK));
        countWhiteText.text = ("WHITE " + board.countDisc(DiscColor.WHITE));
        if (board.getCurrentColor() == DiscColor.BLACK)
        {
            blackTurnImage.enabled = true;
            whiteTurnImage.enabled = false;
        }
        else
        {
            blackTurnImage.enabled = false;
            whiteTurnImage.enabled = true;
        }
    }

    /// <summary>
    /// 
    /// Boradクラスを渡す
    /// 
    /// </summary>
    /// <returns></returns>
    public Board GetBoard()
    {
        return board;
    }
}
