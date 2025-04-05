// ItemUpgradeTest.cs (Standalone)
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ItemUpgradeTest // <<< REMOVED ": UITest"
{
    // --- Variables needed for Setup and Test ---
    // Copied from UITest as we no longer inherit
    private GameObject startButton, mapButton, characterButton, playerObject;
    private Player playerScript;
    private Rigidbody2D rigidbody2D;

    // UI element references specific to Level Up
    private GameObject levelUpPanel;
    private GameObject levelUpItemGroup;

    // Scene name to load
    private const string GameSceneName = "Scenes/SampleScene"; // Adjust if needed

    // --- Constants for Level Up UI ---
    private const string LevelUpPanelPath = "Canvas/SafeArea/LevelUp";
    private const string LevelUpItemGroupPath = "Canvas/SafeArea/LevelUp/Panel/ItemGroup";

    [SetUp]
    public void SetUp() // No 'override' needed
    {
        // --- Standalone Setup ---
        // Clear persistent data relevant to items/leveling
        Item.ResetItems();
        // PlayerPrefs.DeleteAll(); // Add if level up choices save state we need reset

        // Load the scene
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    [TearDown]
    public void TearDown() // No 'override' needed
    {
        // --- Standalone Teardown ---
        // Reset relevant static data
        Item.ResetItems();
        Time.timeScale = 1f;

        // Clear local references
        levelUpPanel = null;
        levelUpItemGroup = null;
        startButton = mapButton = characterButton = playerObject = null;
        playerScript = null; rigidbody2D = null;
    }

    // --- Helper Coroutine for Setup (Copied from UITest) ---
    private IEnumerator PerformSetupAndReachGameplay()
    {
        yield return new WaitForSeconds(1.5f); // Increased wait
        Assert.IsNotNull(GameManager.instance, "GameManager instance not found after scene load!");

        startButton = GameObject.Find("Canvas/SafeArea/GameStart/Button Canvas/Start");
        mapButton = GameObject.Find("Canvas/SafeArea/GameStart/Choose Map/Map 1");
        characterButton = GameObject.Find("Canvas/SafeArea/GameStart/Character Group/Character 0");

        Assert.IsNotNull(startButton, "Start Button not found!");
        Assert.IsNotNull(mapButton, "Map Button not found!");
        Assert.IsNotNull(characterButton, "Character Button not found!");

        startButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.8f);
        mapButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.8f);
        characterButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(1.5f); // Wait for player spawn & init

        playerObject = GameObject.FindWithTag("Player");
        Assert.IsNotNull(playerObject, "Player object with tag 'Player' not found!");
        playerScript = playerObject.GetComponent<Player>();
        rigidbody2D = playerObject.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(playerScript, "Player script not found!");
        Assert.IsNotNull(rigidbody2D, "Rigidbody2D not found!");

        Debug.Log("✅ Setup to Gameplay Complete.");
    }

    private IEnumerator WaitUntilActive(string path, float timeout, Action<GameObject> onFound)
    {
        GameObject element = null;
        RectTransform rect = null;
        float timer = 0f;
        Debug.Log($"Waiting for '{path}' to become active (scale up)...");

        while (timer < timeout)
        {
            element = GameObject.Find(path);
            if (element != null)
            {
                rect = element.GetComponent<RectTransform>(); // Get the RectTransform
                if (rect != null && Vector3.Distance(rect.localScale, Vector3.one) < 0.01f)
                {
                    Debug.Log($"Found active element '{path}' (scaled to 1).");
                    onFound(element); // Pass the found GameObject back
                    yield break; // Found and scaled up
                }
            }
            yield return null; // Wait a frame
            timer += Time.unscaledDeltaTime;
        }
        Assert.Fail($"Element at path '{path}' did not scale up to (1,1,1) within {timeout} seconds. Current scale: {(rect != null ? rect.localScale.ToString() : "Not Found")}");
    }

    // Helper to find the LevelUp GameObject and wait until its scale is (0,0,0)
    private IEnumerator WaitUntilInactive(string path, float timeout)
    {
        GameObject element = null;
        RectTransform rect = null;
        float timer = 0f;
        Debug.Log($"Waiting for '{path}' to become inactive (scale down)...");

        while (timer < timeout)
        {
            element = GameObject.Find(path);
            if (element != null)
            {
                rect = element.GetComponent<RectTransform>();
                // đợi tìm thấy và vector trở thành 0
                if (rect != null && Vector3.Distance(rect.localScale, Vector3.zero) < 0.01f)
                {
                    Debug.Log($"Element '{path}' detected as inactive (scaled to 0).");
                    yield break; // tìm thấy và đã biến mất
                }
            }
            else
            {
                // game object biến mất ==> inactive
                Debug.Log($"Element '{path}' no longer found, considering inactive.");
                yield break;
            }

            yield return null; // Wait a frame
            timer += Time.unscaledDeltaTime;
        }
        Assert.Fail($"Element at path '{path}' did not scale down to (0,0,0) within {timeout} seconds. Current scale: {(rect != null ? rect.localScale.ToString() : "Not Found")}");
    }

    // Kiểm tra xem có phải vũ khí không
    private bool IsWeapon(ItemData data)
    {
        if (data == null) return false;
        // Use the same IDs as defined in LevelUp.Next()
        List<int> weaponIds = new List<int> { 0, 1, 8, 9, 10, 11, 13, 14 };
        return weaponIds.Contains(data.itemId);
    }

    // Kiểm tra xem có phải gear không
    private bool IsGear(ItemData data)
    {
        if (data == null) return false;
        // Use the same IDs as defined in LevelUp.Next()
        List<int> gearIds = new List<int> { 2, 3, 4, 5, 6, 7, 12 };
        return gearIds.Contains(data.itemId);
    }


    [UnityTest]
    public IEnumerator Test_01_LevelUp_SelectsFirstAvailableOption() // Renamed for ordering
    {
        // ... Implementation from previous step ...
        Debug.Log("--- Starting Test_01_LevelUp_SelectsFirstAvailableOption ---");
        yield return PerformSetupAndReachGameplay();
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null.");
        GameManager.instance.levelupbybuttonfortest();
        yield return WaitUntilActive(LevelUpPanelPath, 5f, foundObj => levelUpPanel = foundObj);
        levelUpItemGroup = GameObject.Find(LevelUpItemGroupPath);
        Assert.IsNotNull(levelUpItemGroup, "Level Up Item Group not found.");
        Button[] allUpgradeButtons = levelUpItemGroup.GetComponentsInChildren<Button>(true);
        List<Button> availableOptions = allUpgradeButtons.Where(btn => btn.gameObject.activeInHierarchy && btn.interactable).ToList();
        Assert.Greater(availableOptions.Count, 0, "No upgrade options found.");
        Button chosenButton = availableOptions[0];
        Item chosenItemScript = chosenButton.GetComponent<Item>();
        Assert.IsNotNull(chosenItemScript, "Chosen button missing 'Item' component.");
        Assert.IsNotNull(chosenItemScript.data, "Item component missing ItemData.");
        int itemId = chosenItemScript.data.itemId;
        string itemName = chosenItemScript.data.itemName;
        int initialLevel = Item.ItemLevels.ContainsKey(itemId) ? Item.ItemLevels[itemId] : -1;
        Debug.Log($"Selecting option: '{itemName}' (ID: {itemId}), Initial Level Dict: {initialLevel}");
        chosenButton.onClick.Invoke();
        yield return WaitUntilInactive(LevelUpPanelPath, 5f); // Wait for panel to close
        Assert.IsTrue(Item.ItemLevels.ContainsKey(itemId), $"Item ID {itemId} not added to Item.ItemLevels.");
        int finalLevel = Item.ItemLevels[itemId];
        int expectedFinalLevel = (initialLevel == -1) ? 0 : initialLevel + 1; // Adjust 0->1 if needed
        Assert.AreEqual(expectedFinalLevel, finalLevel, $"Item '{itemName}' level mismatch.");
        Debug.Log($"Final level for '{itemName}': {finalLevel}");
        Debug.Log("✅ Test_01_LevelUp_SelectsFirstAvailableOption Passed!");
    }


    [UnityTest]
    public IEnumerator Test_02_LevelUp_10_Times()
    {
        Debug.Log("--- Starting Test_02_LevelUp_10_Times ---");
        yield return PerformSetupAndReachGameplay();
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null.");

        int levelsToGain = 10;
        int initialPlayerLevel = GameManager.instance.level; // Game manager giữ level

        for (int i = 0; i < levelsToGain; i++)
        {
            Debug.Log($"--- Level Up Iteration {i + 1}/{levelsToGain} ---");

            // Kiểm tra game manager trước khi level up
            if (GameManager.instance == null)
            {
                Assert.Fail($"GameManager instance became null during level up iteration {i + 1}");
                yield break;
            }

            GameManager.instance.levelupbybuttonfortest();
            yield return WaitUntilActive(LevelUpPanelPath, 5f, foundObj => levelUpPanel = foundObj);

            levelUpItemGroup = GameObject.Find(LevelUpItemGroupPath);
            Assert.IsNotNull(levelUpItemGroup, $"Level Up Item Group not found on iteration {i + 1}.");

            Button[] allUpgradeButtons = levelUpItemGroup.GetComponentsInChildren<Button>(true);
            List<Button> availableOptions = allUpgradeButtons
                .Where(btn => btn.gameObject.activeInHierarchy && btn.interactable)
                .ToList();

            // If no options appear (maybe player max level?), handle appropriately
            if (availableOptions.Count == 0)
            {
                Assert.Fail($"No upgrade options available on iteration {i + 1}. Cannot complete 10 level ups.");
                yield break;
            }

            Button chosenButton = availableOptions[0];
            Item chosenItemScript = chosenButton.GetComponent<Item>(); // Get item script for logging
            string chosenItemName = (chosenItemScript != null && chosenItemScript.data != null) ? chosenItemScript.data.itemName : "Unknown Item";
            Debug.Log($"Choosing option {i + 1}: {chosenItemName}");
            chosenButton.onClick.Invoke();

            // Wait for the panel to disappear before the next iteration
            yield return WaitUntilInactive(LevelUpPanelPath, 5f);
        }

        // Final Assertions (Optional but recommended)
        Assert.AreEqual(initialPlayerLevel + levelsToGain, GameManager.instance.level, "GameManager player level did not increase by 10.");
        Debug.Log($"Completed 10 level ups. Final Player Level: {GameManager.instance.level}");
        Debug.Log("✅ Test_02_LevelUp_10_Times Passed!");
    }

    [UnityTest]
    public IEnumerator Test_03_LevelUp_Until_6_Weapons()
    {
        Debug.Log("--- Starting Test_03_LevelUp_Until_6_Weapons ---");
        yield return PerformSetupAndReachGameplay();
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null.");
        int targetWeaponCount = 6;
        int maxLevelUpAttempts = 50;
        int attempts = 0;
        Assert.AreEqual(1, Item.ListWeapon.Count, "Weapon list was not empty at test start.");
        while (Item.ListWeapon.Count < targetWeaponCount && attempts < maxLevelUpAttempts)
        {
            attempts++;
            Debug.Log($"--- Weapon Hunt Iteration {attempts}/{maxLevelUpAttempts} (Current Weapons: {Item.ListWeapon.Count}) ---");
            GameManager.instance.levelupbybuttonfortest();
            yield return WaitUntilActive(LevelUpPanelPath, 5f, foundObj => levelUpPanel = foundObj);
            levelUpItemGroup = GameObject.Find(LevelUpItemGroupPath);
            if (levelUpItemGroup == null) { Assert.Fail($"Level Up Item Group not found on attempt {attempts}."); yield break; }

            Button[] allUpgradeButtons = levelUpItemGroup.GetComponentsInChildren<Button>(true);
            List<Button> availableOptions = allUpgradeButtons.Where(btn => btn.gameObject.activeInHierarchy && btn.interactable).ToList();
            if (availableOptions.Count == 0) { Assert.Fail($"No upgrade options available on attempt {attempts}."); yield break; }

            Button weaponChoice = null;
            foreach (var btn in availableOptions)
            {
                Item itemScript = btn.GetComponent<Item>();
                // --- FIX IS HERE ---
                if (itemScript != null && itemScript.data != null && IsWeapon(itemScript.data)) // Pass the ItemData object
                {
                    weaponChoice = btn;
                    break;
                }
            }

            if (weaponChoice != null)
            {
                Item chosenItemScript = weaponChoice.GetComponent<Item>();
                string chosenItemName = (chosenItemScript?.data != null) ? chosenItemScript.data.itemName : "Unknown Weapon";
                Debug.Log($"Choosing WEAPON option: {chosenItemName}");
                weaponChoice.onClick.Invoke();
            }
            else
            {
                Button fallbackChoice = availableOptions[0];
                Item chosenItemScript = fallbackChoice.GetComponent<Item>();
                string chosenItemName = (chosenItemScript?.data != null) ? chosenItemScript.data.itemName : "Unknown Item";
                Debug.Log($"No weapon offered, choosing fallback: {chosenItemName}");
                fallbackChoice.onClick.Invoke();
            }

            yield return WaitUntilInactive(LevelUpPanelPath, 5f);
        }

        Assert.AreEqual(targetWeaponCount, Item.ListWeapon.Count, $"Did not reach {targetWeaponCount} weapons after {attempts} attempts. Found {Item.ListWeapon.Count}.");
        Debug.Log($"Reached {Item.ListWeapon.Count} weapons after {attempts} level up attempts.");
        Debug.Log("✅ Test_03_LevelUp_Until_6_Weapons Passed!");
    }


    [UnityTest]
    public IEnumerator Test_04_LevelUp_Until_6_Gears()
    {
        Debug.Log("--- Starting Test_04_LevelUp_Until_6_Gears ---");
        yield return PerformSetupAndReachGameplay();
        Assert.IsNotNull(GameManager.instance, "GameManager instance is null.");

        int targetGearCount = 6;
        int maxLevelUpAttempts = 50;
        int attempts = 0;
        Assert.AreEqual(0, Item.ListGear.Count, "Gear list was not empty at test start.");

        while (Item.ListGear.Count < targetGearCount && attempts < maxLevelUpAttempts)
        {
            attempts++;
            Debug.Log($"--- Gear Hunt Iteration {attempts}/{maxLevelUpAttempts} (Current Gears: {Item.ListGear.Count}) ---");

            GameManager.instance.levelupbybuttonfortest();
            yield return WaitUntilActive(LevelUpPanelPath, 5f, foundObj => levelUpPanel = foundObj);

            levelUpItemGroup = GameObject.Find(LevelUpItemGroupPath);
            if (levelUpItemGroup == null) { Assert.Fail($"Level Up Item Group not found on attempt {attempts}."); yield break; }

            Button[] allUpgradeButtons = levelUpItemGroup.GetComponentsInChildren<Button>(true);
            List<Button> availableOptions = allUpgradeButtons.Where(btn => btn.gameObject.activeInHierarchy && btn.interactable).ToList();
            if (availableOptions.Count == 0) { Assert.Fail($"No upgrade options available on attempt {attempts}."); yield break; }

            Button gearChoice = null;
            foreach (var btn in availableOptions)
            {
                Item itemScript = btn.GetComponent<Item>();
                // --- FIX IS HERE ---
                if (itemScript != null && itemScript.data != null && IsGear(itemScript.data)) // Pass the ItemData object
                {
                    gearChoice = btn;
                    break;
                }
            }

            if (gearChoice != null)
            {
                Item chosenItemScript = gearChoice.GetComponent<Item>();
                string chosenItemName = (chosenItemScript?.data != null) ? chosenItemScript.data.itemName : "Unknown Gear";
                Debug.Log($"Choosing GEAR option: {chosenItemName}");
                gearChoice.onClick.Invoke();
            }
            else
            {
                Button fallbackChoice = availableOptions[0];
                Item chosenItemScript = fallbackChoice.GetComponent<Item>();
                string chosenItemName = (chosenItemScript?.data != null) ? chosenItemScript.data.itemName : "Unknown Item";
                Debug.Log($"No gear offered, choosing fallback: {chosenItemName}");
                fallbackChoice.onClick.Invoke();
            }

            yield return WaitUntilInactive(LevelUpPanelPath, 5f);
        }

        Assert.AreEqual(targetGearCount, Item.ListGear.Count, $"Did not reach {targetGearCount} gears after {attempts} attempts. Found {Item.ListGear.Count}.");
        Debug.Log($"Reached {Item.ListGear.Count} gears after {attempts} level up attempts.");
        Debug.Log("✅ Test_04_LevelUp_Until_6_Gears Passed!");
    }
}


    // --- Include Helpers Here ---
    // PerformSetupAndReachGameplay()
    // WaitUntilActive()
    // WaitUntilInactive()
    // IsWeapon()
    // IsGear()
    // ...

 // End of ItemUpgradeTest class