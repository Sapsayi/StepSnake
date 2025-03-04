using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private List<Consumable> consumables = new();
    private Dictionary<Consumable, int> lastSpawnTimes = new();
    
    private void Awake()
    {
        Instance = this;
    }

    public void Tick(int turn)
    {
        foreach (var consumableInfo in consumableInfos)
        {
            
        }
    }
    
    private List<Vector2Int> GetSuitablePositions()
    {
        var suitablePositions = new List<Vector2Int>();
        var playerSegments = Player.Instance.GetSegments();
        
        for (int x = 0; x < GridManager.Instance.GridSize.x; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (Vector2Int.Distance(pos, playerSegments[0]) < minDistanceToSnake) continue;
                if (playerSegments.Any(s => s == pos)) continue;
                suitablePositions.Add(pos);
            }
        }

        return suitablePositions;
    }
}
