using System;
using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI Instance;

    [SerializeField] private GameObject deathScene;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenDeathScene()
    {
        deathScene.SetActive(true);
    }
}
