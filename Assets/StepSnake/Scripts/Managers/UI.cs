using System;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI Instance;

    [SerializeField] private GameObject deathScene;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text killText;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenDeathScene()
    {
        deathScene.SetActive(true);
    }

    public void SetTurnText(int turn)
    {
        turnText.text = turn.ToString();
    }

    public void SetKillText(int killCount, int killGoal)
    {
        killText.text = $"{killCount}/{killGoal}";
    }
}
