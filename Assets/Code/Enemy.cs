using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Spawner;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public float expOnDefeat;

    public bool CanFire = false;
    public bool isFiring = false;
    int distance = 10;
    float approachSpeed = 0;


    public RuntimeAnimatorController[] animController; //Dùng đề đưa animation enemies vào vd animation 1 zombie animation 2 skeleton
    public Rigidbody2D target;
    public GameObject floatingtextPrefab;

    public bool isLive;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Collider2D coll;
    Animator anim;
    WaitForFixedUpdate wait;
   


    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive)
        {
            return;
        }
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec;

        float distanceToPlayer = dirVec.magnitude; 
        
        if (CanFire && distanceToPlayer <= distance && distanceToPlayer > 5) 
        {
           
            float radius = distanceToPlayer; 
            speed = 10;
            float angularSpeed = speed / radius; // Tốc độ góc
            Vector2 perpendicular = new Vector2(- dirVec.y, dirVec.x).normalized; 
            nextVec = perpendicular * angularSpeed * Time.fixedDeltaTime;
        }     
        else
        {
            speed = approachSpeed;
         
            nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        }

        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;

        if (CanFire && !isFiring)
        {
            StartCoroutine(FireCoroutine());
        }
    }



    // Update is called once per frame
    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive)
        {
            return;
        }
        spriter.flipX = target.position.x < rigid.position.x;

    }
    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
        
    }
    public void Init(SpawnData Data, bool CanfireRight)
    {
        anim.runtimeAnimatorController = animController[Data.spriteType];
        speed = Data.speed;
        approachSpeed = Data.speed;
        maxHealth = Data.health;
        health = Data.health;
        expOnDefeat = Data.expOnDefeat;
        CanFire = CanfireRight;

        if (Data.spriteType == 0 || Data.spriteType == 1 || Data.spriteType == 4 || Data.spriteType == 7)
        {
            if (UnityEngine.Random.value < 0.15f) 
            {
                CanFire = true;
            }
            else
            {
                CanFire = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive)
            return;

        if (collision.CompareTag("Bullet"))
        {
            // Nếu là Bullet, giảm máu của Enemy
            health -= collision.GetComponent<Bullet>().damage;
            ShowDamage(collision.GetComponent<Bullet>().damage.ToString());

            if (health > 0)
            {
                anim.SetTrigger("Hit");
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            }

        }
        else if (collision.CompareTag("BulletSpecial"))
        {        
            health -= collision.GetComponent<BulletSpecial>().damage;
            //ShowDamage(collision.GetComponent<BulletSpecial>().damage.ToString());

            if (health > 0)
            {
                anim.SetTrigger("Hit");
                //AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            }
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            return;
        }

        // Kiểm tra xem Enemy có chết không
        if (health <= 0)      
        {
            // Chết
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
        rigid.AddForce(dirVec.normalized * 3,ForceMode2D.Impulse); //Truyền knockback ngược về so với PlayerPos
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

    IEnumerator FireCoroutine()
    {
            isFiring = true;
            Vector3 targetPos = target.position;
            Vector3 dir = targetPos - transform.position;
            dir = dir.normalized;
            Transform targett = target.transform;
            Transform bullet = GameManager.instance.pool.Get(5).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            bullet.GetComponent<BulletEnemy>().Init( 0, dir, targett, false);
            //AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
            yield return new WaitForSeconds(5f); // Delay viên đạn để ko stack lên nhau
            isFiring = false;
      
    }

}
