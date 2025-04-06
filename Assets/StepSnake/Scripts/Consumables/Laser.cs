using System.Collections.Generic;
using UnityEngine;

public class Laser : Consumable
{
    [SerializeField] private SnakeSegmentsConfig config;
    [SerializeField] private ExplosionEffect explosionEffect;
    
    public override void Activate(Snake snake)
    {
        var damagedPositions = GetDamagedPositions(snake.GetSegments()[0] - Pos);
        
        if (snake is Player)
        {
            foreach (var enemy in EnemyController.Instance.Enemies)
            {
                enemy.TryDamagePositions(damagedPositions);
            }
        }
        else
        {
            Player.Instance.TryDamagePositions(damagedPositions);
        }

        foreach (var damagedPosition in damagedPositions)
        {
            Instantiate(explosionEffect, new Vector3(damagedPosition.x, damagedPosition.y, transform.position.z),
                Quaternion.identity).Init(snake is Player ? config.playerColor : config.enemyColor);
        }
        
        ConsumablesController.Instance.RemoveConsumable(this);
        Destroy(gameObject);
    }
    
    private List<Vector2Int> GetDamagedPositions(Vector2Int exceptDirection)
    {
        List<Vector2Int> positions = new();
        List<Vector2Int> directions = new()
            { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

        directions.Remove(exceptDirection);

        foreach (var direction in directions)
        {
            int index = 1;
            while (GridManager.Instance.CheckBorders(Pos + direction * index))
            {
                positions.Add(Pos + direction * index);
                index++;
            }
        }
        
        return positions;
    }
}
