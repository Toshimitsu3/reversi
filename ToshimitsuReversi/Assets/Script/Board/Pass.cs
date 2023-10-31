using UnityEngine;

/// <summary>
/// 
/// パス行うクラス
/// 
/// </summary>
public class Pass : MonoBehaviour
{
    /// <summary>
    /// 
    /// クリック時に呼び出される
    /// 
    /// </summary>
    public void OnClick()
    {
        GameManager.Instance.HumanPass();
    }
}
