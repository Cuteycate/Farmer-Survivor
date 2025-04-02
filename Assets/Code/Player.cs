using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    private Coroutine healthRecoveryCoroutine;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    public bool useMouseToAim = false;
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    public GameObject EffectSkill;
    //weapon Scythe test
    [HideInInspector]
    public float lastHorizontalVector;
    [HideInInspector]
    public float lastVerticalVector;

    //Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashinhTime = 0.2f;
    private float dashingCooldown = 7f;

    [SerializeField] private Slider skillcooldown;
    [SerializeField] private TrailRenderer tr;

    //DoubleWeapon
    [HideInInspector]
    public List<Weapon> weapons = Item.ListWeapon;
    private bool canDW = true;
    [HideInInspector]
    public bool isDWing;
    private float DWtime = 5f;
    private float DWCooldown = 30f;

    //Knife
    [HideInInspector]
    private bool canKnife = true;
    private bool isKnife = false;
    private float knifeCooldown = 25f;
    private float numberOfKnives = 50f;
    private float speedKnife = 1.5f;  
    private float[] Rotaion = {20, 40, 60, 80, 100, 120, 140, 160, 180};
    private float numberOfWave = 4;
    private float damageOfKnife = 30;

    //spawn 
    [HideInInspector]
    private bool canSpawn = true;
    private bool isSpawning = false;
    private float numberOfSolider = 5;
    private float spawnCooldown = 45f;
    private float timeToDestroy = 20f;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);

        if (skillcooldown != null && GameManager.instance.PlayerId == 0)
        {
            skillcooldown.maxValue = dashingCooldown;
            skillcooldown.value = dashingCooldown; // Slider starts full
        }
        if (skillcooldown != null && GameManager.instance.PlayerId == 1)
        {
            skillcooldown.maxValue = DWCooldown;
            skillcooldown.value = DWCooldown; // Slider starts full
        }
        if (skillcooldown != null & GameManager.instance.PlayerId == 2)
        {
            skillcooldown.maxValue = knifeCooldown;
            skillcooldown.value = knifeCooldown;
        }
        if (skillcooldown != null & GameManager.instance.PlayerId == 3)
        {
            skillcooldown.maxValue = spawnCooldown;
            skillcooldown.value = spawnCooldown;
        }
    }
    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.PlayerId];
    }
    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        if (isDashing)
        {
            return;
        }
        if (Gamepad.current != null)
        {
            inputVec = Gamepad.current.leftStick.ReadValue();
        }
        else
        {
            // Fallback for keyboard controls
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
        }

        if (inputVec.x != 0)
        {
            inputVec = inputVec.normalized;
            lastHorizontalVector = inputVec.x;
            lastVerticalVector = inputVec.y;
        }
        if ((Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Space))
        {
            if(canDash && GameManager.instance.PlayerId == 0){
                EffectSkill.SetActive(true);
                StartCoroutine(Dash());
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dash);
            }
        }

        // Double Weapon Skill (Gamepad Button South or Spacebar)
        if ((Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Space))
        {
            if (canDW && GameManager.instance.PlayerId == 1)
            {
                EffectSkill.SetActive(true);
                StartCoroutine(DoubleCountWeapon());
            }
        }
        
        if (Input.GetMouseButtonDown(1)) // chuốt trái
        {
            useMouseToAim = !useMouseToAim; // Toggle
        }

        if ((Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Space))
        {
            if (canKnife && GameManager.instance.PlayerId == 2)
            {
                EffectSkill.SetActive(true);
                StartCoroutine(Knife());
            }
        }

        if ((Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Space))
        {
            if (canSpawn && GameManager.instance.PlayerId == 3)
            {
                EffectSkill.SetActive(true);
                StartCoroutine(SpawnSolider());
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.Space) && canKnife && GameManager.instance.PlayerId == 2)
        {
            EffectSkill.SetActive(true);
            StartCoroutine(Knife());             
        }
        if (Input.GetKeyDown(KeyCode.Space) && canSpawn && GameManager.instance.PlayerId == 3)
        {
            EffectSkill.SetActive(true);
            StartCoroutine(SpawnSolider());         
        }    */  
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (isDashing)
        {
            return;
        }
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

    }
    void OnMove(InputValue value)
    {
        if (!GameManager.instance.isLive)
            return;
        inputVec = value.Get<Vector2>();
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (isDashing)
        {
            return;
        }
        anim.SetFloat("Speed", inputVec.magnitude);
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;
        CapsuleCollider2D playerCollider = GetComponent<CapsuleCollider2D>();
        if (collision.otherCollider != playerCollider)
            return;
        if (collision.gameObject.CompareTag("Turet"))
        {
            return;
        }
        if (collision.gameObject.CompareTag("TuretSkill"))
        {
            return;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            return;
        }
        if (collision.gameObject.CompareTag("Prop"))
        {
            return;
        }
        GameManager.instance.Health -= Time.deltaTime * 10;

        if (GameManager.instance.Health < 0)
        {
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameManager.instance.isLive)
            return;
        if (GameManager.instance.Health < 0)
        {
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }


    IEnumerator KnockBack()
    {
        /*   yield return null; //khựng 1 frame
           yield return new WaitForSeconds(2f); //coroutine 2s hết 2s mới tiếp tục bị knockback bới return null */
        yield return 1f;
        Vector3 playerPos = scanner.transform.position;
        Vector3 dirVec = transform.position - playerPos; // khoảng cách enemy - khoảng cách nhân vật
        rigid.AddForce(dirVec.normalized * 20, ForceMode2D.Impulse); //Truyền knockback ngược về so với PlayerPos
    }


    public void StartHealthRecovery(float gearRecoveryRate)
    {
        float shopRecoveryRate = ShopStats.Instance.healthrecoveryMultiplier;
        if (healthRecoveryCoroutine != null)
        {
            StopCoroutine(healthRecoveryCoroutine);
        }
        healthRecoveryCoroutine = StartCoroutine(HealthRecoveryCoroutine(gearRecoveryRate, shopRecoveryRate));
    }
    public void StopHealthRecovery()
    {
        if (healthRecoveryCoroutine != null)
        {
            StopCoroutine(healthRecoveryCoroutine);
            healthRecoveryCoroutine = null;
        }
    }
    private IEnumerator HealthRecoveryCoroutine(float gearRecoveryRate, float shopRecoveryRate)
    {
        while (true)
        {
            GameManager gameManager = GameManager.instance;
            if (gameManager != null && gameManager.Health < gameManager.MaxHealth)
            {
                // Tinh tong tu gear + shop
                float totalRecoveryRate = gearRecoveryRate + shopRecoveryRate;
                gameManager.Health = Mathf.Min(gameManager.Health + totalRecoveryRate, gameManager.MaxHealth);
            }
            // Moi 1 giay thi hoi nhu totalrecoveryrate mau.
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0;
        Vector2 dashDirection = inputVec.normalized;
        rigid.velocity = dashDirection * dashingPower;
        tr.emitting = true;
        // Chờ hết thời gian Dash
        yield return new WaitForSeconds(dashinhTime);
        tr.emitting = false;
        // Kết thúc Dash
        rigid.gravityScale = originalGravity;
        rigid.velocity = Vector2.zero; // Dừng lại sau Dash
        isDashing = false;
        EffectSkill.SetActive(false);
        // Chờ thời gian hồi chiêu trước khi cho phép Dash lần tiếp theo
        //Hien thi thoi gian cho tren slider
        float cooldownTimer = 0;
        while (cooldownTimer < dashingCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (skillcooldown != null)
            {
                skillcooldown.value = cooldownTimer;
            }
            yield return null;
        }

        if (skillcooldown != null)
        {
            skillcooldown.value = dashingCooldown;
        }
        canDash = true;
    }
    private IEnumerator DoubleCountWeapon()
    {
        canDW = false;
        isDWing = true;
        foreach (Weapon weapon in weapons)
        {
            if (weapon.id == 0)
                weapon.BroadcastMessage("Batch", SendMessageOptions.DontRequireReceiver);
            else
            {
                weapon.count = weapon.CountStatic * 2;
                weapon.ExtraCount =weapon.ExtraCountStatic * 2;
            }
        }
        // Kết thúc x2 weapon
        skillcooldown.value = 0;
        yield return new WaitForSeconds(DWtime);
        // Dừng lại sau x2 weapon
        isDWing = false;
        foreach (Weapon weapon in weapons)
        {
            if (weapon.id == 0)
                weapon.BroadcastMessage("Batch", SendMessageOptions.DontRequireReceiver);
            else
            {
                weapon.count = weapon.CountStatic;
                weapon.ExtraCount = weapon.ExtraCountStatic;
            }
        }
        EffectSkill.SetActive(false);
        // Chờ thời gian hồi chiêu trước khi cho phép Dash lần tiếp theo
        //Hien thi thoi gian cho tren slider
        float cooldownTimer = 0;
        while (cooldownTimer < DWCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (skillcooldown != null)
            {
                skillcooldown.value = cooldownTimer;
            }
            yield return null;
        }

        if (skillcooldown != null)
        {
            skillcooldown.value = DWCooldown;
        }
        canDW = true;
    }

    Transform[] GetChildPositions(GameObject parentObject)
    {
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>();
        return childTransforms;
    }

    private IEnumerator Knife()
    {
        canKnife = false;
        isKnife = true;
        float knifeDelay = 0.5f;

        float levelMultiplier = 1 + (GameManager.instance.level * 0.1f); // 10% increase per level

        for (int i = 0; i < numberOfKnives / numberOfWave; i++)
        {
            for (int j = 0; j < numberOfWave; j++)
            {
                int yinYang = Random.Range(0, 2);
                int ran = Random.Range(0, Rotaion.Length);
                GameObject PointKnife = GameManager.instance.pool.Get(27);
                PointKnife.transform.position = new Vector3(GameManager.instance.player.transform.position.x + Random.Range(-5, 5),
                                                                GameManager.instance.player.transform.position.y + Random.Range(-5, 5),
                                                                0);
                Transform[] transformsknife = GetChildPositions(PointKnife);
                float rotationAngle = Rotaion[ran];
                if (yinYang == 1)
                {
                    rotationAngle = -rotationAngle;
                }
                PointKnife.transform.Rotate(0, 0, rotationAngle);
                Vector3 direction = (transformsknife[2].position - transformsknife[1].position).normalized;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GameObject KnifePrefab = GameManager.instance.pool.Get(26);
                KnifePrefab.transform.position = transformsknife[1].transform.position;

                KnifePrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90 + 180));

                Vector3 dir = transformsknife[2].position - transformsknife[1].position;

                KnifePrefab.GetComponent<BulletSpecial>().Init(damageOfKnife * levelMultiplier, speedKnife, rotationAngle, transformsknife[2], dir);

                AfterImageGenerator generator = KnifePrefab.GetComponent<AfterImageGenerator>();
                if (generator != null)
                {
                    generator.StartAfterImages(); 
                }
            }

            yield return new WaitForSeconds(knifeDelay);
        }
        isKnife = false;
        EffectSkill.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        float cooldownTimer = 0;
        while (cooldownTimer < knifeCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (skillcooldown != null)
            {
                skillcooldown.value = cooldownTimer;
            }
            yield return null;
        }

        if (skillcooldown != null)
        {
            skillcooldown.value = knifeCooldown;
        }

        canKnife = true;
    }

    private IEnumerator SpawnSolider()
    {
        canSpawn = false;
        isSpawning = true;
        Vector3 nextSpawnPosition;
        Vector3 direction;

        float levelMultiplier = 1 + (GameManager.instance.level * 0.1f);

        Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(GameManager.instance.player.transform.position.x + 4, GameManager.instance.player.transform.position.y, 0),
            new Vector3(GameManager.instance.player.transform.position.x, GameManager.instance.player.transform.position.y + 4, 0),
            new Vector3(GameManager.instance.player.transform.position.x - 4, GameManager.instance.player.transform.position.y, 0),
            new Vector3(GameManager.instance.player.transform.position.x, GameManager.instance.player.transform.position.y - 4, 0)
        };


        for (int i = 0; i < spawnPositions.Length; i++)
        {
            Vector3 middlePosition;
            switch (i)
            {
                case 0:
                    middlePosition = (spawnPositions[i] + spawnPositions[i + 1]) / 2f;
                    direction = spawnPositions[i + 1] - spawnPositions[i];
                    break;
                case 1:
                    middlePosition = (spawnPositions[i] + spawnPositions[i + 1]) / 2f;
                    direction = spawnPositions[i + 1] - spawnPositions[i];
                    break;
                case 2:
                    middlePosition = (spawnPositions[i] + spawnPositions[i + 1]) / 2f;
                    direction = spawnPositions[i + 1] - spawnPositions[i];
                    break;
                case 3:
                    middlePosition = (spawnPositions[i] + spawnPositions[0]) / 2f;
                    direction = spawnPositions[0] - spawnPositions[i];
                    break;
                default:
                    middlePosition = (spawnPositions[i] + spawnPositions[0]) / 2f;
                    direction = spawnPositions[0] - spawnPositions[i];
                    break;
            }

            if (middlePosition != null || direction != null)
            {
                // Tạo object tại điểm trung gian
                GameObject rotatingObject = GameManager.instance.pool.Get(32);
                rotatingObject.transform.position = middlePosition;
               
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;  // Tính góc xoay

                // Xoay object theo hướng từ spawnPositions[i] đến spawnPositions[i+1]
                rotatingObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            }


            GameObject SmokeEffect = GameManager.instance.pool.Get(31);
            SmokeEffect.transform.position = new Vector3(spawnPositions[i].x, spawnPositions[i].y + 2, 0);
            yield return new WaitForSeconds(0.5f);

            GameObject TurretSkill = GameManager.instance.pool.Get(29);
            TurretSkill.transform.position = spawnPositions[i];

            TurretSkill.GetComponent<TurretSkill>().Init(10f * levelMultiplier, 0.5f, timeToDestroy);
        }

        isSpawning = false;
        float cooldownTimer = 0;
        while (cooldownTimer < spawnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (skillcooldown != null)
            {
                skillcooldown.value = cooldownTimer;
            }
            yield return null;
        }
        if (skillcooldown != null)
        {
            skillcooldown.value = spawnCooldown;
        }

        canSpawn = true;
    }


}
