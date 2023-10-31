using Sfs2X;
using Sfs2X.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// スマートフォックスサーバーとの通信を管理するクラス
/// 
/// </summary>
public class GlobalManager : MonoBehaviour
{
    private static GlobalManager ins; // インスタンス

    /// <summary>
    /// 
    /// インスタンスの取得
    /// 
    /// </summary>
    public static GlobalManager Instance 
    {
        get
        {
            if( ins == null )
                ins = new GameObject("GlobalManager").AddComponent<GlobalManager>();
        
            return ins;
        }
    }

    private SmartFox sfs; // サーバー通信用スマートフォックスクラス
   
    /// <summary>
    /// 
    /// グローバルマネージャー生成時に呼び出される
    /// 
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad( this );

        Application.runInBackground = true;

        Debug.Log("グローバルマネージャーの準備完了");
    }

    /// <summary>
    /// 
    /// 更新処理
    /// 
    /// </summary>
    // Update is called once per frame
    private void Update()
    {
        if(sfs!= null ) 
        { 
            sfs.ProcessEvents();
        }   
    }

    /// <summary>
    /// 
    /// オブジェクト破壊時に呼び出される
    /// 
    /// </summary>
    private void OnDestroy()
    {
        Debug.Log("グローバルマネージャーがなくなった");
    }

    /// <summary>
    /// 
    /// アプリケーション終了時に呼び出される
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        if (sfs != null && sfs.IsConnected)
            sfs.Disconnect();
    }

    /// <summary>
    /// 
    /// スマートフォックスクラスのインスタンス生成
    /// 
    /// </summary>
    /// <returns>生成したスマートフォックスクラス</returns>
    public SmartFox CreateSfsClient()
    {
        sfs = new SmartFox();
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        return sfs;
    }

    /// <summary>
    /// 
    /// スマートフォックスクラスの取得
    /// 
    /// </summary>
    /// <returns>スマートフォックスクラス</returns>
    public SmartFox GetSfsClient()
    {
        return sfs;
    }

    /// <summary>
    /// 
    /// サーバー切断時の処理
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnConnectionLost(BaseEvent evt)
    {
        sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs = null;

        string connLostReason = (string)evt.Params["reason"];
        
        Debug.Log("サーバーとの接続が切れました。");

        if (SceneManager.GetActiveScene().name != "TitleScene")
        {
            SceneManagementClass.Instance.LoadTitleScene();
        }
    }
}
