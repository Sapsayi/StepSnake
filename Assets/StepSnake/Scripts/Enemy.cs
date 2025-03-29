using UnityEngine;

public class Enemy : Snake
{
    protected override Color GetColor() => config.enemyColor;
    
    public bool CheckPlayerSegments(Vector2Int direction)
    {
        foreach (var playerSegment in Player.Instance.GetSegments())
        {
            if (playerSegment == segments[0] + direction)
                return true;
        }

        return false;
    }
    
    protected override void OnDamageDestroy()
    {
        base.OnDamageDestroy();
        EnemyController.Instance.Enemies.Remove(this);
    }
}
