using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPickUp : MonoBehaviour
{
    public float expAmount; // Kinh nghiệm
    public SpriteRenderer spriteRenderer; // Để hiển thị sprite
    public List<Sprite> xpSprites; // Danh sách sprites cho các cấp
    public float magnetSpeed = 10f; // Tốc độ hút
    public float mergeRadius = 0.5f; // Bán kính kết hợp
    private Transform playerTransform;
    public bool isBeingPulled = false;

    private bool isMerging = false; // EXP có thể merge không
    private Transform mergeTarget; // Mục tiêu để merge

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        SetSpriteBasedOnExp(); // Cập nhật sprite ngay khi khởi tạo
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        if (isBeingPulled && playerTransform != null)
        {
            // Di chuyển XP tới người chơi
            Debug.Log($"Moving towards player at {playerTransform.position}.");
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
        }

        if (isMerging && mergeTarget != null && !isBeingPulled)
        {
            transform.position = mergeTarget.position;
            CompleteMerge();
        }
    }

    public void SetSpriteBasedOnExp()
    {
        int tier = GetSpriteTier(expAmount);
        if (xpSprites != null && tier < xpSprites.Count)
        {
            spriteRenderer.sprite = xpSprites[tier]; // Cập nhật sprite theo tier
        }
    }

    public void ResetState(float newExpAmount)
    {
        expAmount = newExpAmount;
        isBeingPulled = false;
        isMerging = false;
        playerTransform = null;
        mergeTarget = null;
        SetSpriteBasedOnExp(); // Cập nhật sprite khi reset
        gameObject.SetActive(true);
    }

    public void SetMergeSpriteBasedOnExp(float totalExp)
    {
        int newTier = GetSpriteTier(totalExp);
        if (xpSprites != null && newTier < xpSprites.Count)
        {
            spriteRenderer.sprite = xpSprites[newTier]; // Cập nhật sprite mới sau merge
        }
    }

    int GetSpriteTier(float amount)
    {
        if (amount >= 1 && amount <= 5) return 0;
        if (amount >= 6 && amount <= 10) return 1;
        if (amount >= 11 && amount <= 15) return 2;
        return 3;
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
            GameManager gameManager = GameManager.instance;

            if (gameManager != null && gameManager.isLive)
            {
                // Truyền XP cho người chơi
                gameManager.exp += expAmount * (1 + gameManager.ExtraRateExp) * ShopStats.Instance.xpMultiplier;

                // Kiểm tra level up
                if (gameManager.exp >= gameManager.nextExp[Mathf.Min(gameManager.level, gameManager.nextExp.Length - 1)])
                {
                    gameManager.level++;
                    gameManager.exp = 0;
                    gameManager.uiLevelUp.Show();
                }

                AudioManager.instance.PlaySfx(AudioManager.Sfx.ExpPickUp);
                gameObject.SetActive(false); // Ẩn XP sau khi nhặt
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

    public void CheckForNearbyMerges()
    {
        Collider2D[] nearbyPickups = Physics2D.OverlapCircleAll(transform.position, mergeRadius);
        List<ExpPickUp> mergeablePickups = new List<ExpPickUp>();
        float totalExp = expAmount;
        int currentTier = GetSpriteTier(expAmount);
        float expNeeded = GetExpForNextTier(currentTier) - expAmount;

        foreach (Collider2D collider in nearbyPickups)
        {
            if (collider != null && collider.gameObject != gameObject && collider.CompareTag("ExpPickUp"))
            {
                ExpPickUp otherExpPickUp = collider.GetComponent<ExpPickUp>();
                if (otherExpPickUp != null && !otherExpPickUp.isMerging)
                {
                    int otherTier = GetSpriteTier(otherExpPickUp.expAmount);

                    if (otherTier == currentTier)
                    {
                        mergeablePickups.Add(otherExpPickUp);
                    }
                }
            }
        }

        mergeablePickups.Sort((a, b) => a.expAmount.CompareTo(b.expAmount));

        List<ExpPickUp> selectedPickups = new List<ExpPickUp>();
        foreach (ExpPickUp pickup in mergeablePickups)
        {
            if (expNeeded <= 0) break;

            selectedPickups.Add(pickup);
            expNeeded -= pickup.expAmount;
        }

        if (selectedPickups.Count > 0 && expNeeded <= 0)
        {
            foreach (ExpPickUp pickup in selectedPickups)
            {
                pickup.isMerging = true;
                pickup.gameObject.SetActive(false); // Instantly "consume" the merging pickup
                totalExp += pickup.expAmount;
            }

            expAmount = totalExp;
            SetMergeSpriteBasedOnExp(totalExp); // Update the sprite for the new total EXP
            CheckForNearbyMerges(); // Check again for possible merges
        }
    }

    private float GetExpForNextTier(int tier)
    {
        if (tier == 0) return 6f;
        if (tier == 1) return 11f;
        if (tier == 2) return 16f;
        return float.MaxValue;
    }

    private IEnumerator PullPickupToMerge(ExpPickUp pickup)
    {
        while (pickup != null && Vector2.Distance(pickup.transform.position, transform.position) > 0.1f)
        {
            pickup.transform.position = Vector2.MoveTowards(pickup.transform.position, transform.position, magnetSpeed * Time.deltaTime);
            yield return null;
        }

        if (pickup != null)
        {
            pickup.gameObject.SetActive(false);
        }
    }

    void CompleteMerge()
    {
        isMerging = false;
        mergeTarget = null;
        CheckForNearbyMerges();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);
    }
}
