using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public int id;
    public ItemData.ItemType type;
    public float rate;
    private float baseMaxHealth;
    private float baseSpeed;
    public Sprite Icon;
    public int level = 1;
    public int maxlevel;
    public void Init(ItemData data)
    {
        //Basic set
        id = data.itemId;
        name = "Gear" + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;
        //Property set
        if (id == 12)
        {
            type = data.itemType;
            rate = data.damages[0];
            Icon = data.itemIcon;
            maxlevel = data.damages.Length;
            IncreaseRadius();
        }
        else
        {
            type = data.itemType;
            rate = data.damages[0];
            if (baseMaxHealth == 0f)
            {
                baseMaxHealth = GameManager.instance.MaxHealth;
            }
            if (baseSpeed == 0f)
            {
                baseSpeed = GameManager.instance.player.speed;
            }
            Icon = data.itemIcon;
            maxlevel = data.damages.Length;
            ApplyGear();
        }
    }
    public void LevelUp(float rate)
    {
        if (id == 12)
        {
            this.rate = rate;
            level++;
            IncreaseRadius();
        }
        else
        {
            this.rate = rate;
            level++;
            ApplyGear();
        }
    }
    void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
            case ItemData.ItemType.EmptyHeart:
                MaxHealthUp();
                break;
            case ItemData.ItemType.ExtraProjectile:
                ProjectileUp();
                break;
            case ItemData.ItemType.Bandage:
                RecoverHp();
                break;
            case ItemData.ItemType.XpCrown:
                IncreaseXp();
                break;
        }
    }
    public void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            float calculatedSpeed = Weapon.GetBaseCoolDown(weapon.id);
            float rateAdjustedSpeed = calculatedSpeed * (1f - rate) * Character.WeaponRate;

            switch (weapon.id)
            {
                case 0:
                    float speed = 150 * Character.WeaponSpeed;
                    weapon.speed = speed + (speed * rate);
                    float fireRate = rateAdjustedSpeed;
                    weapon.MeleeCoolDown = fireRate * (1f - rate);
                    weapon.BroadcastMessage("Batch", SendMessageOptions.DontRequireReceiver);
                    break;
                case 1:
                case 8:
                case 9:
                case 10:
                case 11:
                case 13:
                    weapon.speed = rateAdjustedSpeed;
                    break;
                case 14:
                    weapon.speed = rateAdjustedSpeed * (1f - weapon.size);
                    break;
                default:
                    break;
            }
        }
    }

    void SpeedUp()
    {
        GameManager.instance.player.speed = baseSpeed * (1 + rate);
    }
    void MaxHealthUp()
    {
        GameManager.instance.MaxHealth = baseMaxHealth * (1 + rate);
    }
    void ProjectileUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id)
            {
                case 0:
                    weapon.ExtraCount = Mathf.Min((int)rate, 2);
                    weapon.ExtraCountStatic = Mathf.Min((int)rate, 2);
                    weapon.BroadcastMessage("Batch", SendMessageOptions.DontRequireReceiver);
                    break;
                case 1:
                case 8:
                case 9:
                case 11:
                case 13:
                case 14:
                    weapon.ExtraCount = Mathf.Min((int)rate, 2);
                    weapon.ExtraCountStatic = Mathf.Min((int)rate, 2);
                    break;
            default:
                    break;
            }
        }
    }
    void RecoverHp()
    {
        float gearRecoveryRate = 0.2f * rate; //Tinh rate sau do cho truyen vao starthealthrecovery
        GameManager.instance.player.StartHealthRecovery(gearRecoveryRate);
    }
    void IncreaseXp()
    {
        GameManager.instance.ExtraRateExp = rate;
    }
    void IncreaseRadius()
    {
        if (id == 12)
        {
            ItemCollider itemCollider = GameManager.instance.player.GetComponentInChildren<ItemCollider>();

            if (itemCollider != null)
            {
                // Tinh Radius moi
                float newRadius = itemCollider.circleCollider.radius * (1f + rate);
                itemCollider.UpdateRadius(newRadius);
            }
        }
    }
}
