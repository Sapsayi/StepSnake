using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainProcess : MonoBehaviour
{
    [SerializeField] private List<Vector2IntSer> startPlayerSegments;
    
    private void Start()
    {
        Player.Instance.Init(startPlayerSegments);
        StartCoroutine(ProcessRoutine());
    }

    private IEnumerator ProcessRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() =>
                GetMoveDirection() != Vector2Int.zero && Player.Instance.CanMove(GetMoveDirection()));
            Player.Instance.Move(GetMoveDirection());
            yield return null;
        }
    }

    private Vector2Int GetMoveDirection()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            return new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            return new Vector2Int(1, 0);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            return new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            return new Vector2Int(-1, 0);
        return Vector2Int.zero;
    }
}
