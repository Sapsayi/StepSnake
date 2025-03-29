using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Snake : MonoBehaviour
{
    [SerializeField] private SnakeBaseSegment baseSegmentPrefab;
    [SerializeField] private GameObject eyesPrefab;
    [SerializeField] private GameObject tailPrefab;
    [SerializeField] protected SnakeSegmentsConfig config;
    
    protected readonly List<Vector2Int> segments = new();
    private readonly List<SnakeBaseSegment> sprites = new();
    private Transform eyes;
    private Transform tail;
    private Vector2Int previousTailPos;
    private int damagedSegmentIndex = -1;

    public void Init(List<Vector2Int> segments)
    {
        this.segments.AddRange(segments);
        previousTailPos = this.segments[^1];
        eyes = Instantiate(eyesPrefab, transform).transform;
        tail = Instantiate(tailPrefab, transform).transform;
        eyes.transform.position = new Vector3(this.segments[0].x, this.segments[0].y, transform.position.z - 1);
        tail.transform.position = new Vector3(this.segments[0].x, this.segments[0].y, transform.position.z - 1);
        
        UpdateSprites(new Vector2Int(0, 0));
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

    public bool CheckEnemies(Vector2Int direction)
    {
        foreach (var enemy in EnemyController.Instance.Enemies)
        {
            foreach (var enemySegment in enemy.segments)
            {
                if (enemySegment == segments[0] + direction)
                    return true;
            }
        }

        return false;
    }
    
    public void CheckConsumable(Vector2Int direction)
    {
        var consumable = ConsumablesController.Instance.GetConsumable(segments[0] + direction);
        if (consumable)
            consumable.Activate(this);
    }
    
    public void Move(Vector2Int direction)
    {
        previousTailPos = segments[^1];
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
        UpdateEyes(direction);

        if (direction != Vector2Int.zero && segments[^1] != previousTailPos)
        {
            DrawTail(direction);
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (sprites.Count == i) 
                sprites.Add(Instantiate(baseSegmentPrefab, transform));
            sprites[i].transform.position = new Vector3(segments[i].x, segments[i].y, transform.position.z);
        }
        
        ApplyColor();
        SetSegmentsRotation(direction);
        if (direction != Vector2Int.zero)
            sprites[0].StartHeadMoveAnimation();
    }

    private void UpdateEyes(Vector2Int direction)
    {
        eyes.eulerAngles = GetRotation(direction);
        var newPos = new Vector3(eyes.position.x + direction.x, eyes.position.y + direction.y, transform.position.z - 1);
        eyes.DOMove(newPos, config.moveAnimDuration).SetEase(Ease.OutBack);
    }

    private void DrawTail(Vector2Int direction)
    {
        var tempTail = Instantiate(baseSegmentPrefab, transform);
        tempTail.SetColor(GetColor());
        tempTail.transform.position = sprites[^1].transform.position;
        var tailDirection = direction;
        if (segments.Count > 1)
            tailDirection = segments[^1] - previousTailPos;
        tempTail.transform.eulerAngles = GetRotation(tailDirection);
        tempTail.StartTailMoveAnimation();

        tail.eulerAngles = GetRotation(tailDirection);
        var newPos = new Vector3(segments[^1].x, segments[^1].y, transform.position.z - 1);
        tail.DOMove(newPos, config.moveAnimDuration).SetEase(Ease.OutQuart);
    }
    
    private void ApplyColor()
    {
        foreach (var sprite in sprites)
        {
            sprite.SetColor(GetColor());
        }
    }

    protected abstract Color GetColor();

    private void SetSegmentsRotation(Vector2Int direction)
    {
        sprites[0].transform.eulerAngles = GetRotation(direction);
        sprites[0].SetBodiesRotation(GetRotation(direction * -1), GetRotation(direction));
        
        for (int i = 1; i < sprites.Count; i++)
        {
            var previousSegmentDirection = segments[i - 1] - segments[i];
            if (i < sprites.Count - 1)
            {
                var nextSegmentDirection = segments[i + 1] - segments[i];
                sprites[i].SetBodiesRotation(GetRotation(previousSegmentDirection), GetRotation(nextSegmentDirection));
            }
            else if (i == sprites.Count - 1)
            {
                var previousTailDirection = previousTailPos - segments[i];
                sprites[i].SetBodiesRotation(GetRotation(previousSegmentDirection),
                    GetRotation(segments[^1] == previousTailPos ? previousSegmentDirection : previousTailDirection));
            }
        }
    }


    public void TryDamagePositions(List<Vector2Int> damagedPositions)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (damagedPositions.Any(p => p == segments[i]))
            {
                damagedSegmentIndex = i;
                break;
            }
        }
    }
    

    public IEnumerator DeathRoutine(Vector2Int direction, bool destroyOnEnd = false)
    {
        sprites[0].transform.eulerAngles = GetRotation(direction);
        
        if (segments.Count > 1)
        {
            var nextSegmentDirection = segments[1] - segments[0];
            sprites[0].SetBodiesRotation(GetRotation(direction), GetRotation(nextSegmentDirection));
        }
        else
        {
            sprites[0].SetBodiesRotation(GetRotation(direction * -1), GetRotation(direction));
            tail.eulerAngles = GetRotation(direction);
        }

        eyes.eulerAngles = GetRotation(direction);
        
        var eyesSeq = DOTween.Sequence();
        var oldEyesPos = eyes.position;
        var newEyesPos = new Vector3(eyes.position.x + direction.x * 0.2f, eyes.position.y + direction.y * 0.2f, eyes.position.z);
        eyesSeq.Append(eyes.DOMove(newEyesPos, config.moveAnimDuration / 2).SetEase(Ease.OutQuad));
        eyesSeq.Append(eyes.DOMove(oldEyesPos, config.moveAnimDuration / 2).SetEase(Ease.OutQuad));
        
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < sprites.Count; i++)
        {
            yield return DestroySegmentAnimationRoutine(i);
        }
        yield return new WaitForSeconds(0.2f);
        if (destroyOnEnd)
            Destroy(gameObject);
    }

    public IEnumerator DamagedRoutine()
    {
        if (damagedSegmentIndex < 0)
            yield break;
        
        Transform tempTail = null;
        if (damagedSegmentIndex != 0)
        {
            tempTail = Instantiate(tailPrefab, transform).transform;
            tempTail.name = "temp tail";
            tempTail.transform.position = new Vector3(segments[damagedSegmentIndex - 1].x,
                segments[damagedSegmentIndex - 1].y, transform.position.z - 1);
            tempTail.transform.eulerAngles =
                GetRotation(segments[damagedSegmentIndex - 1] - segments[damagedSegmentIndex]);
        }
        
        segments.RemoveRange(damagedSegmentIndex, segments.Count - damagedSegmentIndex);
        
        for (int i = damagedSegmentIndex; i < sprites.Count; i++)
        {
            yield return DestroySegmentAnimationRoutine(i);
        }
        sprites.RemoveRange(damagedSegmentIndex, sprites.Count - damagedSegmentIndex);
        yield return new WaitForSeconds(config.baseDeathDuration);
        
        if (damagedSegmentIndex == 0)
            OnDamageDestroy();
        else
        {
            tail = tempTail;
            print("new tail");
        }

        damagedSegmentIndex = -1;
        
        print("end damage routine");
    }

    protected virtual void OnDamageDestroy()
    {
        Destroy(gameObject);
    }

    private IEnumerator DestroySegmentAnimationRoutine(int index)
    {
        float animDuration = config.GetOneSegmentDeathDuration(index);
        
        sprites[index].transform.DOScale(0, animDuration).SetEase(Ease.OutQuart)
            .OnComplete(() => Destroy(sprites[index].gameObject));
        
        if (index == 0)
            eyes.transform.DOScale(0, animDuration).SetEase(Ease.OutQuart).OnComplete(() => Destroy(eyes.gameObject));

        if (index == sprites.Count - 1)
        {
            print("start tail anim");
            tail.transform.DOScale(0, animDuration).SetEase(Ease.OutQuart).OnComplete(() =>
            {
                Destroy(tail.gameObject);
                print("destroy tail");
            });
        }

        yield return new WaitForSeconds(animDuration / 3);
    }

    public float GetDeathRoutineDuration()
    {
        float duration = 0.7f;
        for (int i = 0; i < sprites.Count; i++)
        {
            float animDuration = config.GetOneSegmentDeathDuration(i);
            duration += animDuration / 3;
        }
        
        print($"death routine duration {duration}");
        
        return duration;
    }

    public float GetDamagedRoutineDuration()
    {
        if (damagedSegmentIndex < 0)
            return 0f;
        
        float duration = config.baseDeathDuration;
        for (int i = damagedSegmentIndex; i < segments.Count; i++)
        {
            float animDuration = config.GetOneSegmentDeathDuration(i);
            duration += animDuration / 3;
        }
        
        return duration;
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

    public List<Vector2Int> GetSegments() => segments.ToList();
}
