using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConsumablesController : MonoBehaviour
{
    public static ConsumablesController Instance;

    [SerializeField] private float minDistanceToSnake;
    [SerializeField] private ConsumableInfo[] consumableInfos;
    
    [System.Serializable]
    public struct ConsumableInfo
    {
        public Consumable consumable;
        public int startSpawnDelay;
        public Vector2Int randomSpawnTime;
    }

    public readonly List<Consumable> Consumables = new();
    private readonly Dictionary<Consumable, int> nextSpawnTimes = new();
    
    private void Awake()
    {
        Instance = this;
    }

    public void Tick(int turn)
    {
        var suitablePositions = GetSuitablePositions();
        
        foreach (var consumableInfo in consumableInfos)
        {
            if (suitablePositions.Count == 0) return;
            if (turn < consumableInfo.startSpawnDelay) return;
            
            var nextSpawnTime = nextSpawnTimes.GetValueOrDefault(consumableInfo.consumable,
                Random.Range(consumableInfo.randomSpawnTime.x, consumableInfo.randomSpawnTime.y + 1));
            nextSpawnTimes.TryAdd(consumableInfo.consumable, nextSpawnTime);
            
            if (turn >= nextSpawnTime)
            {
                var randPos = suitablePositions[Random.Range(0, suitablePositions.Count)];
                var pos = new Vector3(randPos.x, randPos.y, transform.position.z);
                var obj = Instantiate(consumableInfo.consumable, pos, Quaternion.identity, transform);
                
                obj.name = consumableInfo.consumable.name + turn;
                print($"Instantiate {consumableInfo.consumable.name}");
                Consumables.Add(obj);
                suitablePositions.Remove(randPos);
                nextSpawnTimes[consumableInfo.consumable] = turn + Random.Range(consumableInfo.randomSpawnTime.x,
                    consumableInfo.randomSpawnTime.y + 1);
                break;
            }
        }
    }
    // todo optimize, random first, then check
    private List<Vector2Int> GetSuitablePositions()
    {
        var suitablePositions = new List<Vector2Int>();
        var playerSegments = Player.Instance.GetSegments();
        
        for (int x = 0; x < GridManager.Instance.GridSize.x; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (Consumables.Any(c => c.Pos == pos)) continue;
                if (playerSegments.Any(s => s == pos)) continue;
                if (playerSegments.Count > 0 && Vector2Int.Distance(pos, playerSegments[0]) < minDistanceToSnake) continue;
                if (EnemyController.Instance.Enemies.Any(e => e.GetSegments().Any(s => s == pos))) continue;
                if (EnemyController.Instance.Enemies.Any(e => Vector2Int.Distance(pos, e.GetSegments()[0]) < minDistanceToSnake)) continue;
                suitablePositions.Add(pos);
            }
        }

        return suitablePositions;
    }

    public Consumable GetConsumable(Vector2Int pos)
    {
        foreach (var consumable in Consumables)
        {
            if (consumable.transform.position == new Vector3(pos.x, pos.y, consumable.transform.position.z))
                return consumable;
        }

        return null;
    }

    public void RemoveConsumable(Consumable consumable) => Consumables.Remove(consumable);
}
