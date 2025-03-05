using UnityEngine;

public class Apple : Consumable
{
    public override void Activate(Snake snake)
    {
        Destroy(gameObject);
        snake.AddLastSegment();
        ConsumablesController.Instance.RemoveConsumable(this);
        print("activate");
    }
}
