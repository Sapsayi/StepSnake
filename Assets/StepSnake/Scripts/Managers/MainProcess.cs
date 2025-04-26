using System;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-9999)]
public class MainProcess : MonoBehaviour
{
    public static MainProcess Instance;

    [SerializeField] private LevelInfo[] levelInfos;
    [SerializeField] private SnakeSegmentsConfig snakeSegmentsConfig;
    [Space] 
    [SerializeField] private int debugLevel;

    public Action<int> OnPlayerConsumeAppleAction;
    public Action<int> OnKillAction;
    public Action<int> OnNewTurnAction;

    private LevelInfo curLevel;
    private int turn;
    private int killCount;
    private int appleCount;
    private CancellationTokenSource mainProcessCancellationToken = new();

    [Button]
    private void SetLevel()
    {
        PlayerPrefs.SetInt("level", debugLevel);
        print($"set level {debugLevel}");
    }
    
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private async void Start()
    {
        if (!PlayerPrefs.HasKey("level"))
            PlayerPrefs.SetInt("level", 0);
        
        curLevel = levelInfos[PlayerPrefs.GetInt("level")];

        UI.Instance.SetLevelText(PlayerPrefs.GetInt("level"));
        UI.Instance.SetLevelPanelAlpha(1f);
        
        await UniTask.WaitForSeconds(1f);
        
        UI.Instance.StartLevelPanelFade(0f, 0.5f);
        
        StartLevel(curLevel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void StartLevel(LevelInfo levelInfo)
    {
        EnemyController.Instance.Init(levelInfo.enemyCaps);
        ConsumablesController.Instance.Init(levelInfo.consumableInfos);
        UI.Instance.Init(levelInfo.goals);
        
        Player.Instance.Init(levelInfo.startPlayerSegments);
        for (var i = 0; i < levelInfo.startEnemiesPositions.Count; i++)
        {
            EnemyController.Instance.SpawnEnemy(levelInfo.startEnemiesPositions[i], "enemy" + i);
        }
        
        ProcessTask(mainProcessCancellationToken.Token).Forget();
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
            OnNewTurnAction?.Invoke(turn);
            CheckGoals().Forget();

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
        OnKillAction?.Invoke(killCount);
        CheckGoals().Forget();
    }

    public void OnPlayerConsumeApple()
    {
        appleCount++;
        OnPlayerConsumeAppleAction?.Invoke(appleCount);
        CheckGoals().Forget();
    }

    private async UniTask CheckGoals()
    {
        foreach (var goal in curLevel.goals)
        {
            switch (goal.type)
            {
                case LevelGoalType.AppleCount:
                    if (appleCount < goal.value)
                        return;
                    break;
                case LevelGoalType.SnakeLength:
                    if (Player.Instance.GetSegments().Count < goal.value)
                        return;
                    break;
                case LevelGoalType.KillCount:
                    if (killCount < goal.value)
                        return;
                    break;
                case LevelGoalType.TurnCount:
                    if (turn < goal.value)
                        return;
                    break;
                case LevelGoalType.FillAllCells:
                    if (Player.Instance.GetSegments().Count < GridManager.Instance.GetAllCellsCount())
                        return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        mainProcessCancellationToken?.Cancel();
        PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1);
        
        await UniTask.WaitForSeconds(2f);
        
        UI.Instance.SetLevelText(PlayerPrefs.GetInt("level"));
        UI.Instance.StartLevelPanelFade(1f, 0.5f);

        await UniTask.WaitForSeconds(1.5f);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
