using System;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainProcess : MonoBehaviour
{
    public static MainProcess Instance;
    
    [SerializeField] private SnakeSegmentsConfig snakeSegmentsConfig;
    [SerializeField] private List<Vector2Int> startPlayerSegments;
    [SerializeField] private List<Vector2Int> startEnemiesPositions;
    [SerializeField] private int killGoal;

    private int turn;
    private int killCount;
    private CancellationTokenSource mainProcessCancellationToken = new();
    
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Player.Instance.Init(startPlayerSegments);
        for (var i = 0; i < startEnemiesPositions.Count; i++)
        {
            EnemyController.Instance.SpawnEnemy(startEnemiesPositions[i], "enemy" + i);
        }

        ProcessTask(mainProcessCancellationToken.Token).Forget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private async UniTask ProcessTask(CancellationToken token)
    {
        while (true)
        {
            Vector2Int direction;
            do
            {
                await UniTask.NextFrame();
                direction = GetMoveDirection();
            } while (direction == Vector2Int.zero || !Player.Instance.CanMove(direction));

            turn++;
            UI.Instance.SetTurnText(turn);

            if (Player.Instance.CheckSelfKill(direction) || Player.Instance.CheckEnemies(direction))
            {
                await Player.Instance.DeathRoutine(direction);
                OnPlayerDestroy();
                token.ThrowIfCancellationRequested();
            }

            await Player.Instance.PlayerTurn(direction);
            token.ThrowIfCancellationRequested();
            
            await EnemyController.Instance.EnemyTurn();
            token.ThrowIfCancellationRequested();
            
            ConsumablesController.Instance.Tick(turn);
            
            EnemyController.Instance.CheckCap(turn);
        }
    }

    public void OnPlayerDestroy()
    {
        mainProcessCancellationToken?.Cancel();
        UI.Instance.OpenDeathScene();
    }

    public void OnEnemyDestroy()
    {
        killCount++;
        UI.Instance.SetKillText(killCount, killGoal);
    }

    private Vector2Int GetMoveDirection()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            return new Vector2Int(0, 1);
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            return new Vector2Int(1, 0);
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            return new Vector2Int(0, -1);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            return new Vector2Int(-1, 0);
        return Vector2Int.zero;
    }
}
