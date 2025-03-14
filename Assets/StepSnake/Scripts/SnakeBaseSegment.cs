using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class SnakeBaseSegment : MonoBehaviour
{
    [SerializeField] private Transform mainBody;
    [SerializeField] private SpriteRenderer[] spritesForColoring;
    [SerializeField] private Transform[] bodyCenters;
    [SerializeField] private SnakeSegmentsConfig config;

    public void SetColor(Color color)
    {
        foreach (var sprite in spritesForColoring)
        {
            sprite.color = color;
        }
    }

    public void StartHeadMoveAnimation()
    {
        mainBody.DOScaleY(1f, config.moveAnimDuration).From(0f).SetEase(Ease.OutBack);
        mainBody.DOLocalMoveY(0.5f, config.moveAnimDuration).From(0f).SetEase(Ease.OutBack);
    }

    public void StartTailMoveAnimation()
    {
        mainBody.DOScaleY(0f, config.moveAnimDuration).From(1f).SetEase(Ease.OutQuart);
        mainBody.DOLocalMoveY(1f, config.moveAnimDuration).From(0.5f).SetEase(Ease.OutQuart)
            .OnComplete(() => Destroy(gameObject));
    }

    public void SetBodiesRotation(Vector3 rotation1, Vector3 rotation2)
    {
        bodyCenters[0].eulerAngles = rotation1;
        bodyCenters[1].eulerAngles = rotation2;
    }
}
