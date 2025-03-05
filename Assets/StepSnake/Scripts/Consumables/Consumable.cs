using UnityEngine;

public abstract class Consumable : MonoBehaviour
{
    public abstract void Activate(Snake snake);

    public Vector2Int Pos => new((int)transform.position.x, (int)transform.position.y);
}
