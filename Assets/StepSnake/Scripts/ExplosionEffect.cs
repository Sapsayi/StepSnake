using System;
using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(Color color)
    {
        StartCoroutine(DestroyRoutine());
        spriteRenderer.color = color;
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
