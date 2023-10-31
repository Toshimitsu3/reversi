using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Match;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Requests.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// サーバーとの通信をするロビーシーンのクラス
/// 
/// </summary>
public class LobbySceneController : BaseSceneController
{
    public static string DEFAULT_ROOM = "The Lobby"; // 最初のルーム名
    public static string GAME_ROOM = "games";    // ゲームをするルーム名

    private SmartFox sfs; // スマートフォックスクラス

    private const string EXTENSION_ID = "ToshimitsuReversi-JS"; // 使用するエクステンションのフォルダ名
    private const string EXTENSION_CLASS = "ToshimitsuReversiExtension.js"; // 使用するエクステンションファイル名

    //private const string EXTENSION_ID = "TicTacToe-JS"; // 仮のエクステンションフォルダ名（削除予定）
    //private const string EXTENSION_CLASS = "TicTacToeExtension.js"; // 仮のエクステンションファイル名（削除予定）

    public static string USERVAR_EXPERIENCE = "exp"; // ルーム作成時に使用
    public static string USERVAR_RANKING = "rank";   // ルーム作成時に使用
    
    /// <summary>
    /// 
    /// 起動時
    /// 
    /// </summary>
    // Start is called before the first frame update
    private void Start()
    {
        sfs = gm.GetSfsClient();
        InitPlayerProfile();
        AddSmartFoxListeners();

        UIManager.Instance.InitLobbyUI();

        // ロビーに参加
        sfs.Send(new JoinRoomRequest(DEFAULT_ROOM));
    }

    /// <summary>
    /// 
    /// 更新処理
    /// 
    /// </summary>
    private void Update()
    {
        UIManager.Instance.UpdateMatchingReadyUI();
    }

    /// <summary>
    /// 
    /// プレイヤー情報の初期化
    /// 
    /// </summary>
    private void InitPlayerProfile()
    {
        if (sfs.MySelf.GetVariable(USERVAR_EXPERIENCE) == null || sfs.MySelf.GetVariable(USERVAR_RANKING) == null)
        {
            SFSUserVariable expVar = new SFSUserVariable(USERVAR_EXPERIENCE, "Novice");
            SFSUserVariable rankVar = new SFSUserVariable(USERVAR_RANKING, 3);

            sfs.Send(new SetUserVariablesRequest(new List<UserVariable>() { expVar, rankVar }));
        }
    }

    /// <summary>
    /// 
    /// 前のシーンに戻るボタン
    /// 
    /// </summary>
    public void OnBackSceneButtonClick()
    {
        sfs.Disconnect();
        SceneManagementClass.Instance.LoadTitleScene();
    }

    /// <summary>
    /// 
    /// マッチング開始ボタンクリック時
    /// 
    /// </summary>
    public void OnMatchingButtonClick()
    {
        // 名前を入力済みならマッチング開始する
        if(UIManager.Instance.IsInput())
        {
            string roomName = sfs.MySelf.Name + "'s game";

            SFSGameSettings settings = new SFSGameSettings(roomName);
            settings.GroupId = GAME_ROOM;
            settings.MaxUsers = 2;
            settings.MaxSpectators = 0;
            settings.MinPlayersToStartGame = 2;
            settings.IsPublic = true;
            settings.LeaveLastJoinedRoom = true;
            settings.NotifyGameStarted = false;
            settings.Extension = new RoomExtension(EXTENSION_ID, EXTENSION_CLASS);
            var matchExp = new MatchExpression(USERVAR_EXPERIENCE, StringMatch.EQUALS, sfs.MySelf.GetVariable(USERVAR_EXPERIENCE).GetStringValue());
            matchExp.And(USERVAR_RANKING, NumberMatch.GREATER_OR_EQUAL_THAN, sfs.MySelf.GetVariable(USERVAR_RANKING).GetIntValue());
            MatchExpression exp = new MatchExpression(RoomProperties.IS_GAME, BoolMatch.EQUALS, false)
                                         .And(RoomProperties.HAS_FREE_PLAYER_SLOTS, BoolMatch.EQUALS, true);

            settings.PlayerMatchExpression = matchExp;

            sfs.Send(new QuickJoinOrCreateRoomRequest(exp, new List<string>() { GAME_ROOM }, settings, sfs.LastJoinedRoom));
            UIManager.Instance.StartMatchingUI();
        }
        else
        {
            UIManager.Instance.DrawInputErrorUI();
        }
    }

    /// <summary>
    /// 
    /// イベントの追加
    /// 
    /// </summary>
    private void AddSmartFoxListeners()
    {
        sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
        sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
        sfs.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
    }

    /// <summary>
    /// 
    /// イベントの削除
    /// 
    /// </summary>
    protected override void RemoveSmartFoxListeners()
    {
        sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
        sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
        sfs.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
    }

    /// <summary>
    /// 
    /// ルームが増えたとき
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnRoomAdded(BaseEvent evt)
    {
        Debug.Log("部屋が追加されました。");
    }

    /// <summary>
    /// 
    /// ルームがなくなったとき
    /// 
    /// </summary>
    /// <param name="evt"></param>
    public void OnRoomRemoved(BaseEvent evt)
    {
        Debug.Log("部屋がなくなりました。");
    }

    /// <summary>
    /// 
    /// ルームに人が増えたとき
    /// 
    /// </summary>
    /// <param name="evt"></param>
    public void OnUserCountChanged(BaseEvent evt)
    {
        Debug.Log("新しいプレイヤーが入室しました。");
        
        Room room = (Room)evt.Params["room"];

        if (room.UserCount == room.MaxUsers)
        {
            Debug.Log("マッチング成功");
            SceneManager.LoadScene("GameScene");
        }

    }

    /// <summary>
    /// 
    /// ルーム参加時
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnRoomJoin(BaseEvent evt)
    {
        Debug.Log("ルームに参加");   
    }

    /// <summary>
    /// 
    /// ルーム参加失敗時
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnRoomJoinError(BaseEvent evt)
    {
        Debug.Log("部屋の参加に失敗しました。");
    }

    /// <summary>
    /// 
    /// ルーム作成失敗時
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnRoomCreationError(BaseEvent evt)
    {
        Debug.Log("部屋の作成に失敗しました。");
    }
}
