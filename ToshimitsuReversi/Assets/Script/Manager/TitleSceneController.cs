using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Util;
using UnityEngine;

/// <summary>
/// 
/// サーバーとの通信をする選択画面のシーンのクラス
/// 
/// </summary>
public class TitleSceneController : BaseSceneController
{
    [SerializeField] public string host = "sfs2x.m-craft.com"; // ホストアドレス
    [SerializeField] public int tcpPort = 9933;        // TCPポート番号
    [SerializeField] public int httpPort = 8080;       // HTTPポート番号
    [SerializeField] public string zone = "Toshimitsu"; // サーバーのゾーン名

    private SmartFox sfs; // スマートフォックスクラス

    /// <summary>
    /// 
    /// オンラインプレイ選択時の処理
    /// 
    /// </summary>
    public void OnOnLinePlayButtonClick()
    {
        Connect();
    }

    /// <summary>
    /// 
    /// オフラインプレイ選択時の処理
    /// 
    /// </summary>
    public void OnOffLinePlayButtonClick()
    {
        SceneManagementClass.Instance.LoadLocalScene();
    }


    /// <summary>
    /// 
    /// サーバーとの接続を図る
    /// 
    /// </summary>
    private void Connect()
    {
        ConfigData cfg = new ConfigData();
        cfg.Host = host;
        cfg.Port = tcpPort;
        cfg.Zone = zone;

        sfs = gm.CreateSfsClient();
        AddSmartFoxListeners();
        sfs.Connect(cfg);
    }

    /// <summary>
    /// 
    /// イベントの追加
    /// 
    /// </summary>
    private void AddSmartFoxListeners()
    {
        sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
    }

    /// <summary>
    /// 
    /// イベント削除
    /// 
    /// </summary>
    protected override void RemoveSmartFoxListeners()
    {
        if(sfs != null)
        {
            sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        }
    }

    /// <summary>
    /// 
    /// サーバーとの接続時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnConnection(BaseEvent evt)
    {
        if ((bool)evt.Params["success"])
        {
            sfs.Send(new LoginRequest("User" + (int)Random.Range(1, 10000)));
            Debug.Log("接続成功");
        }
        else
        {
            Debug.Log("接続失敗");
        }
    }

    /// <summary>
    /// 
    /// サーバーとの切断時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnConnectionLost(BaseEvent evt)
    {
        RemoveSmartFoxListeners();
        string reason = (string)evt.Params["reason"];
        if(reason != ClientDisconnectionReason.MANUAL)
        {
            Debug.Log("サーバーとの接続が切れました。：" + reason);
        }
        if (sfs != null)
            sfs = null;
    }

    /// <summary>
    /// 
    /// ログイン成功時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnLogin(BaseEvent evt)
    {
        SceneManagementClass.Instance.LoadLobbyScene();
    }

    /// <summary>
    /// 
    /// ログイン失敗時のイベント
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnLoginError(BaseEvent evt)
    {
        sfs.Disconnect();

        Debug.Log("接続に失敗しました。");
    }
}
