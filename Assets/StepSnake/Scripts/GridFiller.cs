using System.Collections;
using UnityEngine;

public class GridFiller : MonoBehaviour
{
    [SerializeField] private SpriteRenderer lightSquare;
    [SerializeField] private SpriteRenderer darkSquare;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 cellSize;

    public Vector2Int GridSize => gridSize;

    [ContextMenu("Create Grid")]
    private void CreateGridOnField()
    {
        Vector2 startPos = new Vector2(cellSize.x * -gridSize.x / 2f, cellSize.y * gridSize.y / 2f);
        bool isLight = true;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                isLight = !isLight;
                var pos = new Vector3(startPos.x + cellSize.x * x, startPos.y + cellSize.y * -y, transform.position.z);
                Instantiate(isLight ? lightSquare : darkSquare, pos, Quaternion.identity, transform);
            }
        }
    }
}
