using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance;
    
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float minSpawnDistanceToSnake;
    [SerializeField] private int[] caps;

    public readonly List<Enemy> Enemies = new();

    private void Awake()
    {
        Instance = this;
    }

    public void CheckCap(int turn)
    {
        var cap = 0;
        for (int i = 0; i < caps.Length; i++)
        {
            if (caps[i] < turn)
                cap = i;
        }

        if (Enemies.Count < cap)
        {
            var suitablePositions = GetSuitableSpawnPositions();
            if (suitablePositions.Count > 0)
            {
                var randPos = suitablePositions[Random.Range(0, suitablePositions.Count)];
                var enemy = Instantiate(enemyPrefab, transform);
                enemy.Init(new List<Vector2Int> { randPos });
                Enemies.Add(enemy);
            }
        }
    }

    public void SpawnEnemy(Vector2Int pos)
    {
        var enemy = Instantiate(enemyPrefab, transform);
        enemy.Init(new List<Vector2Int> { pos });
        Enemies.Add(enemy);
    }
    
    private List<Vector2Int> GetSuitableSpawnPositions()
    {
        var suitablePositions = new List<Vector2Int>();
        var playerSegments = Player.Instance.GetSegments();
        
        for (int x = 0; x < GridManager.Instance.GridSize.x; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (Vector2Int.Distance(pos, playerSegments[0]) < minSpawnDistanceToSnake) continue;
                if (playerSegments.Any(s => s == pos)) continue;
                if (ConsumablesController.Instance.Consumables.Any(c => c.Pos == pos)) continue;
                if (Enemies.Any(e => e.GetSegments().Any(s => s == pos))) continue;
                suitablePositions.Add(pos);
            }
        }

        return suitablePositions;
    }

    public IEnumerator EnemyTurn()
    {
        List<Consumable> selectedConsumables = new();
        
        foreach (var enemy in Enemies.ToList())
        {
            var direction = GetRandDirection(enemy);
            
            var suitableConsumables = ConsumablesController.Instance.Consumables.Except(selectedConsumables).ToList();
            if (suitableConsumables.Count > 0)
            {
                var consumable = suitableConsumables
                    .OrderBy(c => Vector2Int.Distance(c.Pos, enemy.GetSegments()[0])).ToList()[0];
                selectedConsumables.Add(consumable);
                var directionToConsumable = Pathfinding.GetDirection(enemy.GetSegments()[0], consumable.Pos);
                if (directionToConsumable != Vector2Int.zero)
                    direction = directionToConsumable;
            }

            if (enemy.CheckSelfKill(direction) || enemy.CheckEnemies(direction) || enemy.CheckPlayerSegments(direction))
            {
                yield return enemy.DeathRoutine(direction);
                Enemies.Remove(enemy);
                Destroy(enemy.gameObject);
            }
            else
            {
                enemy.CheckConsumable(direction);
                yield return enemy.Move(direction);
            }
        }

        yield return null;
    }

    private Vector2Int GetRandDirection(Enemy enemy)
    {
        var directions = GetSuitableDirection(enemy);
        var randDirection = directions[Random.Range(0, directions.Count)];
            
        foreach (var direction in directions.ToList())
        {
            if (enemy.CheckSelfKill(direction))
                directions.Remove(direction);
        }
        if (directions.Count > 0)
            randDirection = directions[Random.Range(0, directions.Count)];
            
        foreach (var direction in directions.ToList())
        {
            if (enemy.CheckEnemies(direction))
                directions.Remove(direction);
        }
        if (directions.Count > 0)
            randDirection = directions[Random.Range(0, directions.Count)];

        foreach (var direction in directions.ToList())
        {
            if (enemy.CheckPlayerSegments(direction))
                directions.Remove(direction);
        }
        if (directions.Count > 0)
            randDirection = directions[Random.Range(0, directions.Count)];

        return randDirection;
    }

    private List<Vector2Int> GetSuitableDirection(Enemy enemy)
    {
        var directions = new List<Vector2Int> { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };
        foreach (var direction in directions.ToList())
        {
            if (!enemy.CanMove(direction))
                directions.Remove(direction);
        }

        return directions;
    }
}
