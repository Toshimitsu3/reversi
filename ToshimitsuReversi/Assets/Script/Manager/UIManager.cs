using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// UIを管理するクラス
/// 
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager Ins; // UIManagerのインスタンス

    private static int P1aiLevel = 0; // P1側のAIの強さ
    private static int P2aiLevel = 0; // P2側のAIの強さ
    private static bool P1AI = false; // P1側にAIが座っているか
    private static bool P2AI = false; // P2側にAIが座っているか
    private static string playerName; // 自分の名前
    private static string enemyName;  // 相手の名前

    private float time = 0; // 時間経過計測用
    private int cnt = 0;    // UI遷移用のカウンタ

    [Header("LocalScene")]
    [SerializeField] private Slider selectPlayer1; // プレイヤーがAIか人かを切り替えるスライダー2P用
    [SerializeField] private Slider selectPlayer2; // プレイヤーがAIか人かを切り替えるスライダー2P用
    [SerializeField] private Slider selectAILevel1;// AIの強さを決めるスライダー1P用
    [SerializeField] private Slider selectAILevel2;// AIの強さを決めるスライダー2P用
    [SerializeField] private Text Who1PText;       // 選択されている1Pを表示するテキスト
    [SerializeField] private Text Who2PText;       // 選択されている2Pを表示するテキスト

    [Header("GameScene")]
    [SerializeField] private Text countBlackText; // 黒石の数を表示するテキスト
    [SerializeField] private Text countWhiteText; // 白石の数を表示するテキスト
    [SerializeField] private Text winnerText;     // 勝者を表示するテキスト
    [SerializeField] private Text AIThinkingUIText;// AIの思考中に表示するテキスト
    [SerializeField] private Image blackTurnBlurImage;// 黒のターンであることを示すイメージ
    [SerializeField] private Image whiteTurnBlurImage;// 白のターンであることを示すイメージ
    [SerializeField] private GameObject backTitleButton; // タイトルに戻るボタン
    [SerializeField] private GameObject UndoButton; // Undoボタンオブジェクト
    [SerializeField] private GameObject QuitButton; // Quitボタンオブジェクト
    [SerializeField] private Image blurImage; // ゲーム終了時のぼかし
    [SerializeField] private Text BlackNameText; // 黒手番の名前
    [SerializeField] private Text WhiteNameText; // 白手番の名前
    [SerializeField] private Text WinByDefaultText; // 不戦勝時のUI

    [Header("LobbyScene")]
    [SerializeField] private Text MatchingReadyText; // マッチング中テキスト
    [SerializeField] private GameObject MatchingStartButton; // マッチを開始するボタン
    [SerializeField] private InputField inputField;  // 名前を入力するフィールド
    [SerializeField] private Text inputErrorText;    // エラーメッセージ

    /// <summary>
    /// 
    /// インスタンスの取得
    /// 
    /// </summary>
    public static UIManager Instance
    {
        get
        {
            if (Ins == null)
            {
                UIManager find = FindFirstObjectByType(typeof(UIManager),
                    FindObjectsInactive.Include) as UIManager;
                if (find == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    Ins = obj.AddComponent<UIManager>();
                }
                else
                {
                    CheckInstance(find);
                }
            }
            else
            {
                return Ins;
            }
            return Ins;
        }
    }

    /// <summary>
    /// 
    /// プレイヤー１のAIレベルを返す
    /// 
    /// </summary>
    public int GetP1aiLevel
    {
        get { return P1aiLevel; }
    }

    /// <summary>
    /// 
    /// プレイヤー２のAIレベルを返す
    /// 
    /// </summary>
    public int GetP2aiLevel
    {
        get { return P2aiLevel; }
    }

    /// <summary>
    /// 
    /// プレイヤー１がAIかどうかを返す
    /// 
    /// </summary>
    public bool GetP1AI
    {
        get { return P1AI; }
    }

    /// <summary>
    /// 
    /// プレイヤー２がAIかどうかを返す
    /// 
    /// </summary>
    public bool GetP2AI
    {
        get { return P2AI; }
    }

    /// <summary>
    /// 
    /// 一度だけ行われる
    /// 
    /// </summary>
    private void Awake()
    {
        CheckInstance(this);
    }

    /// <summary>
    /// 
    /// インスタンスがあるかの確認
    /// 
    /// </summary>
    /// <param name="check">取得したインスタンス</param>
    public static void CheckInstance(UIManager check)
    {
        if (Ins == null)
        {
            Ins = check;
        }
        else if (Ins != check)
        {
            DestroyImmediate(check.gameObject);
        }
    }

    /// <summary>
    /// 
    /// ゲームシーンのUIの初期化
    /// 
    /// </summary>
    /// <param name="board">盤面の情報</param>
    public void InitGameUI(Board board)
    {
        UpdateCountDiscUI(board);
        countBlackText.enabled = true;
        countWhiteText.enabled = true;
        winnerText.enabled = false;
        blackTurnBlurImage.enabled = false;
        whiteTurnBlurImage.enabled = true;
        backTitleButton.SetActive(false);
        AIThinkingUIText.enabled = false;
        UndoButton.SetActive(true);
        QuitButton.SetActive(false);
        blurImage.enabled = false;
        WinByDefaultText.enabled = false;
    }

    /// <summary>
    /// 
    /// オンラインゲームシーンのUIの初期化
    /// 
    /// </summary>
    /// <param name="board">盤面の情報</param>
    public void InitOnlineGameUI(Board board)
    {
        UpdateCountDiscUI(board);
        countBlackText.enabled = true;
        countWhiteText.enabled = true;
        winnerText.enabled = false;
        blackTurnBlurImage.enabled = false;
        whiteTurnBlurImage.enabled = true;
        backTitleButton.SetActive(false);
        AIThinkingUIText.enabled = false;
        UndoButton.SetActive(false);
        QuitButton.SetActive(true);
        blurImage.enabled = false;
        WinByDefaultText.enabled = false;
    }

    /// <summary>
    /// 
    /// 盤面の石の数を表示するUIを更新する
    /// 
    /// </summary>
    public void UpdateGameUI(Board board)
    {
        UpdateCountDiscUI(board);
        if (board.getCurrentColor() == DiscColor.BLACK)
        {
            blackTurnBlurImage.enabled = false;
            whiteTurnBlurImage.enabled = true;
        }
        else
        {
            blackTurnBlurImage.enabled = true;
            whiteTurnBlurImage.enabled = false;
        }
    }

    /// <summary>
    /// 
    /// AI思考中のテキストを表示する
    /// 
    /// </summary>
    public void OnAIThinkingUI()
    {
        AIThinkingUIText.enabled = true;
    }

    /// <summary>
    /// 
    /// AI思考中のテキストを非表示する
    /// 
    /// </summary>
    public void OffAIThinkingUI()
    {
        AIThinkingUIText.enabled = false;
    }

    /// <summary>
    /// 
    /// 石の数を表示するUIを更新する
    /// 
    /// </summary>
    /// <param name="board">盤面情報</param>
    public void UpdateCountDiscUI(Board board)
    {
        countBlackText.text = "" + board.countDisc(DiscColor.BLACK);
        countWhiteText.text = "" + board.countDisc(DiscColor.WHITE);
    }

    /// <summary>
    /// 
    /// ゲーム終了時に表示するUI
    /// 
    /// </summary>
    /// <param name="board">盤面の情報</param>
    public void DrawGameOverUI(Board board)
    {
        winnerText.enabled = true;
        backTitleButton.SetActive(true);
        blurImage.enabled = true;

        uint blackDisc = board.countDisc(DiscColor.BLACK);
        uint whiteDisc = board.countDisc(DiscColor.WHITE);
        if (blackDisc > whiteDisc)
        {
            blackTurnBlurImage.enabled = false;
            whiteTurnBlurImage.enabled = true;
            winnerText.text = ("BLACK WIN!");
        }
        else if (blackDisc < whiteDisc)
        {
            blackTurnBlurImage.enabled = true;
            whiteTurnBlurImage.enabled = false;
            winnerText.text = ("WHITE WIN!");
        }
        else
        {
            blackTurnBlurImage.enabled = false;
            whiteTurnBlurImage.enabled = false;
            winnerText.text = ("DRAW!");
        }
    }

    /// <summary>
    /// 
    /// タイトルのUIからゲームのパラメータを取得する
    /// 
    /// </summary>
    public void SetGameParam()
    {
        P1aiLevel = (int)selectAILevel1.value;
        P2aiLevel = (int)selectAILevel2.value;
        if((int)selectPlayer1.value == 0)
        {
            P1AI = false; 
        }
        else
        {
            P1AI = true;
        }
        if ((int)selectPlayer2.value == 0)
        {
            P2AI = false;
        }
        else
        {
            P2AI = true;
        }
    }

    /// <summary>
    /// 
    /// 1Pプレイヤーの表示をスライダーで選択されているものに変える
    /// 
    /// </summary>
    public void Change1PText()
    {
        if((int)selectPlayer1.value == 0)
        {
            Who1PText.text = "Player";
        }
        else
        {
            Who1PText.text = "AI";
        }
    }

    /// <summary>
    /// 
    /// 2Pプレイヤーの表示をスライダーで選択されているものに変える
    /// 
    /// </summary>
    public void Change2PText()
    {
        if ((int)selectPlayer2.value == 0)
        {
            Who2PText.text = "Player";
        }
        else
        {
            Who2PText.text = "AI";
        }
    }

    /// <summary>
    /// 
    /// マッチング中のUIを変化させる
    /// 
    /// </summary>
    public void UpdateMatchingReadyUI()
    {
        if(MatchingReadyText.enabled == true)
        {
            MatchingReadyText.text = "マッチング中";
            time += Time.deltaTime;
            if(time >= 1.0f)
            {
                cnt++;
                time = 0.0f;
            }
            int num = cnt % 3 + 1;
            for (int i = 0; i < num;i++)
            {
                MatchingReadyText.text += ".";
            }
        }
    }

    /// <summary>
    /// 
    /// マッチング開始時のUI変更
    /// 
    /// </summary>
    public void StartMatchingUI()
    {
        MatchingStartButton.SetActive(false);
        MatchingReadyText.enabled = true;
    }

    /// <summary>
    /// 
    /// マッチングキャンセル時のUI変更
    /// 
    /// </summary>
    public void CancelMatchingUI()
    {
        MatchingStartButton.SetActive(true);
        MatchingReadyText.enabled = false;
    }

    /// <summary>
    /// 
    /// オンラインプレイ時のプレイヤーネームの表示の初期化
    /// 
    /// </summary>
    /// <param name="yourturn">自分の手番番号</param>
    public void InitOnlineNameTextUI(int yourturn)
    {
        if(yourturn == 0)
        {
            BlackNameText.text = playerName;
            WhiteNameText.text = enemyName;
        }
        else
        {
            BlackNameText.text = enemyName;
            WhiteNameText.text = playerName;
        }
    }

    /// <summary>
    /// 
    /// オフラインプレイ時のプレイヤーネームの表示の初期化
    /// 
    /// </summary>
    /// <param name="black">黒手番の</param>
    /// <param name="white"></param>
    public void InitOfflineNameTextUI(Player.PlayerKind player1, Player.PlayerKind player2)
    {
        if(player1 == Player.PlayerKind.HUMAN_PLAYER)
        {
            BlackNameText.text = "Player1";
        }
        else
        {
            BlackNameText.text = "AI";
        }

        if(player2 == Player.PlayerKind.HUMAN_PLAYER)
        {
            WhiteNameText.text = "Player2";
        }
        else
        {
            WhiteNameText.text = "AI";
        }
    }

    /// <summary>
    /// 
    /// 勝敗のUIの表示
    /// 
    /// </summary>
    /// <param name="winlose">勝敗</param>
    public void DrawYouWinLoseTextUI(bool winlose)
    {
        if(winlose == true)
        {
            winnerText.text = "YOU WIN!!";
        }
        else
        {
            winnerText.text = "YOU LOSE..";
        }
    }

    /// <summary>
    /// 
    /// 相手の切断時のUI表示
    /// 
    /// </summary>
    /// <param name="board"></param>
    public void WinByDefaultUI(Board board)
    {
        DrawGameOverUI(board);
        DrawYouWinLoseTextUI(true);
        WinByDefaultText.enabled = true;
        WinByDefaultText.text = "対戦相手との接続が切れました。";
    }

    /// <summary>
    /// 
    /// 名前が設定されているかどうか
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsInput()
    {
        if(string.IsNullOrEmpty(playerName))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 
    /// ロビーシーンUIの初期化
    /// 
    /// </summary>
    public void InitLobbyUI()
    {
        inputField.text = playerName;
    }

    /// <summary>
    /// 
    /// インプットフィールドに入力時
    /// 
    /// </summary>
    public void OnInputName()
    {
        playerName = inputField.text;
    }

    /// <summary>
    /// 
    /// 入力エラー表示
    /// 
    /// </summary>
    public void DrawInputErrorUI()
    {
        inputErrorText.enabled = true;
    }

    /// <summary>
    /// 
    /// プレイヤー名取得
    /// 
    /// </summary>
    /// <returns>プレイヤー名</returns>
    public string GetName()
    {
        return playerName;
    }

    /// <summary>
    /// 
    /// 対戦相手の名前のゲットセット
    /// 
    /// </summary>
    public string EnemyName
    {
        get
        {
            return enemyName;
        }
        set
        {
             enemyName = value;
        }
    }
}
