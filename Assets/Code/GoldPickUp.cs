using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldPickUp : MonoBehaviour
{
    public int goldAmount = 50;
    public float magnetSpeed = 5f; // Tốc độ Magnet
    private Transform playerTransform;
    private bool isBeingPulled = false;

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        if (isBeingPulled && playerTransform != null)
        {
            // di chuyển XP Orb tới vị trí của người chơi
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CircleCollider"))
        {
            isBeingPulled = true;
            playerTransform = other.transform;
        }
        if (other.CompareTag("Player"))
        {
            // Lấy gameManager
            GameManager gameManager = GameManager.instance;

            // nếu gamemamanger còn hoạt đồng và nhân vật còn sống
            if (gameManager != null && gameManager.isLive)
            {
                gameManager.gold += Mathf.RoundToInt(goldAmount * ShopStats.Instance.goldMultiplier);

                AudioManager.instance.PlaySfx(AudioManager.Sfx.GoldPickUp);
                gameObject.SetActive(false);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("CircleCollider"))
        {
            isBeingPulled = false;
            playerTransform = null;
        }
    }
}
