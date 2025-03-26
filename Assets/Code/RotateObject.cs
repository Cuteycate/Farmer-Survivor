using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed = 100f; // Tốc độ xoay, bạn có thể điều chỉnh theo ý muốn
    public float shrinkSpeed = 1f;

    EdgeCollider2D circleCollider;


    private void Awake()
    {
        circleCollider = GetComponent<EdgeCollider2D>();
    }


    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);


        if (transform.localScale.x > 0 && transform.localScale.y > 0)
        {
            float newScale = transform.localScale.x - shrinkSpeed * Time.deltaTime;
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
    }

}
