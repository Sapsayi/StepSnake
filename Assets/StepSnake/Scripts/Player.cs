using System;
using UnityEngine;

public class Player : Snake
{
    public static Player Instance;

    private void Awake()
    {
        Instance = this;
    }
}
