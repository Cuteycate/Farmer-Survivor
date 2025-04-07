using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHitBos : MonoBehaviour
{
    public Collider2D[] Collider2Ds;

    public void CheckSlashBoss()
    {
        Collider2D[] results = new Collider2D[10];
        int numResults = Physics2D.OverlapCollider(Collider2Ds[0], new ContactFilter2D(), results);

        for (int i = 0; i < numResults; i++)
        {
            if (results[i].CompareTag("Player"))
            {
                Debug.Log("Da Chem Trung");
                GameManager.instance.Health -= 5;
                break;
            }
        }
    }

}
