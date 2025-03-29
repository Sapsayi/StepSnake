using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : Consumable
{
    [SerializeField] private SnakeSegmentsConfig config;
    [SerializeField] private ExplosionEffect explosionEffect;
    [SerializeField] private float explosionRadius;
    
    public override void Activate(Snake snake)
    {
        transform.localScale = Vector3.zero;

        var damagedPositions = GetDamagedPositions();
        
        if (snake is Player)
        {
            foreach (var damagedPosition in damagedPositions.ToList())
            {
                if (snake.GetSegments().Count == 1)
                    break;
                foreach (var segment in snake.GetSegments())
                {
                    if (segment == damagedPosition)
                        damagedPositions.Remove(damagedPosition);
                }
            }
            
            foreach (var enemy in EnemyController.Instance.Enemies)
            {
                enemy.TryDamagePositions(damagedPositions);
            }
        }
        else
        {
            foreach (var damagedPosition in damagedPositions.ToList())
            {
                foreach (var enemy in EnemyController.Instance.Enemies)
                {
                    if (enemy.GetSegments().Count == 1)
                        break;
                    foreach (var segment in enemy.GetSegments())
                    {
                        if (segment == damagedPosition)
                            damagedPositions.Remove(damagedPosition);
                    }
                }
            }
            
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

    private List<Vector2Int> GetDamagedPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int intRadius = Mathf.CeilToInt(explosionRadius);
        
        for (int x = -intRadius; x <= intRadius; x++)
        {
            for (int y = -intRadius; y <= intRadius; y++)
            {
                var newPos = new Vector2Int(Pos.x + x, Pos.y + y);
                if (Vector2.Distance(Pos, newPos) <= explosionRadius && GridManager.Instance.CheckBorders(newPos) && newPos != Pos)
                    positions.Add(newPos);
            }
        }

        return positions;
    }
}
