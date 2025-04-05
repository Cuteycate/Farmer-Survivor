using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootInterval = 0.5f; 
    public int shotCount = 10; 
    public float angleIncrement = 15f; 
    public float projectileSpeed = 5f; 

    public float rotaSpeed = 2f;
    public void StartFiring()
    {
        StartCoroutine(FireInThreeDirections());
    }

    void Update()
    {
        
        transform.Rotate(0, 0, rotaSpeed * Time.deltaTime * 360); 
    }
    IEnumerator FireInThreeDirections()
    {
        float[] baseAngles = { 0f, 120f, 240f }; 

        for (int i = 0; i < shotCount; i++)
        {
            foreach (float baseAngle in baseAngles)
            {
                float currentAngle = baseAngle + (i * angleIncrement);
                SpawnProjectile(currentAngle);
            }
            yield return new WaitForSeconds(shootInterval);
        }

        Destroy(gameObject); 
    }

    void SpawnProjectile(float angle)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
        Destroy(projectile, 10f); 
    }
}
