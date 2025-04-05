using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    public float[] speed;
    public float health;
    public float maxHealth;
    public float expOnDefeat;
    public bool IsAlive { get { return isLive; } }
    public RuntimeAnimatorController[] animController;


    public GameObject floatingtextPrefab;
    public bool isLive;

    SpriteRenderer spriter;
    Animator anim;
    Collider2D coll;
    WaitForFixedUpdate wait;
    Rigidbody2D rigid;

    public float timer = 0;
    float cooldown = 20f;
    public int TypeEnemy;

    public Rigidbody2D target;
    public Transform bestSpawnPoint;

    void Awake()
    {    
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
        rigid = GetComponent<Rigidbody2D>();
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }

    public void Init( Transform transform, int Type)
    {
        anim.runtimeAnimatorController = animController[Type];
        bestSpawnPoint = transform;
        TypeEnemy = Type;

        if (TypeEnemy == 1)
        {
            health = 1000;
            rigid.mass = 1000;       
        }
        else
        {
            health = maxHealth;
            rigid.mass = 1f;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if ( timer >= cooldown )
        {
            gameObject.SetActive( false );
        }
    }



    private void FixedUpdate()
    {
      
        if (!GameManager.instance.isLive)
            return;

        if (!isLive)
        {
            coll.isTrigger = true;
        }

        //0 la event chay
        if (TypeEnemy == 0)
        {
            if (bestSpawnPoint != null)
            {
                if (isLive)
                {
                    Vector2 dirVec = (Vector2)bestSpawnPoint.transform.position - rigid.position;
                    Vector2 nextVec = dirVec.normalized * speed[0] * Time.fixedDeltaTime;
                    rigid.MovePosition(rigid.position + nextVec);
                    rigid.velocity = Vector2.zero;
                }

            }
        }
        //1 la event vong tron
        else
        if (TypeEnemy == 1)
        {
            if (isLive)
            {
                Vector2 dirVec = target.position - rigid.position;
                Vector2 nextVec;
                nextVec = dirVec.normalized * speed[1] * Time.fixedDeltaTime;
                rigid.MovePosition(rigid.position + nextVec);
                rigid.velocity = Vector2.zero;
            }
        }  
        

        spriter.flipX = bestSpawnPoint.transform.position.x < rigid.position.x;
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
            GameManager.instance.GetExp(this);
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator KnockBack()
    {
        /*   yield return null; //khựng 1 frame
           yield return new WaitForSeconds(2f); //coroutine 2s hết 2s mới tiếp tục bị knockback bới return null */
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos; // khoảng cách enemy - khoảng cách nhân vật
       rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse); //Truyền knockback ngược về so với PlayerPos
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
            GameManager.instance.GetExp(this);
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            if (TypeEnemy == 1)
            {
                rigid.mass = 500;
                return;
            }
            else if (TypeEnemy == 0)
            {
                rigid.mass = 1.5f;
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            rigid.mass = 1.5f;
            return;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }


}
