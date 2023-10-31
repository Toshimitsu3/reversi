using UnityEngine;

/// <summary>
/// 
/// サーバとの通信をするシーンの制御をするクラスのベース
/// 
/// </summary>
public abstract class BaseSceneController : MonoBehaviour
{
    protected GlobalManager gm; // グローバルマネージャーのインスタンス

    /// <summary>
    /// 
    /// 生成時の処理
    /// 
    /// </summary>
    protected virtual void Awake()
    {
        gm = GlobalManager.Instance;
    }

    /// <summary>
    /// 
    /// 破壊時に処理
    /// 
    /// </summary>
    protected virtual void onDestroy()
    {
        RemoveSmartFoxListeners();
    }

    /// <summary>
    /// 
    /// スマートフォックスのイベントの削除
    /// 
    /// </summary>
    protected abstract void RemoveSmartFoxListeners();
}
