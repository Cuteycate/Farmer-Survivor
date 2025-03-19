using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBoss : MonoBehaviour
{
    public float damage; // Dame vũ khí
    public int per; // Xuyên qua bao nhiêu 
    public int projectileNumber; // Số lượng projectile
    public Transform PlayerTranform;
    public bool FinalBoss = false;
    public Vector3 targetPosition;


    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    public void Init(int per, Vector3 dir, Transform playerpo, bool check)
    {

        this.per = per;

        if (check)
        {
            if (per > -1)
            {
                rigid.velocity = dir * 20f;
            }
        }
        else
        {
            if (per > -1)
            {
                rigid.velocity = dir * 5f;
            }
        }


        PlayerTranform = playerpo;
        FinalBoss = check;
        targetPosition = playerpo.position;
    }

    private void Update()
    {
        if (FinalBoss)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 1.5f) // Kiểm tra xem viên đạn có đến được vị trí người chơi không
            {
                GameObject FireBallEx = GameManager.instance.pool.Get(14);
                FireBallEx.transform.position = transform.position;
                gameObject.SetActive(false);
                AudioManager.instance.PlaySfx(AudioManager.Sfx.FireballExplode);
            }
        }
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (FinalBoss && collision.CompareTag("Bullet"))
            return;


        if (collision.CompareTag("Player"))
        {
            per--;
        }
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.Health -= 40;
        }
        if (per == -1)
        {
            rigid.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
       
    }
}
