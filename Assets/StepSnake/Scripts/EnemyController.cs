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

    public IEnumerator EnemyTurn()
    {
        float duration = config.moveAnimDuration;
        
        assignedConsumables.Clear();
        
        foreach (var enemy in Enemies.ToList())
        {
            var direction = Vector2Int.zero;

            Consumable assignedConsumable = GetAssignedConsumable(GetConsumableDistancesForOne(enemy), enemy);
            
            assignedConsumables.Add(enemy, assignedConsumable);
            
            if (assignedConsumable)
            {
                direction = Pathfinding.GetDirection(enemy.GetSegments()[0], assignedConsumable.Pos);
            }
            
            if (direction == Vector2Int.zero)
                direction = GetRandDirection(enemy);

            if (enemy.CheckSelfKill(direction) || enemy.CheckEnemies(direction) || enemy.CheckPlayerSegments(direction))
            {
                float deathDuration = enemy.GetDeathRoutineDuration();
                if (duration < deathDuration)
                    duration = deathDuration;
                
                StartCoroutine(enemy.DeathRoutine(direction, true));
                Enemies.Remove(enemy);
            }
            else
            {
                enemy.CheckConsumable(direction);
                StartCoroutine(enemy.Move(direction));
            }
        }

        yield return new WaitForSeconds(duration);
    }

    private Dictionary<Consumable, Dictionary<Enemy, int>> GetConsumablesInfo()
    {
        Dictionary<Consumable, Dictionary<Enemy, int>> consumableInfo = new();
        foreach (var consumable in ConsumablesController.Instance.Consumables)
        {
            Dictionary<Enemy, int> distances = new();
            foreach (var enemy in Enemies)
            {
                int distance = Pathfinding.GetDistance(consumable.Pos, enemy.GetSegments()[0]);
                if (distance < int.MaxValue)
                    distances.Add(enemy, distance);
            }

            consumableInfo.Add(consumable, distances);
        }

        return consumableInfo;
    }

    private Dictionary<Enemy, Consumable> GetAssignedConsumables(Dictionary<Consumable, Dictionary<Enemy, int>> consumablesInfo)
    {
        Dictionary<Enemy, Consumable> assignedConsumables = new();
        Dictionary<Consumable, Dictionary<Enemy, int>> selectedConsumablesInfo = new();
        foreach (var enemy in Enemies)
        {
            int minDistance = int.MaxValue;
            Consumable minDistanceConsumable = null;
            foreach (var consumable in consumablesInfo)
            {
                if (consumable.Value.ContainsKey(enemy))
                {
                    if (consumable.Value[enemy] < minDistance)
                    {
                        minDistance = consumable.Value[enemy];
                        minDistanceConsumable = consumable.Key;
                    }
                }
            }

            assignedConsumables.Add(enemy, minDistanceConsumable);   
        }

        return assignedConsumables;
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
            print($"{enemy.name} checking {consumable.Key.name} nearest enemy {nearestEnemy.name}");
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
            if (enemy != exceptEnemy)
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
            if (consumable.Value)
            {
                var enemyPos = new Vector3(consumable.Key.GetSegments()[0].x, consumable.Key.GetSegments()[0].y, 0f);
                Gizmos.DrawLine(enemyPos, consumable.Value.transform.position);
            }
        }
    }
}
