using System;
using System.Collections;
using UnityEngine;

public class GridFiller : MonoBehaviour
{
    public static GridFiller Instance;
    
    [SerializeField] private SpriteRenderer lightSquare;
    [SerializeField] private SpriteRenderer darkSquare;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 cellSize;
    [SerializeField] private Vector3 offset;

    public Vector2Int GridSize => gridSize;

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("Create Grid")]
    private void CreateGridOnField()
    {
        bool isLight = true;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                isLight = !isLight;
                var pos = GetWorldPos(new Vector2IntSer(x, y));
                Instantiate(isLight ? lightSquare : darkSquare, pos, Quaternion.identity, transform);
            }
        }
    }

    public Vector3 GetWorldPos(Vector2IntSer pos)
    {
        var startPos = new Vector2(cellSize.x * -gridSize.x / 2f, cellSize.y * -gridSize.y / 2f);
        var worldPos = new Vector3(startPos.x + cellSize.x * pos.x, startPos.y + cellSize.y * pos.y, transform.position.z);
        return worldPos + offset;
    }

    public bool CheckBorders(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < gridSize.x) && (pos.y >= 0 && pos.y < gridSize.y);
    }
}
