using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using static Spawner;
using static UnityEngine.UI.CanvasScaler;

public class EventWave : MonoBehaviour
{
    public float speed;

    Rigidbody2D rigid;
    public Transform[] spawnPoint;
    public Transform bestSpawnPoint = null;
    public Transform FirstSpawnPoint;
    public int TypeEvent;
    public bool isRotaion;

    public float Timer;
    public float coolTime = 5;

    public Rigidbody2D target;
    Vector2 playerPos;
    Vector2 objectPos;
    Vector2 oppositePos;

   
    bool check = true;
    public Spawner spawner;

    public GameObject spawnEffect;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();      
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    public void Inti(Transform BestSpawnPoint, Transform f, int TypeEv, bool isRotationR)
    {
        bestSpawnPoint = BestSpawnPoint;
        FirstSpawnPoint = f;
        TypeEvent = TypeEv;
        isRotaion = isRotationR;
    }



    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        playerPos = target.position;
        objectPos = transform.position;
        Timer = 0;
        check = true;
  
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        if(Timer >= coolTime)
        {
            gameObject.SetActive(false);
        }
    }


    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
    }

    void LateUpdate()
    {
        
        if (!GameManager.instance.isLive)
            return;

        if(check)
        {
            if ( TypeEvent == 1 )
            {
                foreach (Transform SpawnPoint in spawnPoint)
                {
                    GameObject Wave = GameManager.instance.pool.Get(9);
                    Wave.transform.position = SpawnPoint.position;
                    Wave.GetComponent<EnemyEvent>().Init(bestSpawnPoint, 0); //0 là event chay 
                }
                check = false;
            }
 
            if( TypeEvent == 2 )
            {
                for ( int i = 1; i < spawnPoint.Length; i++ )
                {
                    GameObject Wave = GameManager.instance.pool.Get(9);
                    GameObject vfx = Instantiate(spawnEffect, spawnPoint[i].position, Quaternion.identity);
                    Wave.transform.position = spawnPoint[i].position;
                    Wave.GetComponent<EnemyEvent>().Init(bestSpawnPoint, 1); //1 là event vong tron
                    check = false;
                }

            }
            
            if ( TypeEvent == 3)
            {
                for (int i = 1; i < spawnPoint.Length; i++)
                {
                   GameObject Wave = GameManager.instance.pool.Get(16);
                   Wave.transform.position = spawnPoint[i].position;  
                   if ( i % 2 == 0)
                    {
                        Wave.GetComponent<EnemyEventPlus>().Init( true, isRotaion ,TypeEvent);
                    }
                    else
                    {
                        Wave.GetComponent<EnemyEventPlus>().Init (false, isRotaion, TypeEvent);
                    }
                   
                   check = false;
                }
            }

            if(TypeEvent == 4)
            {
                for (int i = 1; i < spawnPoint.Length; i++)
                {
                    GameObject Wave = GameManager.instance.pool.Get(16);
                    Wave.transform.position = spawnPoint[i].position;
                    if (i % 2 == 0)
                    {
                        Wave.GetComponent<EnemyEventPlus>().Init(true, isRotaion,TypeEvent);
                    }
                    else
                    {
                        Wave.GetComponent<EnemyEventPlus>().Init(false, isRotaion, TypeEvent);
                    }

                    check = false;
                }
            } 


        }
    }




    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area"))
        {
            gameObject.SetActive(false);
        }
    }


}
