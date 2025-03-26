using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public int CountStatic;
    public int penetration;
    public float speed;
    public float size;
    public float hitCooldown;
    public int ExtraCount;
    public int ExtraCountStatic;
    float timer;
    private bool isBatchEnabled = false;
    public float MeleeCoolDown = 3f;
    private static List<float> baseCoolDowns = new List<float>();
    public float baseCoolDown;
    public GameObject hiteffect;
    private Coroutine batchCoroutine;
    Player player;
    public Sprite Icon;
    public int level = 1;
    public int maxlevel;

    public float spiralSpeed = 5f;
    void Awake()
    {
        player = GameManager.instance.player;
    }
    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime); // Cho vũ khí xoay ngược chiều kim đồng hồ
                break;
            case 1:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    StartCoroutine(FireCoroutine()); // Bắt đầu bắn
                }
                break;

            case 8:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;  // Bắt đầu bắn
                    FireShotgun();
                }
                break;
            case 9:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;  // Bắt đầu bắn
                    StartCoroutine(SniperFireCoroutine());
                }
                break;
            case 10:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    MeleeAttack();
                }
                break;
            case 11:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    StartCoroutine(LightningAttack());
                }
                break;
            case 13:
                transform.Rotate(Vector3.forward * 360 * Time.deltaTime);
                break;
            case 14:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
            default:
                break;

        }
    }
    public void Init(ItemData data)
    {
        //Basic Set
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;
        //Property Set
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        baseCoolDown = data.baseCoolDown;
        // Base Count = Count nguyên bản - ExtraCount = Count từ Gear ProjectileMultiplier = Count từ Shop
        count = data.baseCount + Character.Count + ExtraCount + ShopStats.Instance.projectileMultiplier;
        CountStatic = data.baseCount + Character.Count + ExtraCount + ShopStats.Instance.projectileMultiplier;
        penetration = data.basePenetration;
        size = data.baseSize;
        hitCooldown = data.HitCooldown;
        //Su dung de tinh CoolDown Cua vu khi va luu tru vao baseCoolDowns
        float calculatedSpeed = 0f;
        //hiteffect
        hiteffect = data.HitEffect;
        Icon = data.itemIcon;
        maxlevel = data.damages.Length;
        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (data.projectiles == GameManager.instance.pool.prefabs[i])
            {
                prefabId = i;
                break;
            }
        }
        switch (id)
        {
            case 0:
                speed = 150 * Character.WeaponSpeed;
                MeleeCoolDown = MeleeCoolDown * Character.WeaponRate;
                calculatedSpeed = MeleeCoolDown;
                Batch();
                batchCoroutine = StartCoroutine(ToggleBatchCoroutine());
                break;
            case 1:
            case 8:
            case 9:
            case 10:
            case 11:
            case 14:
                speed = baseCoolDown * Character.WeaponRate;
                calculatedSpeed = speed;
                break;
            case 13:
                speed = baseCoolDown * Character.WeaponRate;
                calculatedSpeed = speed;
                StartCoroutine(ToggleBatchCoroutineAxe());
                break;
            default:
                break;
        }
        if (id >= baseCoolDowns.Count)
        {
            baseCoolDowns.AddRange(new float[id - baseCoolDowns.Count + 1]);
        }
        baseCoolDowns[id] = calculatedSpeed;
        if ((int)data.itemType == 0 || (int)data.itemType == 1)
        {
            Hand hand = player.hands[(int)data.itemType];
            hand.spriter.sprite = data.hand;
            hand.gameObject.SetActive(true);
        }
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }
    public void LevelUp(float damage, int count, int penetration, float size)
    {
        level++;
        switch (id)
        {
            case 14:
                this.damage += damage;
                this.count += count;
                this.CountStatic += count;
                this.penetration += penetration;
                this.size = size;
                speed = baseCoolDown * Character.WeaponRate * (1f - size);
                break;
            case 13:
                this.damage += damage;
                this.count += count;
                this.CountStatic += count;
                this.penetration += penetration;
                this.size += size;
                break;
            default:
                this.damage += damage;
                this.count += count;
                this.CountStatic += count;
                this.penetration += penetration;
                break;
        }
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
        if (id == 0)
            Batch();
    }
    IEnumerator ToggleBatchCoroutine()
    {
        while (true)
        {
            float cooldownDuration = isBatchEnabled ? 3f : MeleeCoolDown;

            yield return new WaitForSeconds(cooldownDuration);

            if (isBatchEnabled)
            {
                DisableBatch();
            }
            else
            {
                EnableBatch();
            }
        }
    }

    void EnableBatch()
    {
        isBatchEnabled = true;
        // Enable all children
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    void DisableBatch()
    {
        isBatchEnabled = false;
        // Disable all children
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    void Batch()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        bool checkDW = player.isDWing;
        count = count + ExtraCount;
        if (checkDW)
        {
            count *= 2;
        }
        if (count != transform.childCount && checkDW == false)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                GameManager.instance.pool.ReturnToPool(child.gameObject);
            }
        }


        for (int i = 0; i < count; i++)
        {
            Transform bullet;
            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            Vector3 rotVec = Vector3.forward * 360 * i / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 3f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, penetration, Vector3.zero, count, hitCooldown);
        }

        if (checkDW)
        {
            count /= 2;
        }

        count = count - ExtraCount;
    }
    //Code cho Súng tiểu liên ID 1
    IEnumerator FireCoroutine()
    {
        int initialCount = ExtraCount;
        int totalCount = count + initialCount;

        for (int i = 0; i < totalCount; i++)
        {
            Vector3 dir;
            float angle;

            if (player.useMouseToAim)
            {
                // nhấm bằng vị trí chuột
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dir = mousePos - transform.position;
                dir.z = 0; 
                dir = dir.normalized;

                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            else
            {
                // tự nhấm
                if (!player.scanner.nearestTarget)
                {
                    yield break;
                }

                Vector3 targetPos = player.scanner.nearestTarget.position;
                dir = targetPos - transform.position;
                dir = dir.normalized;

                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;

            // Thêm rotation
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

            // bắt đầu bullet
            bullet.GetComponent<Bullet>().Init(damage, penetration, dir, i, hitCooldown);

            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
            yield return new WaitForSeconds(0.1f);
        }
    }


    //Knife code below
    void Fire()
    {
        int initialCount = ExtraCount; // Số lượng súng bổ sung
        int totalCount = count + initialCount; // Tổng số súng
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
        // Lấy tất cả mục tiêu có thể từ bộ quét
        RaycastHit2D[] hitResults = GameManager.instance.player.GetComponent<Scanner>().targets;
        List<Transform> targetTransforms = new List<Transform>();
        foreach (RaycastHit2D hitResult in hitResults)
        {
            if (hitResult.transform != null)
            {
                targetTransforms.Add(hitResult.transform); // Thêm mục tiêu vào danh sách
            }
        }
        // Chuyển đổi danh sách mục tiêu thành mảng để dễ truy cập
        Transform[] targetEnemies = targetTransforms.ToArray();
        // Nếu không có mục tiêu, bắn theo hướng ngẫu nhiên
        if (targetEnemies.Length == 0)
        {
            for (int i = 0; i < totalCount; i++)
            {
                // Bắn theo hướng ngẫu nhiên
                Vector3 dir = Random.insideUnitCircle.normalized;
                Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.position = transform.position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                bullet.GetComponent<Bullet>().Init(damage, penetration, dir, i, hitCooldown);
            }
            return; // Exit if no targets are found
        }
        // Sắp xếp mục tiêu ngẫu nhiên để đảm bảo không có mục tiêu trùng lặp
        List<int> targetIndices = new List<int>();
        for (int i = 0; i < targetEnemies.Length; i++)
        {
            targetIndices.Add(i);
        }
        ShuffleList(targetIndices); // Xáo trộn danh sách mục tiêu
        // Lặp qua tất cả "súng" (dựa trên số lượng totalCount)
        for (int i = 0; i < totalCount; i++)
        {
            if (i < targetEnemies.Length)
            {
                // Chọn mục tiêu duy nhất từ danh sách đã xáo trộn
                int randomIndex = targetIndices[i];
                Vector3 targetPos = targetEnemies[randomIndex].position;
                Vector3 dir = (targetPos - transform.position).normalized;
                // Bắn đạn vào mục tiêu
                Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.position = transform.position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                bullet.GetComponent<Bullet>().Init(damage, penetration, dir, i, hitCooldown);
            }
            else
            {
                // Nếu không đủ mục tiêu, bắn theo hướng ngẫu nhiên
                Vector3 dir = Random.insideUnitCircle.normalized;
                Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.position = transform.position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                bullet.GetComponent<Bullet>().Init(damage, penetration, dir, i, hitCooldown);
            }
        }
    }
    // Phương thức hỗ trợ để xáo trộn danh sách
    void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Chọn chỉ mục ngẫu nhiên
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    // Knife Code



    //Shotgun code ID 8
    void FireShotgun()
    {
        int initialCount = ExtraCount;
        int totalCount = count + initialCount;
        Vector3 dirToTarget;
        if (player.useMouseToAim)
        {
            // ban su dung chuot
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dirToTarget = (mousePos - transform.position).normalized;
            dirToTarget.z = 0; // Ensure 2D direction
            dirToTarget = dirToTarget.normalized;
        }
        else
        {
            dirToTarget = new Vector3(player.lastHorizontalVector, player.lastVerticalVector, 0).normalized;
            if (dirToTarget == Vector3.zero)
            {
                dirToTarget = Vector3.right;
            }
        }

        for (int i = 0; i < totalCount; i++)
        {
            float angle = (i - (totalCount - 1) / 2f) * 10f;
            Vector3 spreadDir = Quaternion.Euler(0, 0, angle) * dirToTarget;
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, spreadDir);
            bullet.GetComponent<Bullet>().Init(damage, penetration, spreadDir, i, hitCooldown);
        }
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }

    //Sniper rifle code ID 9
    IEnumerator SniperFireCoroutine()
    {
        int initialCount = ExtraCount;
        int totalCount = count + initialCount;

        for (int i = 0; i < totalCount; i++)
        {
            Vector3 dir;

            if (player.useMouseToAim)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dir = (mousePos - transform.position).normalized;
                dir.z = 0; 
                dir = dir.normalized;
            }
            else
            {
                if (!player.scanner.farthestTarget)
                {
                    yield break;
                }

                Vector3 targetPos = player.scanner.farthestTarget.position;
                dir = (targetPos - transform.position).normalized;
                dir = dir.normalized;
            }
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            bullet.GetComponent<Bullet>().Init(damage, penetration, dir, i, hitCooldown);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
            yield return new WaitForSeconds(0.5f);
        }
    }

    //Melee Attack code ID 10 (Scythe)
    void MeleeAttack()
    {
        float temp = 1f + (float)count * 0.2f;

        if (!player.scanner.nearestTarget)
            return;

        Vector3 direction;

        if (player.useMouseToAim)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = (mousePosition - transform.position);
            direction.z = 0;
            direction = direction.normalized;
        }
        else
        {
            Vector3 movementDirection = new Vector3(player.inputVec.x, player.inputVec.y, 0);
            if (movementDirection == Vector3.zero)
            {
                movementDirection = new Vector3(player.lastHorizontalVector, player.lastVerticalVector, 0);
            }
            direction = movementDirection.normalized;
            if (direction == Vector3.zero)
            {
                direction = Vector3.right;
            }
        }
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;

        bullet.localScale = new Vector3(temp, temp, 1f);
        bullet.rotation = rotation;
        bullet.Translate(bullet.right * 3f, Space.World);
        bullet.GetComponent<Bullet>().Init(damage, penetration, direction, 1, hitCooldown);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }




    HashSet<Transform> enemiesHit = new HashSet<Transform>(); // HashSet to store enemies hit by lightning

    //Lightning code ID 11
    IEnumerator LightningAttack()
    {
        int initialCount = ExtraCount;
        int TempCount = count + initialCount;

        // Find the nearest enemies to the player
        RaycastHit2D[] hitResults = GameManager.instance.player.GetComponent<Scanner>().targets;

        List<Transform> targetTransforms = new List<Transform>();

        foreach (RaycastHit2D hitResult in hitResults)
        {
            if (hitResult.transform != null)
            {
                targetTransforms.Add(hitResult.transform);
            }
        }

        // Convert to array for easier indexing
        Transform[] targetEnemies = targetTransforms.ToArray();

        // Check if there are any enemies found
        if (targetEnemies.Length > 0 && targetEnemies != null)
        {
            // Define the lightning bolt effect prefab
            GameObject lightningBoltEffectPrefab = hiteffect;

            // Define the damage and range of the lightning attack
            float damage = this.damage;
            float radius = this.penetration;

            // Limit the count to the number of enemies if count exceeds the number of enemies
            int temp = Mathf.Min(TempCount, targetEnemies.Length);

            // List to keep track of already targeted enemies
            List<int> targetedIndices = new List<int>();

            // Loop through the count and spawn lightning bolts on each enemy
            for (int i = 0; i < temp; i++)
            {
                int randomIndex;

                // Find a unique random enemy index that has not been targeted yet
                do
                {
                    randomIndex = Random.Range(0, targetEnemies.Length);
                } while (targetedIndices.Contains(randomIndex));

                // Add this index to the list of targeted indices
                targetedIndices.Add(randomIndex);
                if (targetEnemies[randomIndex] != null)
                {
                    // Instantiate the lightning bolt effect at the position of the target enemy
                    GameObject lightningBolt = Instantiate(lightningBoltEffectPrefab, targetEnemies[randomIndex].position, Quaternion.identity);

                    // Perform an area attack around the lightning bolt's position
                    AreaAttack(lightningBolt.transform.position, radius, damage);
                    AudioManager.instance.PlaySfx(AudioManager.Sfx.Lightning);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        else
        {
            // If no enemy is found, apply cooldown or wait before attempting another attack
            StartCoroutine(CooldownCoroutine());
        }
    }

    void AreaAttack(Vector3 center, float radius, float damage)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                // Get the enemy component and deal damage to it
                Enemy enemy = collider.GetComponent<Enemy>();
                BossEnemy bossEnemy = collider.GetComponent<BossEnemy>();
                FinalBoss finalBoss = collider.GetComponent<FinalBoss>();
                EnemyEvent enemyEvent = collider.GetComponent<EnemyEvent>();

                if (enemy != null)
                {
                    if (enemy.isLive)
                    {
                        enemy.TakeDamage(damage);
                    }

                }
                else if (bossEnemy != null)
                {
                    if (bossEnemy.isLive)
                        bossEnemy.TakeDamage(damage);

                } else if (finalBoss != null)
                {
                    finalBoss.TakeDamage(damage);
                }
                else if (enemyEvent != null)
                {
                    if (enemyEvent.isLive)
                        enemyEvent.TakeDamage(damage);

                }
            }
            if (collider.CompareTag("Prop"))
            {
                PropBreak prop = collider.GetComponent<PropBreak>();
                if (prop != null)
                {
                    prop.TakeDamage(damage);
                }
            }
        }


    }



    IEnumerator CooldownCoroutine()
    {
        // Apply a cooldown or wait before attempting another attack
        yield return new WaitForSeconds(this.speed); // Adjust the cooldown duration as needed

        // Xoa Ke thu bi danh de LightNing hoat dong tot
        enemiesHit.Clear();

        // After cooldown, retry the lightning attack
        LightningAttack();
    }
    //Lay basecooldown tu danh sach vu khi dua vao weaponId
    public static float GetBaseCoolDown(int weaponId)
    {
        return weaponId < baseCoolDowns.Count ? baseCoolDowns[weaponId] : 0f;
    }
    /// LIGHTNING CODE ABOVE

    // AXE WEAPON -------
    private IEnumerator FireAxesCoroutine()
    {
        int initialCount = ExtraCount;
        int totalCount = count + initialCount;

        for (int i = 0; i < totalCount; i++)
        {
            // Tạo prefab trong pool
            Transform axe = GameManager.instance.pool.Get(prefabId).transform;

            // Đặt rìu ở ngay người chơi
            axe.parent = null;
            axe.position = transform.position; // Bắt đầu ở người chơi
            axe.localScale = Vector3.one * size; // Chỉnh sửa độ lớn
            axe.rotation = Quaternion.identity;

            // Tính toán
            float angle = (360f / totalCount) * i;
            Vector3 initialDirection = new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * angle),
                Mathf.Sin(Mathf.Deg2Rad * angle),
                0f
            ).normalized;

            // Khởi tạo rìu
            axe.GetComponent<Bullet>().Init(damage, penetration, initialDirection, i, hitCooldown);

            // Bắt đầu Coroutine cho rìu quay
            StartCoroutine(SpiralMovementSingleAxe(axe));
            AfterImageGenerator generator = axe.GetComponent<AfterImageGenerator>();
            if (generator != null)
            {
                generator.StartAfterImages(); // bắt đầu afterimage
            }
            // tắt rìu sau 4s
            StartCoroutine(DeactivateBulletAfterTime(axe, 4f));

            // bắn mỗi 0.5s
            AudioManager.instance.PlaySfx(AudioManager.Sfx.AxeSquirl);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator DeactivateBulletAfterTime(Transform axe, float time)
    {
        yield return new WaitForSeconds(time);
        if (axe != null && axe.gameObject.activeSelf)
        {
            axe.gameObject.SetActive(false); // Deactivate the axe
        }
        AfterImageGenerator generator = axe.GetComponent<AfterImageGenerator>();
        if (generator != null)
        {
            generator.StopAfterImages(); // dừng afterimage
        }
    }

    private IEnumerator ToggleBatchCoroutineAxe()
    {
        while (true)
        {
            yield return StartCoroutine(FireAxesCoroutine()); // Fire the axes
            yield return new WaitForSeconds(speed); // Cooldown before the next batch
        }
    }

    private IEnumerator SpiralMovementSingleAxe(Transform axe)
    {
        float angleOffset = 90f; // bắt đầu của rìu
        float radius = 0f; // Starting radius
        float radiusGrowth = 1.5f; // Fixed growth for the spiral radius
        Vector3 originPosition = transform.position; // Starting position of the player

        while (axe.gameObject.activeSelf) // Continue until the axe deactivates
        {
            // Update radius dynamically over time
            radius += radiusGrowth * Time.deltaTime;

            // Calculate the angle for the current frame
            float angle = angleOffset + (Time.time * spiralSpeed);

            // Calculate the offset based on the spiral
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            // Update the axe position relative to the origin position
            axe.position = originPosition + offset;

            // Optionally rotate the axe
            axe.Rotate(Vector3.back * 360 * Time.deltaTime);

            // Wait for the next frame
            yield return null;
        }
    }


}
