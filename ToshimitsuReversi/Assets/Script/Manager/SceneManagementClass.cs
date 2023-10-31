using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// シーン遷移を管理するクラス
/// 
/// </summary>
public class SceneManagementClass : MonoBehaviour
{
    private static SceneManagementClass Ins; // ゲームマネージャーのインスタンス

    private static bool bOnLine; // オンラインオフラインの判定

    /// <summary>
    /// 
    /// インスタンスの取得
    /// 
    /// </summary>
    public static SceneManagementClass Instance
    {
        get
        {
            if (Ins == null)
            {
                SceneManagementClass find = FindFirstObjectByType(typeof(SceneManagementClass),
                    FindObjectsInactive.Include) as SceneManagementClass;
                if (find == null)
                {
                    GameObject obj = new GameObject("SceneManager");
                    Ins = obj.AddComponent<SceneManagementClass>();
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
    /// オンラインオフライン判定
    /// 
    /// </summary>
    /// <returns>判定結果</returns>
    public bool IsOnLine()
    {
        return bOnLine;
    }

    /// <summary>
    /// 
    /// 最初の一度のみ動く
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
    public static void CheckInstance(SceneManagementClass check)
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
    /// ゲームを開始する
    /// 
    /// </summary>
    public void LoadGameScene()
    {
        UIManager.Instance.SetGameParam();
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 
    /// ローカル対戦シーンをロードする
    /// 
    /// </summary>
    public void LoadLocalScene()
    {
        bOnLine = false;
        SceneManager.LoadScene("LocalScene");
    }

    /// <summary>
    /// 
    /// ロビーをロードする
    /// 
    /// </summary>
    public void LoadLobbyScene()
    {
        bOnLine = true;
        SceneManager.LoadScene("LobbyScene");
    }

    /// <summary>
    /// 
    /// タイトルシーンをロードする
    /// 
    /// </summary>
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
