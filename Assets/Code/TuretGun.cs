using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class TuretGun : MonoBehaviour
{
    public bool isFiring = false;
    public bool canfire = true;
    public bool isLive;

    public float damage;
    public float speed;
    public int per;
    public int count = 5;
    Scanner scanner;

    public float cooldownTime = 21f; // Thời gian cooldown trước khi turret bị ẩn
    public float timer; // Biến đếm ngược thời gian
    public float disappear;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriter;

    void Awake()
    {
        scanner = GetComponent<Scanner>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
       
            timer -= Time.deltaTime ;
        
         if (timer >= -1 && timer <= 0.9 || timer < disappear)
        {
            anim.SetTrigger("Dead");
            isLive = false;
            isFiring = false;
            canfire = false;
        }

        if (scanner.nearestTarget && isFiring)
        {
            spriter.flipX = scanner.nearestTarget.position.x < rigid.position.x;
        }

        if (!isLive)
        {
            StartCoroutine(Dead());
        }



    }

    IEnumerator Dead()
    {

        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }


    private void FixedUpdate()
    {

        if (canfire && !isFiring && timer > 0 && isLive)
        {
            anim.SetBool("Fire", true);
            StartCoroutine(FireCoroutine());
        }
    }

    private void OnEnable()
    {
        timer = cooldownTime * -1;
        canfire = true; 
        isLive = true;
        disappear = timer + timer;
    }

    IEnumerator FireCoroutine()
    {
        isFiring = true;

        for (int i = 0; i < count; i++)
        {
            if (!scanner.nearestTarget)
            {
                isFiring = false;
                anim.SetBool("Fire", false);
                yield break;
            }
            Vector3 targetPos = scanner.nearestTarget.position;
            Vector3 dir = targetPos - transform.position;
            dir = dir.normalized;
            Transform bullet = GameManager.instance.pool.Get(2).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            bullet.GetComponent<Bullet>().Init(damage, per, dir, i, 0.33f);
            //AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
            yield return new WaitForSeconds(0.1f); // Chia thời gian cho số lượng viên đạn để đảm bảo rằng chỉ có 5 viên đạn được bắn sau mỗi 20 giây
        }
        anim.SetBool("Fire", false);
        yield return new WaitForSeconds(4f);
        isFiring = false;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area"))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            timer = cooldownTime;
            isFiring = false;
            anim.SetTrigger("Ready");
        }
    }


}