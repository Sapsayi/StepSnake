using System;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainProcess : MonoBehaviour
{
    public static MainProcess Instance;
    
    [SerializeField] private SnakeSegmentsConfig snakeSegmentsConfig;
    [SerializeField] private List<Vector2Int> startPlayerSegments;
    [SerializeField] private List<Vector2Int> startEnemiesPositions;

    private int turn;
    private IEnumerator mainProcessCoroutine;
    
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

        StartCoroutine(mainProcessCoroutine = ProcessRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator ProcessRoutine()
    {
        while (true)
        {
            Vector2Int direction;
            do
            {
                yield return null;
                direction = GetMoveDirection();
            } while (direction == Vector2Int.zero || !Player.Instance.CanMove(direction));

            turn++;

            if (Player.Instance.CheckSelfKill(direction) || Player.Instance.CheckEnemies(direction))
            {
                yield return Player.Instance.DeathRoutine(direction);
                OnPlayerDestroy();
            }

            yield return Player.Instance.PlayerTurn(direction);
            yield return null;
            
            yield return EnemyController.Instance.EnemyTurn();
            yield return null;
            
            ConsumablesController.Instance.Tick(turn);
            
            //EnemyController.Instance.CheckCap(turn);
        }
    }

    public void OnPlayerDestroy()
    {
        if (mainProcessCoroutine != null)
        {
            StopCoroutine(mainProcessCoroutine);
            print("on player destroy");
            UI.Instance.OpenDeathScene();
        }
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
