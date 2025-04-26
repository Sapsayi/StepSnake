using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelInfo", menuName = "Scriptable Objects/LevelInfo")]
public class LevelInfo : ScriptableObject
{
    public LevelGoal[] goals;
    [Space]
    public List<Vector2Int> startPlayerSegments;
    public List<Vector2Int> startEnemiesPositions;
    [Space]
    public int[] enemyCaps;
    [Space]
    public ConsumableInfo[] consumableInfos;
    
    
    [System.Serializable]
    public class LevelGoal
    {
        public LevelGoalType type;
        public int value;
    }
    
    [System.Serializable]
    public class ConsumableInfo
    {
        public Consumable consumable;
        public int startSpawnDelay;
        public Vector2Int randomSpawnTime;
    }
}

public enum LevelGoalType
{
    AppleCount = 0,
    SnakeLength = 1,
    KillCount = 2,
    TurnCount = 3,
    FillAllCells = 4
}
