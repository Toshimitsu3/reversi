/// <summary>
/// 
/// 木構造を表すクラス
/// 
/// </summary>
[System.Serializable]
public class Node 
{ 
    public Node child = null; // 子供ノード
    public Node sibling = null; // 兄弟ノード
    public Point point = new Point(); // 座標情報
}

