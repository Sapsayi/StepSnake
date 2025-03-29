using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance;
    
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float minSpawnDistanceToSnake;
    [SerializeField] private SnakeSegmentsConfig config;
    [SerializeField] private int[] caps;

    public readonly List<Enemy> Enemies = new();

    private Dictionary<Enemy, Consumable> assignedConsumables = new();
    
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

    public void SpawnEnemy(Vector2Int pos, string enemyName)
    {
        var enemy = Instantiate(enemyPrefab, transform);
        enemy.name = enemyName;
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

    public async UniTask EnemyTurn()
    {
        List<UniTask> uniTasks = new();
        
        assignedConsumables.Clear();
        
        foreach (var enemy in Enemies.ToList())
        {
            var direction = Vector2Int.zero;

            Consumable assignedConsumable = GetAssignedConsumable(GetConsumableDistancesForOne(enemy), enemy);
            
            assignedConsumables.Add(enemy, assignedConsumable);
            
            if (assignedConsumable)
                direction = Pathfinding.GetDirection(enemy.GetSegments()[0], assignedConsumable.Pos);
            
            if (direction == Vector2Int.zero)
                direction = GetRandDirection(enemy);

            if (enemy.CheckSelfKill(direction) || enemy.CheckEnemies(direction) || enemy.CheckPlayerSegments(direction))
            {
                uniTasks.Add(enemy.DeathRoutine(direction, true));
                Enemies.Remove(enemy);
            }
            else
            {
                enemy.CheckConsumable(direction);
                uniTasks.Add(enemy.Move(direction));
            }
        }

        if (Player.Instance.IsDamaged)
            uniTasks.Add(Player.Instance.DamagedRoutine());
        
        await UniTask.WhenAll(uniTasks);
    }
    
    private Dictionary<Consumable, int> GetConsumableDistancesForOne(Enemy enemy)
    {
        Dictionary<Consumable, int> consumableDistances = new();
        foreach (var consumable in ConsumablesController.Instance.Consumables)
        {
            int distance = Pathfinding.GetDistance(consumable.Pos, enemy.GetSegments()[0]);
            if (distance < int.MaxValue)
                consumableDistances.Add(consumable, distance);
        }

        consumableDistances = consumableDistances.OrderBy(c => c.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        return consumableDistances;
    }

    private Consumable GetAssignedConsumable(Dictionary<Consumable, int> consumableDistances, Enemy enemy)
    {
        if (consumableDistances.Count == 0)
            return null;
        
        foreach (var consumable in consumableDistances)
        {
            var nearestEnemy = GetNearestEnemy(consumable.Key, enemy, consumable.Value);
            if (enemy == nearestEnemy)
                return consumable.Key;
        }

        return consumableDistances.First().Key;
    }

    private Enemy GetNearestEnemy(Consumable consumable, Enemy exceptEnemy, int exceptEnemyDistance)
    {
        Dictionary<Enemy, int> distances = new() { { exceptEnemy, exceptEnemyDistance } };

        foreach (var enemy in Enemies)
        {
            if (enemy != exceptEnemy && enemy && enemy.GetSegments().Count > 0)
            {
                int distance = Pathfinding.GetDistance(enemy.GetSegments()[0], consumable.Pos);
                distances.Add(enemy, distance);
            }
        }

        return distances.OrderBy(c => c.Value).First().Key;
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

    private void OnDrawGizmos()
    {
        foreach (var consumable in assignedConsumables)
        {
            if (consumable.Value && consumable.Key && consumable.Key.GetSegments().Count > 0)
            {
                var enemyPos = new Vector3(consumable.Key.GetSegments()[0].x, consumable.Key.GetSegments()[0].y, 0f);
                Gizmos.DrawLine(enemyPos, consumable.Value.transform.position);
            }
        }
    }
}
