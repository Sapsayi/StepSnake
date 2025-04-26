using UnityEngine;

public class Apple : Consumable
{
    public override void Activate(Snake snake)
    {
        Destroy(gameObject);
        snake.AddLastSegment();
        if (snake is Player)
            MainProcess.Instance.OnPlayerConsumeApple();
        ConsumablesController.Instance.RemoveConsumable(this);
    }
}
