using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 
/// ゲーム進行を管理するクラス
/// 
/// </summary>
public class GameManager : MonoBehaviour
{
    private const int MAX_PLAYER = 2;       // 最大プレイヤー人数
    private const float LIMIT_TIME = 30.0f; // オンラインプレイ時のタイムアウトまでの時間　

    private static GameManager Ins;         // ゲームマネージャーのインスタンス

    private int currentTurn = 0;            // 現在の手番
    private Board board = new Board();      // 盤面の情報
    private Player[] player = null;         // AIか人間かを判別するためのクラス
    [SerializeField] private BoardObject boardObject;  // オブジェクトとしての盤面
    private bool switchTurn = false;                   // ターンの切り替えを感知するフラグ
    [SerializeField] private AssetReference reference; // アセットのアドレス
    private TextAsset asset;            // テキストアセット格納用
    private static string lines = null; // 読み込んだテキスト格納用
    private float limitTimer;           // タイムアウトまでの時間計測用
    private bool bGameEnd = false;      // ゲームが終了したかどうか
    private bool bOnline = false;       // 通信対戦中かどうか
    private SmartFox sfs;               // 通信対戦に使用するスマートフォックスクラス
    private int yourTurn;               // 通信対戦中の自分の手番を表す変数
    private bool bEnemy;
    /// <summary>
    /// 
    /// インスタンスの取得
    /// 
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (Ins == null)
            {
                GameManager find = FindFirstObjectByType(typeof(GameManager),
                    FindObjectsInactive.Include) as GameManager;
                if (find == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    Ins = obj.AddComponent<GameManager>();
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
    /// 初回のみ
    /// 
    /// </summary>
    private void Awake()
    {
        CheckInstance(this);
    }

    /// <summary>
    /// 
    /// インスタンスがあるか確認し、なければ格納
    /// あれば破棄
    /// 
    /// </summary>
    /// <param name="check">インスタンス</param>
    public static void CheckInstance(GameManager check)
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
    /// 更新処理
    /// 
    /// </summary>
    void Update()
    {
        if (switchTurn)
        {
            if(!bOnline)
            {
                if (IsCheckAI())
                {
                    StartCoroutine(AIPlayerMove());
                }
            }          
            switchTurn = false;
        }

        // タイムアウトタイマー
        CalcTimeOut();
    }

    /// <summary>
    /// 
    /// オフラインプレイ時の初期化をする
    /// 
    /// </summary>
    public void InitOfflineGame()
    {
        bOnline = false;

        Init();
        UIManager.Instance.InitGameUI(board);

        // テキストファイルのロード
        LoadTextAsset();

        player = new Player[MAX_PLAYER];
        if (UIManager.Instance.GetP1AI)
        {
            player[0] = new AIPlayer(UIManager.Instance.GetP1aiLevel);
        }
        else
        {
            player[0] = new HumanPlayer();
        }

        if (UIManager.Instance.GetP2AI)
        {
            player[1] = new AIPlayer(UIManager.Instance.GetP2aiLevel);
        }
        else
        {
            player[1] = new HumanPlayer();
        }

        UIManager.Instance.InitOfflineNameTextUI(player[0].getKind(), player[1].getKind());

        if (IsCheckAI()) StartCoroutine(AIPlayerMove());
    }

    /// <summary>
    /// 
    /// オンラインプレイ時の初期化
    /// 
    /// </summary>
    /// <param name="smartFox">スマートフォックスクラス</param>
    public void InitOnlineGame(SmartFox smartFox)
    {
        bOnline = true;
        bEnemy = false;
        UIManager.Instance.EnemyName = null;

        sfs = smartFox;
        AddSmartFoxListeners();

        Init();
        UIManager.Instance.InitOnlineGameUI(board);

        player = new HumanPlayer[MAX_PLAYER];

        
        sfs.Send(new ExtensionRequest("start", new SFSObject(), sfs.LastJoinedRoom));

    }

    /// <summary>
    /// 
    /// 共通部分の初期化
    /// 
    /// </summary>
    public void Init()
    {
        board.Init();
        boardObject.Init(board);
        currentTurn = 0;
    }

    /// <summary>
    /// 
    /// プレイヤーの石を設置を試みる
    /// 
    /// </summary>
    /// <param name="point">座標情報</param>
    public void HumanPlayerMove(in Point point)
    {
        if (!bOnline)
        {
            if (!IsCheckAI())
            {
                if (board.Move(point))
                {
                    boardObject.UpdateDiscColor(board);
                    UIManager.Instance.UpdateGameUI(board);
                    ChangeTurn();
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            limitTimer = 0.0f;
            if (currentTurn == yourTurn && bEnemy)
            {
                if (board.Move(point))
                {
                    
                    board.Undo();
                    ISFSObject move = new SFSObject();
                    move.PutInt("x", point.x);
                    move.PutInt("y", point.y);
                    move.PutInt("t", yourTurn);
                    // Send move to Extension
                    sfs.Send(new ExtensionRequest("move", move, sfs.LastJoinedRoom));
                }
                else
                {
                    return;
                }
            }          
        }
    }

    /// <summary>
    /// 
    /// AIの石の設置を試みる
    /// 
    /// </summary>
    public IEnumerator AIPlayerMove()
    {
        UIManager.Instance.OnAIThinkingUI();
        player[currentTurn].onTurn(board);

        yield return new WaitForSeconds(0.8f);

        boardObject.UpdateDiscColor(board);
        UIManager.Instance.UpdateGameUI(board);
        UIManager.Instance.OffAIThinkingUI();
        ChangeTurn();
    }

    /// <summary>
    /// 
    /// プレイヤーが人間で打てる場所がない時パスする
    /// 
    /// </summary>
    public void HumanPass()
    {
        if(!bOnline)
        {
            if (!IsCheckAI())
            {
                if (board.Pass())
                {
                    boardObject.UpdateDiscColor(board);
                    UIManager.Instance.UpdateGameUI(board);
                    ChangeTurn();
                }
            }
        }
        else
        {
            if(currentTurn == yourTurn)
            {
                if(board.Pass())
                {
                    limitTimer = 0.0f;
                    boardObject.UpdateDiscColor(board);
                    UIManager.Instance.UpdateGameUI(board);
                    ISFSObject turn = new SFSObject();
                    turn.PutInt("t", yourTurn);
                    sfs.Send(new ExtensionRequest("pass", turn, sfs.LastJoinedRoom));
                }
            }
        }
    }

    /// <summary>
    /// 
    /// 一手巻き戻す
    /// 
    /// </summary>
    public void Undo()
    {
        if(!bOnline)
        {
            if (!IsCheckAI())
            {
                if (!IsGameOver())
                {
                    board.Undo();
                    board.Undo();
                    boardObject.UpdateDiscColor(board);
                    UIManager.Instance.UpdateGameUI(board);
                }
            }
        }    
    }

    /// <summary>
    /// 
    /// ターンを0なら1
    /// 1なら0にする
    /// 
    /// </summary>
    public void ChangeTurn()
    {
        currentTurn = 1 - currentTurn;

        if (IsGameOver())
        {
            return;
        }

        switchTurn = true;

    }

    /// <summary>
    /// 
    /// ゲームオーバーかどうかの確認をし
    /// それぞれ場合の処理を行う
    /// 
    /// </summary>
    public bool IsGameOver()
    {
        if (board.isGameOver())
        {
            UIManager.Instance.DrawGameOverUI(board);
            bGameEnd = true;
        }
        else
        {
            bGameEnd = false;
        }
        return bGameEnd;
    }

    /// <summary>
    /// 
    /// AIの手番かどうかを確認する
    /// 
    /// </summary>
    /// <returns>AIの手番ならtrue,違うならfalse</returns>
    public bool IsCheckAI()
    {
        if (player[currentTurn].getKind() == Player.PlayerKind.AI_PLAYER) return true;
        else return false;
    }

    /// <summary>
    /// 
    /// テキストファイルを読み込む
    /// 
    /// 
    /// </summary>
    public void LoadTextAsset()
    {
        try
        {
            var textAsset = Addressables.LoadAssetAsync<TextAsset>(reference).WaitForCompletion();
            asset = textAsset;
            lines = asset.text;
        }
        catch
        {
            Debug.Log("ファイル読み込みに失敗");
        }
    }

    /// <summary>
    /// 
    /// 読み込んだテキストを渡す
    /// 
    /// </summary>
    /// <returns>読み込んだテキスト</returns>
    public string GetLoadedText()
    {
        return lines;
    }

    /// <summary>
    /// 
    /// サーバーイベントの追加
    /// 
    /// </summary>
    private void AddSmartFoxListeners()
    {
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
    }

    /// <summary>
    /// 
    /// サーバーイベントの削除
    /// 
    /// </summary>
    private void RemoveSmartFoxListeners()
    {
        sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
    }

    /// <summary>
    /// 
    /// 破壊時に処理
    /// 
    /// </summary>
    private void OnDestroy()
    {
        if(sfs != null)
        RemoveSmartFoxListeners();
    }

    /// <summary>
    /// 
    /// ユーザーが通信対戦部屋に入った時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Debug.Log(user.Name + "が入室しました。");
    }

    /// <summary>
    /// 
    /// ユーザーが通信対戦部屋から抜けた時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnUserExitRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];
        if (user != sfs.MySelf && user.IsPlayer)
        {
            Debug.Log(user.Name + "が退室しました。");
        }

        if(!board.isGameOver())UIManager.Instance.WinByDefaultUI(board);
        if(room.UserCount == 1)sfs.Send(new LeaveRoomRequest());
    }

    /// <summary>
    /// 
    /// サーバーからのリクエストを受信したときのイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnExtensionResponse(BaseEvent evt)
    {
        string cmd = (string)evt.Params["cmd"];
        ISFSObject data = (SFSObject)evt.Params["params"];

        switch(cmd)
        {
            case "init":
                StartGame(data.GetInt("t"));
                break;
            case "move":
                MoveOnline(data.GetInt("t"), data.GetInt("x"), data.GetInt("y"));
                break;
            case "pass":
                PassOnline(data.GetInt("t"));
                break;
            case "name":
                NameResponse(data.GetUtfString("b"), data.GetUtfString("w"));
                break;
        }
    }

    /// <summary>
    /// 
    /// サーバーからのスタートイベント受信時
    /// 
    /// </summary>
    /// <param name="turn">自分の手番を表す番号</param>
    private void StartGame(int turn)
    {
        yourTurn = turn;

        ISFSObject name = new SFSObject();
        name.PutUtfString("n", UIManager.Instance.GetName());
        name.PutInt("t", yourTurn);
        sfs.Send(new ExtensionRequest("name", name, sfs.LastJoinedRoom));
    }

    /// <summary>
    /// 
    /// サーバーからのムーブイベント受信時
    /// 
    /// </summary>
    /// <param name="turn"></param>
    private void MoveOnline(int turn,int x, int y)
    {
        board.Move(new Point(x, y));
        currentTurn = turn;
        boardObject.UpdateDiscColor(board);
        UIManager.Instance.UpdateGameUI(board);
        if(board.isGameOver())
        {
            bGameEnd = true;
            UIManager.Instance.DrawGameOverUI(board);
            if ((yourTurn == 0 && board.countDisc(DiscColor.BLACK) > board.countDisc(DiscColor.WHITE))||
                (yourTurn == 1 && board.countDisc(DiscColor.BLACK) < board.countDisc(DiscColor.WHITE)))
                UIManager.Instance.DrawYouWinLoseTextUI(true);

            if ((yourTurn == 0 && board.countDisc(DiscColor.BLACK) < board.countDisc(DiscColor.WHITE)) ||
                (yourTurn == 1 && board.countDisc(DiscColor.BLACK) > board.countDisc(DiscColor.WHITE)))
                UIManager.Instance.DrawYouWinLoseTextUI(false);
        }
    }

    /// <summary>
    /// 
    /// サーバーからのパスイベント受信時
    /// 
    /// </summary>
    /// <param name="turn">サーバーから取得した現在の手番</param>
    private void PassOnline(int turn)
    {
        if(currentTurn != yourTurn)
        {
            board.Pass();
            boardObject.UpdateDiscColor(board);
            UIManager.Instance.UpdateGameUI(board);
        }
        currentTurn = turn;
    }

    /// <summary>
    /// 
    /// サーバーからの名前イベント受信時
    /// 
    /// </summary>
    /// <param name="name">受信した名前</param>
    private void NameResponse(string blackname, string whitename)
    {
        if(yourTurn == 0)
        {
            UIManager.Instance.EnemyName = whitename;
        }
        else
        {
            UIManager.Instance.EnemyName = blackname;
        }
        if(!string.IsNullOrEmpty(UIManager.Instance.EnemyName) && UIManager.Instance.EnemyName != "undefined")
        {
            bEnemy = true;
        }

        UIManager.Instance.InitOnlineNameTextUI(yourTurn);
    }

    /// <summary>
    /// 
    /// タイムアウトまでの時間を数え、
    /// 時間オーバーしたらタイムアウさせる。
    /// 
    /// </summary>
    private void CalcTimeOut()
    {
        // オンラインプレイ時のタイムアウト処理
        if (bOnline && currentTurn == yourTurn)
        {
            limitTimer += Time.deltaTime;
            if (limitTimer >= LIMIT_TIME)
            {
                limitTimer = 0.0f;
                sfs.Send(new LeaveRoomRequest());
                SceneManagementClass.Instance.LoadTitleScene();
            }
        }
    }
}
