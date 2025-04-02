using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public List<Canvas> ListWeapon;
    public List<Canvas> ListGear;
    public List<Weapon> weapons = Item.ListWeapon;
    public List<Gear> gears = Item.ListGear;
    public bool canmove = false;
    void Start()
    {
        if (Application.isMobilePlatform)
        {
            if (canmove) {
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Adjust the position on the X-axis by 15 units
                    Vector3 newPosition = rectTransform.localPosition;
                    newPosition.x += 15;
                    rectTransform.localPosition = newPosition;
                }
            }
        }
    }
    private void Update()
    {
  
        for (int i = 0; i < ListWeapon.Count && i < weapons.Count; i++)
        {
            UpdateWeaponCanvas(ListWeapon[i], weapons[i]);
            ListWeapon[i].gameObject.SetActive(true); 

        }

        for (int i = weapons.Count; i < ListWeapon.Count; i++)
        {
            ListWeapon[i].gameObject.SetActive(false); 
        }

        for (int i = 0; i < ListGear.Count && i < gears.Count; i++)
        {
            UpdateGearCanvas(ListGear[i], gears[i]);
            ListGear[i].gameObject.SetActive(true); 
        }

        for (int i = gears.Count; i < ListGear.Count; i++)
        {
            ListGear[i].gameObject.SetActive(false); 
        }
    }

    private void UpdateWeaponCanvas(Canvas weaponCanvas, Weapon weapon)
    {

        Image imageComponent = weaponCanvas.GetComponentInChildren<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = weapon.Icon;
        }
        Text textComponent = weaponCanvas.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            string text;
            if (weapon.maxlevel == weapon.level)
            {
                text = "Max";
            }
            else
            {
                text = "lv" + weapon.level;
            }
            textComponent.text = text;
        }

    }

    private void UpdateGearCanvas(Canvas gearCanvas, Gear gear)
    {

        Image imageComponent = gearCanvas.GetComponentInChildren<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = gear.Icon; 
        }
        Text textComponent = gearCanvas.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            string text;
            if (gear.maxlevel == gear.level)
            {
                text = "Max";
            }
            else
            {
                text = "lv" + gear.level;
            }
            textComponent.text = text;
        }
    }
}
