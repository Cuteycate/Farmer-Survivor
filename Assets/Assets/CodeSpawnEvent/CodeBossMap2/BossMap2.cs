using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossMap2 : MonoBehaviour
{

    [Header("# Heal Boss")]
    public float health = 1000;
    public float maxHealth = 1000;
    public float expOnDefeat = 50f;

    [Header("# State")]
    public bool isImmune = false; // Boss miễn sát thương khi đang sử dụng skill

    public float moveInterval = 5f; 
    public float moveSpeed = 5f;
    public bool isLeft = false;

    [Header("# Object ")]
    public GameObject projectilePrefab; 
    public GameObject turretProjectilePrefab;
    public GameObject groundPrefab; // Prefab for ground effect
    public GameObject thunderPrefab;
    public GameObject shootPrefacb;
    public Slider HealthBar;

    [Header("# Skill 1")]
    public int projectileCount = 10; 
    public float spawnInterval = 0.2f; 
    public float angleIncrement = 15f;
    public float projectileSpeed = 5f; 
    public float projectileLifetime = 5f;

    [Header("# Skill 2")]
    public float waveAmplitude = 4f; 
    public float waveFrequency = 1f;
    public float shootInterval = 0.2f;

    [Header("# Skill 4")]
    public float projectileCount4 = 10f;

    [Header("# Component")]
    private Vector3 targetPosition;
    private Camera mainCamera;
    SpriteRenderer spriter;
    Rigidbody2D rigid;
    BoxCollider2D coll;
    public Transform player;
    private Animator anim;

    [Header("# CoolTime Skill")]
    public float timeStart = 20f;
    public bool isUseSkill1 = false;
    public bool isUseSkill2 = false;
    public bool isUseSkill3 = false;
    public bool isUseSkill4 = false;
    private bool isUsingSkill = false;  
    public float skill1Cooldown = 10f;
    public float skill2Cooldown = 15f; 
    public float skill3Cooldown = 20f;
    public float skill4Cooldown = 25f;
    private float skill1Timer = 0f;
    private float skill2Timer = 0f;
    private float skill3Timer = 0f;
    private float skill4Timer = 0f;

    [Header("# Cutscene")]
    public string cutsceneSceneName = "Cutscene"; 
    public string gameSceneName = "MainGame"; 

   //private bool cutsceneTriggered = false;

    private void OnEnable()
    {
        /*if (!cutsceneTriggered)
        {
            cutsceneTriggered = true;
            StartCoroutine(PlayCutsceneAndResumeGame());
       }*/

        player = GameManager.instance.player.transform;
        //coll.enabled = true;
        //rigid.simulated = true;
        //spriter.sortingOrder = 2;
        health = maxHealth;
        HealthBar.value = health;
        HealthBar.transform.localScale = new Vector3(1f, 5f, 1f); // Gấp đôi chiều ngang, giữ nguyên chiều dọc

    }

    void Start()
    {       
        mainCamera = Camera.main;
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        player = GameManager.instance.player.transform;
        HealthBar.value = health;
        anim = GetComponent<Animator>();
        StartCoroutine(MoveBossRandomly());
    }

    IEnumerator PlayCutsceneAndResumeGame()
    {
        // Pause the game
        Time.timeScale = 0;

        // Load the cutscene scene additively
        AsyncOperation loadCutscene = SceneManager.LoadSceneAsync(cutsceneSceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadCutscene.isDone);

        // Wait until the cutscene is finished
        // You can signal cutscene completion using any mechanism like a button press, animation event, etc.
        yield return StartCoroutine(WaitForCutsceneCompletion());

        // Unload the cutscene scene
        AsyncOperation unloadCutscene = SceneManager.UnloadSceneAsync(cutsceneSceneName);
        yield return new WaitUntil(() => unloadCutscene.isDone);

        // Resume the game
        Time.timeScale = 1;
    }

    IEnumerator WaitForCutsceneCompletion()
    {
        // Example: Wait for 5 seconds or until a specific condition is met
        float elapsedTime = 0f;
        float cutsceneDuration = 5f; // Adjust this to match your cutscene duration
        while (elapsedTime < cutsceneDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time because the game is paused
            yield return null;
        }

    }

    void Update()
    {
        // Countdown cooldowns
        skill1Timer -= Time.deltaTime;
        skill2Timer -= Time.deltaTime;
        skill3Timer -= Time.deltaTime;
        skill4Timer -= Time.deltaTime;
        timeStart -= Time.deltaTime;
        if ( timeStart <= 0)
        {
            switch (HealthBar.value)
            {
                case > 0.5f:
                    isUseSkill1 = true;
                    isUseSkill2 = true;
                    break;
                case < 0.5f:
                    isUseSkill3 = true;
                    isUseSkill4 = true;
                    break;
            }
        }     
        // Check and trigger skills
        if (isUseSkill1 && skill1Timer <= 0 && !isUsingSkill && IsInCameraView())
        {
            StartCoroutine(ShootSkill());
        }
        else if (isUseSkill2 && skill2Timer <= 0 && !isUsingSkill && IsInCameraView())
        {
            StartCoroutine(PerformWaveSkill());
        }
        else if (isUseSkill3 && skill3Timer <= 0 && !isUsingSkill && IsInCameraView())
        {
            StartCoroutine(ShootTurretProjectiles());
        }
        else if (isUseSkill4 && skill4Timer <= 0 && !isUsingSkill && IsInCameraView())
        {
            StartCoroutine(SummonLightningStrikes());
        }

        // Flip sprite based on player position
        spriter.flipX = player.position.x < rigid.position.x;  isLeft = spriter.flipX;
        
    }

    bool IsInCameraView()
    {
       
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    IEnumerator MoveBossRandomly()
    {
        while (true)
        {         
            yield return new WaitUntil(() => !isUsingSkill);
        
            yield return new WaitForSeconds(moveInterval);
        
            targetPosition = GetRandomPositionInCamera();

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f && !isUsingSkill)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    Vector3 GetRandomPositionInCamera()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float randomX = Random.Range(mainCamera.transform.position.x - cameraWidth / 2, mainCamera.transform.position.x + cameraWidth / 2);
        float randomY = Random.Range(mainCamera.transform.position.y - cameraHeight / 2, mainCamera.transform.position.y + cameraHeight / 2);

        return new Vector3(randomX, randomY, transform.position.z);
    }

    void SpawnProjectile(float angle)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
        Destroy(projectile, projectileLifetime);
        projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    IEnumerator ShootSkill()
    {
        isImmune = true;
        isUsingSkill = true;
        skill1Timer = skill1Cooldown;

        float[] baseAngles = { 0f, 120f, 240f }; 

        for (int i = 0; i < projectileCount; i++)
        {
            foreach (float baseAngle in baseAngles)
            {
                float currentAngle = baseAngle + (i * angleIncrement);
                SpawnProjectile(currentAngle);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
       
        isUsingSkill = false;
        isImmune = false;
    }

    IEnumerator ShootWavePattern()
    {
        float elapsedTime = 0f;

        while (elapsedTime < projectileLifetime)
        {
            transform.position = new Vector3(transform.position.x, player.position.y, transform.position.z);

            float waveOffset = Mathf.Sin(elapsedTime * waveFrequency) * waveAmplitude;
            Vector3 spawnPosition = transform.position + new Vector3(0, waveOffset, 0);

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            GameObject shoot;
            Vector2 direction;
            float rotationAngle;
            Rigidbody2D rb;
            switch (isLeft)
            {
                case true:
                    shoot = Instantiate(shootPrefacb, spawnPosition, Quaternion.Euler(0, 0, 135));
                    direction = Vector2.left;
                    rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    projectile.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
                    projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                    rb = projectile.GetComponent<Rigidbody2D>();
                    rb.velocity = direction * projectileSpeed;

                    break;
                case false:
                    shoot = Instantiate(shootPrefacb, spawnPosition, Quaternion.Euler(0, 0, -45));
                    direction = Vector2.right;
                    rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    projectile.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
                    projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                    rb = projectile.GetComponent<Rigidbody2D>();
                    rb.velocity = direction * projectileSpeed;

                    break;

            }

            Destroy(projectile, projectileLifetime);
            Destroy(shoot, 2f);

            elapsedTime += shootInterval;
            yield return new WaitForSeconds(shootInterval);
        }
    }

    IEnumerator PerformWaveSkill()
    {
        isUsingSkill = true;
        isImmune = true;
        skill2Timer = skill2Cooldown; 

        Vector3 leftPosition = new Vector3(
            mainCamera.ViewportToWorldPoint(new Vector3(0.1f, 0, 0)).x,
            player.position.y,
            transform.position.z
        );

        while (Vector3.Distance(transform.position, leftPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, leftPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return StartCoroutine(ShootWavePattern());

        isUsingSkill = false;
        isImmune = true;
    }

    IEnumerator ShootTurretProjectiles()
    {
        isUsingSkill = true;
        isImmune = true;
        skill3Timer = skill3Cooldown;

        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPosition = GetRandomPositionInCamera();
            GameObject turretProjectile = Instantiate(turretProjectilePrefab, transform.position, Quaternion.identity);
             StartCoroutine(MoveProjectileToPosition(turretProjectile, randomPosition));
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);  isUsingSkill = false;
        isImmune = false;
    }

    IEnumerator MoveProjectileToPosition(GameObject projectile, Vector3 targetPosition)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        while (Vector3.Distance(projectile.transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - projectile.transform.position).normalized;
            rb.velocity = direction * projectileSpeed;
            yield return null;
        }
        rb.velocity = Vector2.zero;  
        projectile.GetComponent<TurretProjectile>().StartFiring();
    }

    IEnumerator SummonLightningStrikes()
    {
        isUsingSkill = true;
        isImmune = true;
        skill4Timer = skill4Cooldown;

        int strikeCount = 0; 
        int strikesPerRound = 5; 

        while (strikeCount < projectileCount4)
        {
            List<Vector3> strikePositions = new List<Vector3>();

            
            for (int i = 0; i < strikesPerRound; i++)
            {
                strikePositions.Add(GetRandomPositionInCamera());
            }

            
            foreach (Vector3 position in strikePositions)
            {
                GameObject groundEffect = Instantiate(groundPrefab, position, Quaternion.identity);
                Destroy(groundEffect, 1f); 
            }

            yield return new WaitForSeconds(0.5f); 

           
            foreach (Vector3 position in strikePositions)
            {
                GameObject thunderEffect = Instantiate(thunderPrefab, position, Quaternion.identity);
                Destroy(thunderEffect, 1.5f); 
            }

            strikeCount += 1; 
            yield return new WaitForSeconds(0.25f); 
        }

        isUsingSkill = false;
        isImmune = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameManager.instance.FinalBossStillAlive || isImmune)
            return;
        if (collision.CompareTag("Bullet"))
        {
            // Nếu là Bullet, giảm máu của Enemy
            health -= collision.GetComponent<Bullet>().damage;
            //ShowDamage(collision.GetComponent<Bullet>().damage.ToString());

            if (health > 0)
            {
                anim.SetTrigger("Hit");
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
                HealthBar.value = health / maxHealth;
            }
        }
        else if (collision.CompareTag("BulletSpecial"))
        {
            health -= collision.GetComponent<BulletSpecial>().damage;
            //ShowDamage(collision.GetComponent<BulletSpecial>().damage.ToString());

            if (health > 0)
            {
                anim.SetTrigger("Hit");
                //AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
                HealthBar.value = health / maxHealth;
            }
        }
        //if (health > 0)
        //{
        //    anim.SetTrigger("Hit");
        //    AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);         
        //    HealthBar.value = health / maxHealth;            
        //}
        if (health <= 0)
        {          
            HealthBar.value = health / maxHealth;
            GameManager.instance.FinalBossStillAlive = false;
            HealthBar.transform.localScale = new Vector3(0f, 0f, 0f); // Gấp đôi chiều ngang, giữ nguyên chiều dọc
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.enabled = false;
            gameObject.SetActive(false);
            //anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            //GameManager.instance.GetExp(this);
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Bossdead);
        }

    }
}
