using UnityEngine;

[System.Serializable]
public class Vector2IntSer
{
    public int x;
    public int y;

    public Vector2IntSer(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public static explicit operator Vector2Int(Vector2IntSer ser)
    {
        return new Vector2Int(ser.x, ser.y);
    }
}
