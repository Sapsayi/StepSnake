using System;
using DG.Tweening;
using UnityEngine;

public abstract class Consumable : MonoBehaviour
{
    public abstract void Activate(Snake snake);

    public Vector2Int Pos => new((int)transform.position.x, (int)transform.position.y);

    private void Start()
    {
        transform.DOScale(1f, 0.2f).From(0f).SetEase(Ease.OutBack);
    }
}
