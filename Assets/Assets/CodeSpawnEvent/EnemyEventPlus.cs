using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnemyEventPlus : MonoBehaviour
{
    public float[] speed;
    public float health;
    public float maxHealth;
    public float expOnDefeat;

    public ParticleSystem DustLeft;
    public ParticleSystem DustRight;
    public ParticleSystem DustUp;
    public ParticleSystem DustDown;

    public bool IsAlive { get { return isLive; } }
    public RuntimeAnimatorController[] animController;

    public GameObject floatingtextPrefab;
    public bool isLive;

    SpriteRenderer spriter;
    Animator anim;
    Collider2D coll;
    WaitForFixedUpdate wait;
    Rigidbody2D rigid;

    public Rigidbody2D target;
    public Transform bestSpawnPoint;

    public float timer = 0;
    float cooldown = 10f;
    public int TypeEnemy;

    private bool isTopDown;
    private bool isLeft;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
        rigid = GetComponent<Rigidbody2D>();
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }

    public void Init( bool c, bool isRotation, int TypeEn )
    {
        
        isLeft = c;
        maxHealth = 1000;
        health = maxHealth;
        rigid.mass = health;
        isTopDown = isRotation;
        TypeEnemy = TypeEn;
       
    }

    private void FixedUpdate()
    {

        if (!GameManager.instance.isLive)
            return;

        if (!isLive)
        {
            coll.isTrigger = true;
        }

        switch (TypeEnemy) 
        {
            case 3:
                if (isTopDown)
                {
                    if (isLeft)
                    {
                        rigid.velocity = Vector2.up * 1f;
                        CreateDustDown();
                    }
                    else
                    {
                        rigid.velocity = Vector2.down * 1f;
                        CreateDustUp();
                    }
                }
                else
                {
                    if (isLeft)
                    {
                        rigid.velocity = Vector2.right * 1f;
                        CreateDustLeft();
                    }
                    else
                    {
                        rigid.velocity = Vector2.left * 1f;
                        CreateDustRight();
                    }
                }

                spriter.flipX = target.position.x < rigid.position.x;

                break;
            case 4:
                
                if (isLeft)
                {
                    rigid.velocity = Vector2.right * 5f;
                    CreateDustLeft();
                    
                }
                else
                {
                    spriter.flipX = true;
                    rigid.velocity = Vector2.left * 5f;
                    CreateDustRight();
                    
                }
                break;

        }  
    }



    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= cooldown)
        {
            anim.SetBool("Dead", true);
            //gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
        timer = 0;    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;
        health -= collision.GetComponent<Bullet>().damage;
        //StartCoroutine(KnockBack());
        ShowDamage(collision.GetComponent<Bullet>().damage.ToString());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            //.. sống,bị trúng
        }
        else
        {
            //Chết
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            //GameManager.instance.GetExp(this);
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
    void ShowDamage(string text)
    {
        GameObject prefab = Instantiate(floatingtextPrefab, transform.position, Quaternion.identity);
        prefab.GetComponentInChildren<TextMesh>().text = text;
    }

    public void TakeDamage(float damage)
    {
        // Subtract damage from health
        health -= damage;
        ShowDamage(damage.ToString());
        // Check if the enemy is dead
        if (health <= 0)
        {
            isLive = false;
            coll.enabled = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            //GameManager.instance.GetExp(this);
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
         
            rigid.mass = 500;
            return;
      
    }

    private void CreateDustLeft()
    {
        DustLeft.Play();
    }
    
    private void CreateDustRight()
    {
        DustRight.Play();
    }

    private void CreateDustUp()
    {
        DustUp.Play();
    }
    private void CreateDustDown()
    {
        DustDown.Play();
    }
}
