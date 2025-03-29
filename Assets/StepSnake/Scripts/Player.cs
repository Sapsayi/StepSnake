using System;
using System.Collections;
using UnityEngine;

public class Player : Snake
{
    public static Player Instance;

    private void Awake()
    {
        Instance = this;
    }

    protected override Color GetColor() => config.playerColor;

    public IEnumerator PlayerTurn(Vector2Int direction)
    {
        float duration = config.moveAnimDuration;
        
        print("check player consumable");
        CheckConsumable(direction);
        print("move player");
        Move(direction);
        print("check enemy damages");
        foreach (var enemy in EnemyController.Instance.Enemies)
        {
            float damageDuration = enemy.GetDamagedRoutineDuration();
            StartCoroutine(enemy.DamagedRoutine());
            if (duration < damageDuration)
                duration = damageDuration;
        }
        
        print($"player turn duration {duration}");
        yield return new WaitForSeconds(duration);
    }

    protected override void OnDamageDestroy()
    {
        base.OnDamageDestroy();
        MainProcess.Instance.OnPlayerDestroy();
    }
}
