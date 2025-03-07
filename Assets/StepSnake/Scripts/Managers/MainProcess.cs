using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainProcess : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> startPlayerSegments;

    private int turn;
    
    private void Start()
    {
        Player.Instance.Init(startPlayerSegments);
        StartCoroutine(ProcessRoutine());
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
                UI.Instance.OpenDeathScene();
                yield break;
            }
            
            Player.Instance.CheckConsumable(direction);
            Player.Instance.Move(direction);
            
            yield return new WaitForSeconds(0.5f);
            
            yield return EnemyController.Instance.EnemyTurn();
            
            ConsumablesController.Instance.Tick(turn);
            
            EnemyController.Instance.CheckCap(turn);
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
