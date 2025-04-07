using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedlineCheck : MonoBehaviour
{
    private CircleCollider2D Collider;

    private void Start()
    {
        Collider = GetComponent<CircleCollider2D>();
    }

    private void Awake()
    {
        Collider = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.Health -= 20;
        }
    }




}
