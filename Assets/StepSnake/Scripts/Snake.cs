using System.Collections.Generic;
using UnityEngine;

public abstract class Snake : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spritePrefab;
    [SerializeField] private Color color;
    [SerializeField] private SnakeSegmentsConfig snakeSegmentsConfig;
    
    private readonly List<Vector2Int> segments = new();
    private readonly List<SpriteRenderer> sprites = new();

    public void Init(List<Vector2Int> segments)
    {
        this.segments.AddRange(segments);
        UpdateSprites(new Vector2Int(0, 1));
    }

    public bool CanMove(Vector2Int direction)
    {
        var nextPos = segments[0] + direction;
        if (segments.Count > 1 && nextPos == segments[1])
            return false;
        return GridManager.Instance.CheckBorders(nextPos);
    }

    public bool CheckSelfKill(Vector2Int direction)
    {
        foreach (var segment in segments)
        {
            if (segment == segments[0] + direction)
                return true;
        }

        return false;
    }
    
    public void CheckConsumable(Vector2Int direction)
    {
        var consumable = ConsumablesController.Instance.GetConsumable(segments[0] + direction);
        if (consumable)
        {
            consumable.Activate(this);
        }
    }
    
    public void Move(Vector2Int direction)
    {
        for (int i = segments.Count - 1; i >= 1; i--)
        {
            segments[i] = segments[i - 1];
        }
        segments[0] += direction;
        UpdateSprites(direction);
    }


    public void AddLastSegment()
    {
        segments.Add(segments[^1]);
    }
    
    
    private void UpdateSprites(Vector2Int direction)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (sprites.Count == i)
                sprites.Add(Instantiate(spritePrefab, transform));
            sprites[i].transform.position = new Vector3(segments[i].x, segments[i].y, transform.position.z);
        }
        
        ApplyColor();
        DrawHead(direction);
        for (int i = 0; i < sprites.Count; i++)
        {
            if (i != 0 && i != sprites.Count - 1)
                DrawBody(i);
        }
        DrawTail();
    }

    private void ApplyColor()
    {
        foreach (var sprite in sprites)
        {
            sprite.color = color;
        }
    }
    
    private void DrawHead(Vector2Int direction)
    {
        sprites[0].sprite = sprites.Count == 1 ? snakeSegmentsConfig.singleHead : snakeSegmentsConfig.headWithBody;
        sprites[0].transform.eulerAngles = GetRotation(direction);
    }

    private void DrawBody(int index)
    {
        var direction1 = segments[index - 1] - segments[index];
        var direction2 = segments[index + 1] - segments[index];
        if (direction1.x == direction2.x)
        {
            sprites[index].sprite = snakeSegmentsConfig.straightBody;
            sprites[index].transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (direction1.y == direction2.y)
        {
            sprites[index].sprite = snakeSegmentsConfig.straightBody;
            sprites[index].transform.eulerAngles = new Vector3(0, 0, 90);
        }
        else
        {
            sprites[index].sprite = direction2 == new Vector2Int(direction1.y, -direction1.x)
                ? snakeSegmentsConfig.turnedRightBody
                : snakeSegmentsConfig.turnedLeftBody;
            sprites[index].transform.eulerAngles = GetRotation(direction1);
        }
    }

    private void DrawTail()
    {
        if (sprites.Count == 1)
            return;
        int lastIndex = sprites.Count - 1;
        sprites[lastIndex].sprite = snakeSegmentsConfig.tail;
        var direction = segments[lastIndex - 1] - segments[lastIndex];
        sprites[lastIndex].transform.eulerAngles = GetRotation(direction);
    }
    
    private Vector3 GetRotation(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1))
            return new Vector3(0, 0, 0);
        if (direction == new Vector2Int(1, 0))
            return new Vector3(0, 0, 270);
        if (direction == new Vector2Int(0, -1))
            return new Vector3(0, 0, 180);
        if (direction == new Vector2Int(-1, 0))
            return new Vector3(0, 0, 90);
        return new Vector3(0, 0, 0);
    }

    public List<Vector2Int> GetSegments()
    {
        var list = new List<Vector2Int>();
        list.AddRange(segments);
        return list;
    }
}
