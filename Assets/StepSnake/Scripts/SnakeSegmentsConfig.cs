using UnityEngine;

[CreateAssetMenu(fileName = "SnakeSegmentsConfig", menuName = "Scriptable Objects/SnakeSegmentsConfig")]
public class SnakeSegmentsConfig : ScriptableObject
{
    public Sprite headWithBody;
    public Sprite singleHead;
    public Sprite straightBody;
    public Sprite tail;
    public Sprite turnedLeftBody;
    public Sprite turnedRightBody;
}
