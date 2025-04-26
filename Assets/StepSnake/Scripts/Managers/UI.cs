using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI Instance;

    [SerializeField] private GameObject deathScene;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private CanvasGroup levelPanelCanvasGroup;
    [SerializeField] private TMP_Text levelText;
    [Header("Text Goals")] 
    [SerializeField]private Color unmetGoalTextColor;
    [SerializeField]private Color metGoalTextColor;
    [SerializeField] private TMP_Text appleGoalText;
    [SerializeField] private TMP_Text snakeLenghtGoalText;
    [SerializeField] private TMP_Text killGoalText;
    [SerializeField] private TMP_Text turnGoalText;
    [SerializeField] private TMP_Text fillAllCellsGoalText;

    private LevelInfo.LevelGoal[] goals;
    
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        MainProcess.Instance.OnNewTurnAction += SetTurnText;
    }

    private void OnDisable()
    {
        MainProcess.Instance.OnNewTurnAction -= SetTurnText;
        if (goals.Any(g => g.type == LevelGoalType.AppleCount))
            MainProcess.Instance.OnPlayerConsumeAppleAction -= SetAppleGoalText;
        if (goals.Any(g => g.type == LevelGoalType.SnakeLength))
            Player.Instance.OnLengthChange -= SetSnakeLengthGoalText;
        if (goals.Any(g => g.type == LevelGoalType.KillCount))
            MainProcess.Instance.OnKillAction -= SetKillCountGoalText;
        if (goals.Any(g => g.type == LevelGoalType.TurnCount))
            MainProcess.Instance.OnNewTurnAction -= SetTurnCountGoalText;
        if (goals.Any(g => g.type == LevelGoalType.FillAllCells))
            MainProcess.Instance.OnPlayerConsumeAppleAction -= SetFillAllCellsGoalText;
    }

    public void Init(LevelInfo.LevelGoal[] goals)
    {
        this.goals = goals;
        foreach (var goal in goals)
        {
            appleGoalText.gameObject.SetActive(goal.type == LevelGoalType.AppleCount);
            snakeLenghtGoalText.gameObject.SetActive(goal.type == LevelGoalType.SnakeLength);
            killGoalText.gameObject.SetActive(goal.type == LevelGoalType.KillCount);
            turnGoalText.gameObject.SetActive(goal.type == LevelGoalType.TurnCount);
            fillAllCellsGoalText.gameObject.SetActive(goal.type == LevelGoalType.FillAllCells);
        }

        if (goals.Any(g => g.type == LevelGoalType.AppleCount))
        {
            MainProcess.Instance.OnPlayerConsumeAppleAction += SetAppleGoalText;
            SetAppleGoalText(0);
        }

        if (goals.Any(g => g.type == LevelGoalType.SnakeLength))
        {
            Player.Instance.OnLengthChange += SetSnakeLengthGoalText;
        }

        if (goals.Any(g => g.type == LevelGoalType.KillCount))
        {
            MainProcess.Instance.OnKillAction += SetKillCountGoalText;
            SetKillCountGoalText(0);
        }

        if (goals.Any(g => g.type == LevelGoalType.TurnCount))
        {
            MainProcess.Instance.OnNewTurnAction += SetTurnCountGoalText;
            SetTurnCountGoalText(0);
        }

        if (this.goals.Any(g => g.type == LevelGoalType.FillAllCells))
        {
            MainProcess.Instance.OnPlayerConsumeAppleAction += SetFillAllCellsGoalText;
            SetFillAllCellsGoalText(0);
        }
    }
    
    public void OpenDeathScene() => deathScene.SetActive(true);

    private void SetTurnText(int turn) => turnText.text = turn.ToString();

    public void SetLevelPanelAlpha(float alpha) => levelPanelCanvasGroup.alpha = alpha;

    public void StartLevelPanelFade(float endValue, float duration) =>
        levelPanelCanvasGroup.DOFade(endValue, duration).SetEase(Ease.OutQuad);
    
    public void SetLevelText(int level) => levelText.text = $"LEVEL {level}";
    
    //todo: it could be done better
    
    private void SetAppleGoalText(int appleCount)
    {
        int goalValue = goals.First(g => g.type == LevelGoalType.AppleCount).value;
        appleGoalText.text = $"{appleCount}/{goalValue}";
        appleGoalText.color = appleCount >= goalValue ? metGoalTextColor : unmetGoalTextColor;
    }

    private void SetSnakeLengthGoalText(int length)
    {
        int goalValue = goals.First(g => g.type == LevelGoalType.SnakeLength).value;
        snakeLenghtGoalText.text = $"{length}/{goalValue}";
        snakeLenghtGoalText.color = length >= goalValue ? metGoalTextColor : unmetGoalTextColor;
    }

    private void SetKillCountGoalText(int killCount)
    {
        int goalValue = goals.First(g => g.type == LevelGoalType.KillCount).value;
        killGoalText.text = $"{killCount}/{goalValue}";
        killGoalText.color = killCount >= goalValue ? metGoalTextColor : unmetGoalTextColor;
    }

    private void SetTurnCountGoalText(int turnCount)
    {
        int goalValue = goals.First(g => g.type == LevelGoalType.TurnCount).value;
        turnGoalText.text = $"{turnCount}/{goalValue}";
        turnGoalText.color = turnCount >= goalValue ? metGoalTextColor : unmetGoalTextColor;
    }

    private void SetFillAllCellsGoalText(int value)
    {
        fillAllCellsGoalText.color = Player.Instance.GetSegments().Count >= GridManager.Instance.GetAllCellsCount()
            ? metGoalTextColor
            : unmetGoalTextColor;
    }
}
