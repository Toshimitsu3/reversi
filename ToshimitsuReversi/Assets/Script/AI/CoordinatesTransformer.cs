/// <summary>
/// 
/// 座標の変換および逆変換を行うためのクラス
/// 
/// </summary>
[System.Serializable]
public class CoordinatesTransformer
{
    private int rotate; // 回転角
    private bool mirror; // 反転フラグ

    /// <summary>
    /// 
    /// コンストラクタ
    /// 
    /// </summary>
    /// <param name="first">初手の座標</param>
    public CoordinatesTransformer(in Point first)
    {
        rotate = 0;
        mirror = false;

        if(first.x == 4 && first.y == 3)
        {
            rotate = 1;
            mirror = true;
        }
        else if(first.x == 3 && first.y == 4)
        {
            rotate = 2;
        }
        else if(first.x == 5 && first.y == 6)
        {
            rotate = -1;
            mirror = true;
        }
    }

    /// <summary>
    ///
    ///  座標をf5を開始点とする座標系に正規化する
    /// </summary>
    /// <param name="point">正規化前座標</param>
    /// <returns>正規化後座標</returns>
    public Point normalize(in Point point)
    {
        Point newp = rotatePoint(point, rotate);
        if (mirror) newp = mirrorPoint(newp);

        return newp;
    }

    /// <summary>
    /// 
    /// f5を開始点とする座標を本来の座標に戻す
    /// 
    /// </summary>
    /// <param name="point">正規化後座標</param>
    /// <returns>正規化前座標</returns>
    public Point denormalize(in Point point)
    {
        Point newp = new Point(point.x, point.y);
        if(mirror ) newp = mirrorPoint(newp);

        newp = rotatePoint(newp, -rotate);

        return newp;
    }

    /// <summary>
    /// 
    /// 座標の回転関数
    /// 
    /// </summary>
    /// <param name="old_point">回転前の座標</param>
    /// <param name="rotate">回転角</param>
    /// <returns>回転後の座標</returns>
    private Point rotatePoint(in Point old_point, int rotate)
    {
        rotate %= 4;
        if (rotate < 0) rotate += 4;

        Point new_point = new Point();

        switch (rotate)
        {
            case 1:
                new_point.x = old_point.y;
                new_point.y = Board.BOARD_SIZE - old_point.x + 1;
                break;
            case 2:
                new_point.x = Board.BOARD_SIZE - old_point.x + 1;
                new_point.y = Board.BOARD_SIZE - old_point.y + 1;
                break;
            case 3:
                new_point.x = Board.BOARD_SIZE - old_point.y + 1;
                new_point.y = old_point.x;
                break;
            default:
                new_point.x = old_point.x;
                new_point.y = old_point.y;
                break;
        }
        return new_point;
    }

    /// <summary>
    /// 
    /// 座標の左右反転を行う関数
    /// 
    /// </summary>
    /// <param name="point">反転前座標</param>
    /// <returns>反転後座標</returns>
    private Point mirrorPoint(in Point point)
    {
        Point new_point = new Point();
        new_point.x = Board.BOARD_SIZE - point.x + 1;
        new_point.y = point.y;

        return new_point;
    }
}
