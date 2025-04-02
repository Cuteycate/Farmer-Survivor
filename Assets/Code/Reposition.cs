using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class RePosition : MonoBehaviour
{
    public UnityEvent OnTilemapMove;
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        switch (transform.tag)
        {
            case "Ground":
                float diffX = playerPos.x - myPos.x;
                float diffY = playerPos.y - myPos.y;
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                diffX = Mathf.Abs(diffX);
                diffY = Mathf.Abs(diffY);
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 60);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 60);
                }
                break;
            case "Enemy":
                if(coll.enabled)
                {
                    Vector3 dist = (playerPos - myPos);
                    Vector3 ran = new Vector3(Random.Range(1,-3), Random.Range(1,-3), 0);
                    transform.Translate(ran + dist * 2);
                }
                break;
            case "Wall":
                float difffX = playerPos.x - myPos.x;
                float difffY = playerPos.y - myPos.y;
                float dirrX = difffX < 0 ? -1 : 1;
                float dirrY = difffY < 0 ? -1 : 1;
                diffX = Mathf.Abs(difffX);
                diffY = Mathf.Abs(difffY);
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirrX * 60 );
                }
                
                break;
        }
        OnTilemapMove.Invoke();
    }
}