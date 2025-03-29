using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Player : Snake
{
    public static Player Instance;

    private void Awake()
    {
        Instance = this;
    }

    protected override Color GetColor() => config.playerColor;

    public async UniTask PlayerTurn(Vector2Int direction)
    {
        List<UniTask> uniTasks = new();
        CheckConsumable(direction);
        uniTasks.Add(Move(direction));
        foreach (var enemy in EnemyController.Instance.Enemies)
        {
            if (enemy.IsDamaged)
                uniTasks.Add(enemy.DamagedRoutine()); 
        }

        await UniTask.WhenAll(uniTasks);
    }

    protected override void OnDamageDestroy()
    {
        base.OnDamageDestroy();
        MainProcess.Instance.OnPlayerDestroy();
    }
}
