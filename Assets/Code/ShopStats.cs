using UnityEngine;

public class ShopStats : MonoBehaviour
{
    public static ShopStats Instance { get; private set; }
    public float damageMultiplier = 1f;      // Additional multiplier for damage upgrades
    public float maxhealthMultiplier = 1f;
    public float healthrecoveryMultiplier = 0f;
    public float movementspeedMultiplier = 1f;
    public int projectileMultiplier = 0;
    public float cooldownMultiplier = 1f;
    public float xpMultiplier = 1f;
    public float goldMultiplier = 1f;
    public float magnetMultiplier = 1f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }
    }
    // Method to apply an upgrade based on shopID

    public void ApplyUpgrade(int shopID, float upgradeValue)
    {
        switch (shopID)
        {
            case 0: // Might
                IncreaseDamage(upgradeValue);
                break;
            case 1: // Increase Max Health (ID 1)
                IncreaseMaxHealth(upgradeValue);
                break;
            case 2: // Increase Health Recovery (ID 2)
                IncreaseHealthRecovery(upgradeValue);
                break;
            case 3: // Increase Character Movement Speed (ID 3)
                IncreaseMovementSpeed(upgradeValue);
                break;
            case 4: //Increase ProjectileCount (ID 4)
                IncreaseCountProjectile((int)upgradeValue);
                break;
            case 5: //Decrease Cooldown (ID 5)
                DecreaseCoolDown(upgradeValue);
                break;
            case 6: //Increase XP magnet range (ID 6)
                IncreaseMagnet(upgradeValue);
                break;
            case 7: //Increase EXP gain Cooldown (ID 7)
                IncreaseXPRate(upgradeValue);
                break;
            case 8:
                IncreaseGoldRate(upgradeValue);
                break;
            default:
                Debug.LogWarning("Unknown shopID: " + shopID);
                break;
        }
        SaveStats();
    }

    // Tăng dame theo %
    private void IncreaseDamage(float value)
    {
        damageMultiplier = 1 + value; // Cộng dồn dame theo chỉ số %
        Debug.Log("Damage upgraded! New multiplier: " + damageMultiplier);
    }

    private void IncreaseMaxHealth(float value)
    {
        maxhealthMultiplier = 1 + value;
        Debug.Log("Max Health upgraded ! " + value);
    }
    private void IncreaseHealthRecovery(float value)
    {
        healthrecoveryMultiplier = 0.1f * value;
        Debug.Log("Max Health upgraded ! " + value);
    }
    private void IncreaseMovementSpeed(float value)
    {
        movementspeedMultiplier = 1 + value;
        Debug.Log("Character speed upgraded ! " + value);

    }
    private void IncreaseCountProjectile(int value)
    {
        projectileMultiplier = value;
        Debug.Log("Projectile count increase ! " + value);
    }
    private void DecreaseCoolDown(float value)
    {
        cooldownMultiplier = 1 - value;
        Debug.Log("Projectile Cooldown decrease by :" + value);
    }
    private void IncreaseXPRate(float value)
    {
        xpMultiplier = 1 + value;
        Debug.Log("Enemy XP Rate increases by :" + value);
    }
    private void IncreaseGoldRate(float value)
    {
        goldMultiplier = 1 + value;
        Debug.Log("Gold multiplier by :" + value);
    }
    private void IncreaseMagnet(float value)
    {
        magnetMultiplier = 1 + value;
        Debug.Log("Magnet multiplied by :" + value);
    }
    // Lưu Stats qua PlayerPrefs
    public void SaveStats()
    {
        PlayerPrefs.SetFloat("DamageMultiplier", damageMultiplier);
        PlayerPrefs.SetFloat("MaxHealthMultiplier", maxhealthMultiplier);
        PlayerPrefs.SetFloat("HealthRecoveryMultiplier", healthrecoveryMultiplier);
        PlayerPrefs.SetFloat("MovementSpeedMultiplier", movementspeedMultiplier);
        PlayerPrefs.SetInt("IncreaseCountProjectile", projectileMultiplier);
        PlayerPrefs.SetFloat("DecreaseCoolDown", cooldownMultiplier);
        PlayerPrefs.SetFloat("IncreaseXPRate", xpMultiplier);
        PlayerPrefs.SetFloat("GoldMultiplier", goldMultiplier);
        PlayerPrefs.SetFloat("MagnetMultiplier", magnetMultiplier);
        PlayerPrefs.Save();
    }

    // Load Stats qua PlayerPrefs
    public void LoadStats()
    {
       damageMultiplier = PlayerPrefs.GetFloat("DamageMultiplier", 1f);  // Default 1f nếu như không lưu
       maxhealthMultiplier = PlayerPrefs.GetFloat("MaxHealthMultiplier", 1f);
       healthrecoveryMultiplier = PlayerPrefs.GetFloat("HealthRecoveryMultiplier", 0f);
       movementspeedMultiplier = PlayerPrefs.GetFloat("MovementSpeedMultiplier", 1f);
       projectileMultiplier = PlayerPrefs.GetInt("IncreaseCountProjectile", 0);
       cooldownMultiplier = PlayerPrefs.GetFloat("DecreaseCoolDown", 1f);
       xpMultiplier = PlayerPrefs.GetFloat("IncreaseXPRate", 1f);
       goldMultiplier = PlayerPrefs.GetFloat("GoldMultiplier", 1f);
       magnetMultiplier = PlayerPrefs.GetFloat("MagnetMultiplier", 1f);
    }
}
