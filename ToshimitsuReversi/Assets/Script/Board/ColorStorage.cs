/// <summary>
/// 
/// 色別の情報を格納するためのクラス
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class ColorStorage<T>
{
    private T[] data = new T[3];

    public T this[DiscColor color]
        {
        set { data[(int)color + 1] = value; }
        get { return data[(int)color + 1]; }
        }
}
