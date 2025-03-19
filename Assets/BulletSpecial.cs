using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class BulletSpecial : MonoBehaviour
{
    public float damage;
    public float speed;
    private float angle;
    Transform goalPointt;
    private Vector3 dir;

    Rigidbody2D rigid; 
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();       
    }

    private void Update()   
    {

        //Vector2 dirVec = goalPointt.transform.position - rigid.transform.position;
        //Vector2 nextVec = dirVec.normalized * speed * Time.deltaTime;
        //rigid.MovePosition(nextVec + rigid.position);
        //rigid.velocity = Vector2.zero;
        float distance = Vector2.Distance(rigid.position, goalPointt.position);
        if (distance < 3f)
        {
            AfterImageGenerator afterImage = GetComponent<AfterImageGenerator>();
            if (afterImage != null)
            {
                afterImage.StopAfterImages();
            }
            gameObject.SetActive(false);
        }

    }

    public void OnEnable()
    {
        
    }


    public void Init(float damage, float speed, float angle, Transform goalPoint, Vector3 dir)
    {
        this.damage = damage;
        this.speed = speed;
        this.angle = angle;   
        this.goalPointt = goalPoint;
        this.dir = dir;
        rigid.velocity = dir * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
      
    }

    void OnTriggerExit2D(Collider2D other)
    {     
        if (other.CompareTag("Area"))
        {
           
        }
    }
}
