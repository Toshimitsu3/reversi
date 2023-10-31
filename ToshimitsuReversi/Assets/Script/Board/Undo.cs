using UnityEngine;

/// <summary>
/// 
/// 巻き戻し処理をするクラス
/// 
/// </summary>
public class Undo : MonoBehaviour
{
   /// <summary>
    /// 
    /// クリック時に呼び出される
    /// 
    /// </summary>
    public void OnClick()
    {
        GameManager.Instance.Undo();
    }
}
