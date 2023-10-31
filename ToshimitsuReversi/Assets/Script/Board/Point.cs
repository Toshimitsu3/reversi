/// <summary>
/// 
/// 座標を扱うためのクラス
/// 
/// </summary>
[System.Serializable]
public class Point
{
    public int x; // 盤面上のx座標
    public int y; // 盤面上のy座標

    /// <summary>
    /// 
    /// デフォルト引数付きコンストラクタ
    /// 
    /// </summary>
    /// <param name="x">盤面上のx座標</param>
    /// <param name="y">盤面上のy座標</param>
    public Point(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }  

    /// <summary>
    /// 
    /// 文字列から座標情報に変換
    /// 
    /// </summary>
    /// <param name="coord">座標を表す文字列</param>
    /// <exception cref="System.Exception"></exception>
    public Point(string coord)
    {
        if (coord == null || coord.Length < 2) throw new System.Exception();
        x = coord[0] - 'a' + 1;
        y = coord[1] - '1' + 1;
    }

    /// <summary>
    /// 
    /// 座標情報から文字列に変換
    /// 
    /// </summary>
    /// <returns>座標を表す文字列</returns>
    public string toString()
    {
        string coord = new string("");
        coord += (char)('a' + x - 1);
        coord += (char)('1' + y - 1);

        return coord;
    }
}
