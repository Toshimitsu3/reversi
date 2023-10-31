using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;
using UnityEngine;

/// <summary>
/// 
/// BaseSceneControllerクラスを継承したゲームシーンの通信部分を管理するクラス
/// 
/// </summary>
public class GameSceneController : BaseSceneController
{
    public bool bOnLine; // オンラインオフラインのフラグ
    private SmartFox sfs;// 通信に使用するスマートフォックスクラス

    // Start is called before the first frame update
    void Start()
    {
        bOnLine = SceneManagementClass.Instance.IsOnLine();
        Debug.Log(bOnLine);
        if(gm.GetSfsClient() != null)
        {
            sfs = gm.GetSfsClient();
        }
        else
        {
            sfs = null;
        }

        if(bOnLine)
        {
            AddSmartFoxListeners();
            GameManager.Instance.InitOnlineGame(sfs);
        }
        else
        {
            GameManager.Instance.InitOfflineGame();
        }
    }

    /// <summary>
    /// 
    /// サーバーイベントの追加
    /// 
    /// </summary>
    private void AddSmartFoxListeners()
    {
        sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
    }

    /// <summary>
    /// 
    /// サーバーイベントの削除
    /// 
    /// </summary>
    override protected void RemoveSmartFoxListeners()
    {
        sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
    }

    /// <summary>
    /// 
    /// ユーザーがルームに入った時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];

        Debug.Log(user.Name + "が参加しました。");
    }

    /// <summary>
    /// 
    /// ユーザーがルームを抜けたときのイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnUserExitRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];

        if (user != sfs.MySelf)
        {
            Debug.Log(user.Name + "が退室しました。");
        }
    }

    /// <summary>
    /// 
    /// やめるボタンクリック時
    /// 
    /// </summary>
    public void OnQuitButtonClick()
    {
        // Leave current game room
        sfs.Disconnect();
        // Return to lobby scene
        SceneManagementClass.Instance.LoadTitleScene();
    }

    /// <summary>
    /// 
    /// タイトルに戻るボタンクリック時
    /// 
    /// </summary>
    public void OnBackTitleButtonClick()
    {
        sfs.Disconnect();
        // Leave current game room
        SceneManagementClass.Instance.LoadTitleScene();
    }
}
