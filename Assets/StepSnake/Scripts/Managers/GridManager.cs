using System;
using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    
    [SerializeField] private SpriteRenderer lightSquare;
    [SerializeField] private SpriteRenderer darkSquare;
    [SerializeField] private Vector2Int gridSize;

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
                var pos = new Vector3(x, y, transform.position.z);
                Instantiate(isLight ? lightSquare : darkSquare, pos, Quaternion.identity, transform);
            }
        }
    }

    public bool CheckBorders(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < gridSize.x) && (pos.y >= 0 && pos.y < gridSize.y);
    }

    public int GetAllCellsCount() => gridSize.x * gridSize.y;
}
