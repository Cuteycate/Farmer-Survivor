using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage; 
    public int per; 
    public int projectileNumber; 
    public float hitCooldown;

    private Rigidbody2D rigid;
    private Dictionary<Collider2D, float> hitTimestamps = new Dictionary<Collider2D, float>();

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, int per, Vector3 dir, int projectileNumber, float hitCooldown)
    {
        this.damage = damage;
        this.per = per;
        this.projectileNumber = projectileNumber;
        this.hitCooldown = hitCooldown;

        if (per > -1)
        {
            rigid.velocity = dir * 15f;
        }
        if (per == -2)
        {
            rigid.velocity = dir * 30f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || !collision.CompareTag("Enemy") || per == -1 || per == -2)
            return;

        float currentTime = Time.time;

        // Check hit cooldown for the current enemy
        if (hitTimestamps.TryGetValue(collision, out float lastHitTime))
        {
            if (currentTime - lastHitTime < hitCooldown)
            {
                return; // Skip if still on cooldown
            }
        }

        // Update the hit timestamp
        hitTimestamps[collision] = currentTime;

        per--;

        if (per == -1)
        {
            AfterImageGenerator afterImageGenerator = GetComponent<AfterImageGenerator>();
            if (afterImageGenerator != null)
            {
                afterImageGenerator.StopAfterImages();
            }
            rigid.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }

        if (collision.CompareTag("BulletBoss"))
        {
            rigid.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || !other.CompareTag("Area"))
            return;

        if (other.CompareTag("Area"))
        {
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
