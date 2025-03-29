using UnityEngine;

[CreateAssetMenu(fileName = "SnakeSegmentsConfig", menuName = "Scriptable Objects/SnakeSegmentsConfig")]
public class SnakeSegmentsConfig : ScriptableObject
{
    public Color playerColor;
    public Color enemyColor;
    
    public float moveAnimDuration;

    public float baseDeathDuration;
    public float deathDurationMultiplier;
    public float minDeathDuration;

    public float GetOneSegmentDeathDuration(int pos)
    {
        float duration = baseDeathDuration;
        for (int i = 0; i < pos; i++)
        {
            duration *= deathDurationMultiplier;
        }

        return Mathf.Max(minDeathDuration, duration);
    }
}
