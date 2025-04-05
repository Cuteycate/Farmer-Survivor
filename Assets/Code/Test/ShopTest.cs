// ShopTest.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ShopTest
{
    // Shop elements
    private GameObject mainShopButton; // Button on the initial screen to OPEN the shop menu
    private GameObject shopItem1Button, shopItem4Button; // Items WITHIN the shop menu
    private GameObject itemPopUpPanel; // The persistent ItemPopUp GameObject
    private Button buyButton;
    private Shop shop1Script, shop4Script; // References to the Shop components on items

    // Scene name to load
    private const string GameSceneName = "Scenes/SampleScene"; // Adjust if needed

    // --- Game Config Assumptions (ADJUST THESE) ---
    private const int SufficientTotalGoldForTest = 500; // Ensure enough gold based on actual costs
    private const int InsufficientTotalGoldForTest = 10; // Ensure NOT enough gold

    // --- UI Paths/Names (ADJUST THESE) ---
    private const string MainShopButtonPath = "Canvas/SafeArea/GameStart/Button Canvas/Shop"; // Button to OPEN the shop menu
    private const string ShopItem1Path = "Canvas/SafeArea/Shop Menu/List/Shop Group Viewport/Shop Group Content/Shop 1"; // Item 1 INSIDE the shop menu
    private const string ShopItem4Path = "Canvas/SafeArea/Shop Menu/List/Shop Group Viewport/Shop Group Content/Shop 4"; // Item 4 INSIDE the shop menu
    private const string ItemPopUpObjectName = "Item Pop Up"; // Name of the GameObject holding the ItemPopUp script <<< ADJUST
    private const string BuyButtonName = "Buy"; // Name of the buy button GameObject within the popup

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[ShopTest.SetUp] Cleared PlayerPrefs.");
        Item.ResetItems();
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[ShopTest.TearDown] Cleared PlayerPrefs.");
        Item.ResetItems();
        Time.timeScale = 1f;
        mainShopButton = shopItem1Button = shopItem4Button = itemPopUpPanel = null;
        buyButton = null; shop1Script = shop4Script = null;
    }

    // --- Helper to wait until the Item Popup is scaled up ---
    private IEnumerator WaitUntilPopupScaledUp(float timeout)
    {
        if (itemPopUpPanel == null)
        {
             itemPopUpPanel = GameObject.Find(ItemPopUpObjectName);
             Assert.IsNotNull(itemPopUpPanel, $"Persistent Item Popup Panel '{ItemPopUpObjectName}' not found!");
        }
        float timer = 0f;
        while (timer < timeout)
        {
            if (Vector3.Distance(itemPopUpPanel.transform.localScale, Vector3.one) < 0.01f)
            {
                 Debug.Log("Popup detected as scaled up.");
                 yield break;
            }
            yield return null;
            timer += Time.unscaledDeltaTime;
        }
        Assert.Fail($"Item Popup '{ItemPopUpObjectName}' did not scale up within {timeout} seconds. Current scale: {itemPopUpPanel.transform.localScale}");
    }

    // --- Shop Test Cases ---

    [UnityTest]
    public IEnumerator Test_Shop_01_UpgradeItem1_SufficientGold()
    {
        Debug.Log("--- Starting Test_Shop_01_UpgradeItem1_SufficientGold ---");
        yield return new WaitForSeconds(1.0f);

        mainShopButton = GameObject.Find(MainShopButtonPath);
        Assert.IsNotNull(mainShopButton, $"Main Shop Button at '{MainShopButtonPath}' not found!");
        mainShopButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        // --- Arrange: Use totalGold ---
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null - needed for gold.");
        GameManager.instance.totalGold = SufficientTotalGoldForTest; 
        int initialTotalGold = GameManager.instance.totalGold;

        shopItem1Button = GameObject.Find(ShopItem1Path);
        Assert.IsNotNull(shopItem1Button, $"Shop Item 1 at '{ShopItem1Path}' not found.");
        shop1Script = shopItem1Button.GetComponent<Shop>();
        Assert.IsNotNull(shop1Script, "Shop component not found on Shop Item 1.");
        int initialLevel = shop1Script.data.currentLevel;
        Debug.Log($"Shop 1 Initial - Total Gold: {initialTotalGold}, Level: {initialLevel}"); 

        // --- Act ---
        shopItem1Button.GetComponent<Button>().onClick.Invoke();
        yield return WaitUntilPopupScaledUp(5f);

        buyButton = itemPopUpPanel.transform.Find(BuyButtonName)?.GetComponent<Button>();
        Assert.IsNotNull(buyButton, $"'{BuyButtonName}' button not found.");
        Assert.IsTrue(buyButton.interactable, "Buy Button should be interactable.");
        buyButton.onClick.Invoke();
        yield return new WaitForSeconds(3f);

        // --- Assert: Check totalGold ---
        int finalLevel = shop1Script.data.currentLevel;
        int finalTotalGold = GameManager.instance.totalGold;
        Debug.Log($"Shop 1 Final - Total Gold: {finalTotalGold}, Level: {finalLevel}");
        int expectedCost = (shop1Script.data.Golds.Length > initialLevel) ? shop1Script.data.Golds[initialLevel] : -1;
        Assert.AreNotEqual(-1, expectedCost, "Could not determine expected cost.");

        Assert.AreEqual(initialTotalGold - expectedCost, finalTotalGold, "Total Gold was not deducted correctly."); 
        Assert.AreEqual(initialLevel + 1, finalLevel, "Shop item level did not increase.");
        Debug.Log("✅ Test_Shop_01_UpgradeItem1_SufficientGold Passed!");
    }


    [UnityTest]
    public IEnumerator Test_Shop_02_CannotUpgradeItem4_InsufficientGold()
    {
        Debug.Log("--- Starting Test_Shop_02_CannotUpgradeItem4_InsufficientGold ---");
        yield return new WaitForSeconds(1.0f);

        mainShopButton = GameObject.Find(MainShopButtonPath);
        Assert.IsNotNull(mainShopButton, $"Main Shop Button at '{MainShopButtonPath}' not found!");
        mainShopButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        //--- Arrange: Use totalGold ---
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null - needed for gold.");
        GameManager.instance.totalGold = InsufficientTotalGoldForTest; 
        int initialTotalGold = GameManager.instance.totalGold;

        shopItem4Button = GameObject.Find(ShopItem4Path);
        Assert.IsNotNull(shopItem4Button, $"Shop Item 4 at '{ShopItem4Path}' not found.");
        shop4Script = shopItem4Button.GetComponent<Shop>();
        Assert.IsNotNull(shop4Script, "Shop component not found on Shop Item 4.");
        int initialLevel = shop4Script.data.currentLevel;
        Debug.Log($"Shop 4 Initial - Total Gold: {initialTotalGold}, Level: {initialLevel}");

        // --- Act ---
        shopItem4Button.GetComponent<Button>().onClick.Invoke();
        yield return WaitUntilPopupScaledUp(5f);

        buyButton = itemPopUpPanel.transform.Find(BuyButtonName)?.GetComponent<Button>();
        Assert.IsNotNull(buyButton, $"'{BuyButtonName}' button not found.");

        // --- Assert: Check totalGold ---
        Assert.IsFalse(buyButton.interactable, "Buy Button should NOT be interactable.");
        yield return new WaitForSeconds(0.2f);
        int finalLevel = shop4Script.data.currentLevel;
        int finalTotalGold = GameManager.instance.totalGold;
        Assert.AreEqual(initialTotalGold, finalTotalGold, "Total Gold should not have changed."); 
        Assert.AreEqual(initialLevel, finalLevel, "Shop item level should not have changed.");
        Debug.Log("✅ Test_Shop_02_CannotUpgradeItem4_InsufficientGold Passed!");
    }

}