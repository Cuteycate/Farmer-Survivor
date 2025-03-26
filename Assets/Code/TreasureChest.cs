using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    public RectTransform rect; // Chest RectTransform
    public RectTransform goldTitleRect; // Gold title RectTransform
    public RectTransform imageToShake; // Image inside chest to shake
    Item[] items;
    public LevelUp uiLevelUp;
    public Text goldText;
    public GameObject goldRainEffect;
    public GameObject ChestOpen;
    public GameObject Chest;
    public List<ChestIcon> chestIcons;
    public GameObject ChestPopUp;
    private bool isChestClicked = false;
    public ChestPopUp chestPopUp;
    public HashSet<int> unlockedItems;
    public List<int> activatedItems = new List<int>();
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
        unlockedItems = new HashSet<int>();
    }
    public void Show()
    {
        rect.localScale = Vector3.one; 
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.PauseBgm();
        AudioManager.instance.EffectBgm(true);
        goldText.text = "000";
        if (ChestPopUp != null)
        {
            ChestPopUp.transform.localScale = Vector3.zero;
        }
    }


    public void Hide()
    {
        if (!isChestClicked)
        {
            // Set the flag to true to interrupt shaking
            isChestClicked = true;
            AudioManager.instance.StopAllSfx();
        }
        else
        {
            // Scale down the rect and resume game actions
            rect.localScale = Vector3.zero;
            GameManager.instance.Resume();
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
            AudioManager.instance.EffectBgm(false);
            AudioManager.instance.ResumeBgm();
            isChestClicked = false;
            HideAllitems();
            // Deactivate VFX for each item
            foreach (var item in items)
            {
                GameObject itemGameObject = item.gameObject;
                Transform vfxTransform = itemGameObject.transform.Find("ItemVFX");

                if (vfxTransform != null)
                {
                    vfxTransform.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"VFX not found in item: {item.name}");
                }
            }

            // Call HideAllitems to handle additional deactivations
            HideAllEffects();
        }
    }

    public void HideAllitems()
    {
        // Deactivate all item game objects
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }
    }
    public void HideAllEffects()
    {


        if (goldRainEffect != null)
        {
            goldRainEffect.SetActive(false);
        }

        // Set the Chest Open state to false and activate the Chest GameObject
        if (ChestOpen != null)
        {
            ChestOpen.SetActive(false); // Assuming chestOpen is the GameObject for the opened chest
        }

        if (Chest != null)
        {
            Chest.SetActive(true); // Assuming chest is the GameObject for the closed chest
        }
    }

    public void Next()
    {
        HideAllitems();

        // Create a list to store indices of upgradable items
        List<int> upgradableIndices = new List<int>();
        for (int i = 0; i < Item.allItemData.Count; i++)
        {
            ItemData currentItemData = Item.allItemData[i];
            for (int j = 0; j < items.Length; j++)
            {
                Item currentItem = items[j];

                // Check if the item's data matches and is not max level
                if (currentItem.data == currentItemData &&
                    Item.ItemLevels[currentItem.data.itemId] < currentItem.data.damages.Length)
                {
                    upgradableIndices.Add(j);
                    Debug.Log($"Added index {j} to upgradableIndices. Current count: {upgradableIndices.Count}");
                }
            }
        }

        // Start shaking and gold counting, and activate items afterward
        StartCoroutine(ShakeAndCountGold(upgradableIndices));
    }


    private IEnumerator ShakeAndCountGold(List<int> upgradableIndices)
    {
        // Tinh toan so vang
        goldRainEffect.SetActive(true);
        ParticleSystem burstSystem = goldRainEffect.GetComponent<ParticleSystem>();
        if (burstSystem != null)
        {
            burstSystem.Play();
        }
        Vector3 originalGoldTitlePosition = goldTitleRect.localPosition;
        Vector3 originalImagePosition = imageToShake != null ? imageToShake.localPosition : Vector3.zero;
        int baseGold = Random.Range(100, 450);
        float multiplier = ShopStats.Instance.goldMultiplier;
        int totalGold = Mathf.RoundToInt(baseGold * multiplier);



        // quyet dinh 
        int rarity = Random.Range(0, 100);
        int itemsToActivate = 0;
        float soundDuration = 0f;
        AudioManager.Sfx selectedSfx;
        if (upgradableIndices.Count >= 0)
        {
            if (rarity < 10 && upgradableIndices.Count > 2) // 3 items
            {
                itemsToActivate = Mathf.Min(3, upgradableIndices.Count);
                selectedSfx = AudioManager.Sfx.Treasuremusic2;
                soundDuration = AudioManager.instance.GetSfxLength(selectedSfx);
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Treasuremusic2); // choi rare sound effect
                ActivateChestIcon(3);
                StartCoroutine(Shake(goldTitleRect, soundDuration, soundDuration)); // lac vang
                if (imageToShake != null)
                {
                    StartCoroutine(Shake(imageToShake, 6f, 6f)); // Shake hinh
                }
            }
            else if (rarity < 40 && upgradableIndices.Count > 1) // Medium chance: Activate 2 items
            {
                itemsToActivate = Mathf.Min(2, upgradableIndices.Count);
                selectedSfx = AudioManager.Sfx.Treasuremusic1;
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Treasuremusic1); // choi medium sound effect
                soundDuration = AudioManager.instance.GetSfxLength(selectedSfx);
                ActivateChestIcon(2);
                StartCoroutine(Shake(goldTitleRect, soundDuration, soundDuration)); // lac vang
                if (imageToShake != null)
                {
                    StartCoroutine(Shake(imageToShake, soundDuration, soundDuration)); // Shake hinh
                }
            }
            else // mo mot item
            {
                itemsToActivate = 1;
                selectedSfx = AudioManager.Sfx.Treasuremusic0;
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Treasuremusic0); // choi item bth
                soundDuration = AudioManager.instance.GetSfxLength(selectedSfx);
                ActivateChestIcon(1);
                StartCoroutine(Shake(goldTitleRect, soundDuration, soundDuration)); // lac vang
                if (imageToShake != null)
                {
                    StartCoroutine(Shake(imageToShake, soundDuration, soundDuration)); // Shake hinh
                }
            }
        }

        int currentGold = 0;
        float duration = soundDuration > 0 ? soundDuration : 4f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (isChestClicked)
            {
                break;
            }

            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;

            currentGold = Mathf.RoundToInt(Mathf.Lerp(1, totalGold, progress * progress));
            goldText.text = currentGold.ToString();
            yield return null;
        }
        isChestClicked = true;
        ActivateGoldBurstsForCurrentChest();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Success);
        goldText.text = totalGold.ToString();
        StopAllCoroutines();
        goldTitleRect.localPosition = originalGoldTitlePosition;
        if (imageToShake != null)
        {
            imageToShake.localPosition = originalImagePosition;
        }
        

        // Activate the items visually after animation
        ActivateItemsHelper(upgradableIndices, itemsToActivate,chestPopUp);
        chestPopUp.InitializeChest();
        if (ChestPopUp != null)
        {
            ChestPopUp.transform.localScale = Vector3.one;
        }
        // Update game gold
        GameManager.instance.gold += totalGold;
    }
    public void ActivateItemsHelper(List<int> indices, int count, ChestPopUp chestPopUp)
    {
        // Clear previous chest items from ChestPopUp
        chestPopUp.ClearChestItems();

        for (int i = 0; i < count; i++)
        {
            if (indices.Count > 0)
            {
                // Select a random index from the list of available indices
                int randomIndex = Random.Range(0, indices.Count);
                int activatedIndex = indices[randomIndex];

                Debug.Log("Activated index: " + activatedIndex);

                // Access the associated GameObject from the Item
                GameObject itemGameObject = items[activatedIndex].gameObject;
                itemGameObject.SetActive(true);

                // Activate the VFX child if it exists
                Transform vfxTransform = itemGameObject.transform.Find("ItemVFX");
                if (vfxTransform != null)
                {
                    vfxTransform.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"VFX child not found for item at index: {activatedIndex}");
                }

                // Notify UI about the activation
                uiLevelUp.Select(activatedIndex);

                // Record the activated item for later use in ChestPopUp
                activatedItems.Add(activatedIndex);

                // Add the GameObject directly to ChestPopUp
                chestPopUp.AddChestItem(itemGameObject);
                chestPopUp.InitializeChest();
                if (ChestPopUp != null)
                {
                    ChestPopUp.transform.localScale = Vector3.one;
                }
                // Remove the used index from the list
                indices.RemoveAt(randomIndex);
            }
            else
            {
                Debug.LogWarning("Not enough upgradable items to activate.");
                GameObject itemGameObject = items[15].gameObject;
                itemGameObject.SetActive(true);
                uiLevelUp.Select(15);
                break;
            }
        }
    }



    private IEnumerator Shake(RectTransform target, float duration, float magnitude)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.unscaledDeltaTime; // Use unscaled time here
            yield return null;
        }

        target.localPosition = originalPosition; // Reset to original position
    }
    private void ActivateChestIcon(int itemCount)
    {
        // Deactivate all chest icons first
        foreach (var chestIcon in chestIcons)
        {
            chestIcon.Deactivate();
        }

        // Activate the correct chest icon based on item count
        if (itemCount > 0 && itemCount <= chestIcons.Count)
        {
            chestIcons[itemCount - 1].Activate();
        }
    }
    private void ActivateGoldBurstsForCurrentChest()
    {
        foreach (var chestIcon in chestIcons)
        {
            if (chestIcon.icon.activeSelf)
            {
                chestIcon.ActivateGoldBursts();
                break;
            }
        }
    }
    public List<int> GetActivatedItems()
    {
        return activatedItems;
    }
}

