using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxgameTime = 30f;
    [Header("# Player Info")]
    public int PlayerId;
    public bool FinalBossStillAlive = true;


    public float Health;
    public float MaxHealth = 100;
    public int level;
    public int kill;
    public float exp;
    public int gold;
    public int totalGold;
    public float[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
    public float ExtraRateExp=0;
    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public TreasureChest treasureChest;
    public Result uiResult;
    public GameObject enemyCleaner;
    public GameObject ExpPickUpPrefab;

    public int mapid;
    void Awake()
    {
        instance = this;
        totalGold = PlayerPrefs.GetInt("TotalGold", 0);
    }
    public void GameStart(int id)
    {
        Item.ListGear.Clear();
        Item.ListWeapon.Clear();
        ShopStats.Instance.LoadStats();
        PlayerId = id;
        MaxHealth *= ShopStats.Instance.maxhealthMultiplier; // Lay multiplier to ShopStats roi tinh toan
        Health = MaxHealth; // Health = maxHealth
        player.gameObject.SetActive(true); // Set player object true de bat dau
        instance.player.StartHealthRecovery(0); // Bat dau HealthRecovery nhung khong co gear (trong truong hop nay gear = 0)
        uiLevelUp.Select(PlayerId % 2); //
        Resume();
        AudioManager.instance.PlayOpening(false);
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);

    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine()
    {
        isLive = false;
        // Truyền vàng vào Total vàng rồi lưu lại qua PlayerPrefs
        totalGold += gold;
        PlayerPrefs.SetInt("TotalGold", totalGold);
        PlayerPrefs.Save();
        yield return new WaitForSeconds(0.5f);
        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();
        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }
    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }
    IEnumerator GameVictoryRoutine()
    {
        enemyCleaner.SetActive(true);
        isLive = false;
        // Truyền vàng vào Total vàng rồi lưu lại qua PlayerPrefs
        totalGold += gold;
        PlayerPrefs.SetInt("TotalGold", totalGold);
        PlayerPrefs.Save();
        AudioManager.instance.PlayBgm(false);
        yield return new WaitForSeconds(4f);
        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }
    public void GameRetry()
    {
        AudioManager.instance.PlayOpening(true);
        Item.ResetItems();
        SceneManager.LoadScene(0);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }
    public void Quit()
    {
        Application.Quit();
    }
    void Update()
    {
        if (!isLive)
            return;
        gameTime += Time.deltaTime;
        if (gameTime > maxgameTime && !FinalBossStillAlive)
        {
            GameVictory();
        }
    }
    public void GetExp(Enemy enemy)
    {
        if (!isLive) return;
        float chance = UnityEngine.Random.Range(1, 91);
        if (chance == 1)
        {
            SpawnGold(enemy.transform.position);
        }
        // Spawn ra hat XP duoi vi tri ma Enemy dead
        SpawnExpPickUp(enemy.transform.position, enemy.expOnDefeat);
    }
    void SpawnExpPickUp(Vector3 position, float expAmount)
    {
        GameObject expPickUp = pool.Get(18);
        expPickUp.transform.position = position;
        ExpPickUp expPickUpComponent = expPickUp.GetComponent<ExpPickUp>();
        if (expPickUpComponent != null)
        {
            expPickUpComponent.ResetState(expAmount);
            expPickUpComponent.CheckForNearbyMerges();
            StartCoroutine(AnimateExpPickUpMovement(expPickUp, position));
        }
    }

    private IEnumerator AnimateExpPickUpMovement(GameObject expPickUp, Vector3 originPosition)
    {
        if (!expPickUp.activeSelf) yield break;

        ExpPickUp expScript = expPickUp.GetComponent<ExpPickUp>();
        if (expScript == null) yield break;

        float dropDistance = 1f;
        float dropDuration = 0.4f;
        Vector3 dropPosition = originPosition + new Vector3(
            UnityEngine.Random.Range(-0.1f, 0.1f),
            -UnityEngine.Random.Range(0.1f, dropDistance),
            0);

        float elapsedTime = 0f;
        while (elapsedTime < dropDuration && expPickUp.activeSelf)
        {
            if (expScript.isBeingPulled) yield break; // Stop if being pulled
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dropDuration;
            expPickUp.transform.position = Vector3.Lerp(originPosition, dropPosition, progress);
            yield return null;
        }

        expPickUp.transform.position = dropPosition;

        float floatOffsetY = 0.2f;
        float floatDuration = 2f;
        Vector3 upPosition = dropPosition + new Vector3(0, floatOffsetY, 0);

        while (expPickUp.activeSelf)
        {
            elapsedTime = 0f;
            while (elapsedTime < floatDuration / 2 && expPickUp.activeSelf)
            {
                if (expScript.isBeingPulled) yield break; // Stop if being pulled
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (floatDuration / 2);
                expPickUp.transform.position = Vector3.Lerp(dropPosition, upPosition, progress);
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < floatDuration / 2 && expPickUp.activeSelf)
            {
                if (expScript.isBeingPulled) yield break; // Stop if being pulled
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (floatDuration / 2);
                expPickUp.transform.position = Vector3.Lerp(upPosition, dropPosition, progress);
                yield return null;
            }
        }
    }



    public void GetExp(EnemyEvent enemy)
    {
        if (!isLive) return;

        // Spawn ra hat XP duoi vi tri ma Enemy dead
        float chance = UnityEngine.Random.Range(1, 91);
        if (chance == 1)
        {
            SpawnGold(enemy.transform.position);
        }
        SpawnExpPickUp(enemy.transform.position, enemy.expOnDefeat);
    }
    public void GetExp(BossEnemy enemy)
    {
        if (!isLive) return;
        float chance = UnityEngine.Random.Range(1, 91);
        if (chance == 1)
        {
            SpawnGold(enemy.transform.position);
        }
        // Spawn ra hat XP duoi vi tri ma Enemy dead
        SpawnExpPickUp(enemy.transform.position, enemy.expOnDefeat);
    }
    void SpawnGold(Vector3 position)
    {
        GameObject goldPrefab = instance.pool.Get(25);

        if (goldPrefab != null)
        {
            GameObject goldPickup = Instantiate(goldPrefab, position, Quaternion.identity);
        }
    }
    public void ResHealth(float amount)
    {
        if(Health < MaxHealth)
        {
            Health += amount;
            if(Health >= MaxHealth)
            {
                Health = MaxHealth;
            }
        }
      
    }
    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }
    public void PlaySelect()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }
    
    public void setmap(int id)
    {
        mapid = id;
    }
    public void levelupbybuttonfortest()
    {
        level++;
        GameManager.instance.uiLevelUp.Show();
    }
}
