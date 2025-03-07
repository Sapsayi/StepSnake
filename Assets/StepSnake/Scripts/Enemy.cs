using UnityEngine;

public class Enemy : Snake
{
    public bool CheckPlayerSegments(Vector2Int direction)
    {
        foreach (var playerSegment in Player.Instance.GetSegments())
        {
            if (playerSegment == segments[0] + direction)
                return true;
        }

        return false;
    }
}
