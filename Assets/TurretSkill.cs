using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurretSkill : MonoBehaviour
{
    public Transform barrelTransform;  
    public float shootForce = 10f;
    public float fireRate = 0.5f;
    private bool canShoot = true;
    private Scanner scanner;
    private bool isShooting = false;
    private float numberOfBullet = 10;

    public Animator anim;
    public Transform[] spawnPoint;

    private float timeCooldown;
    private float timer;

    public float bulletSpeed = 20f;

    public void Init(float shootForce, float fireRate, float timeCooldown)
    {
        this.shootForce = shootForce;
        this.fireRate = fireRate;
        this.timeCooldown = timeCooldown;
    }


    void Start()
    {
        scanner = GetComponent<Scanner>();  // Scanner để tìm mục tiêu
        spawnPoint = GetChildPositions(gameObject);
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeCooldown) {
            timer = 0;
            gameObject.SetActive(false);
            if (!AnyTurretActive())
            {
                GameManager.instance.player.GetComponent<Player>().EffectSkill.SetActive(false);
            }
        }

        if (scanner.nearestTarget != null)
        {          
            RotateBarrelTowardsTarget(scanner.nearestTarget.position);
            Vector3 direction = scanner.nearestTarget.position - barrelTransform.position;
            // Kiểm tra có thể bắn không và bắn nếu được
            if (canShoot && !isShooting)
            {
                StartCoroutine(ShootBullet(direction));
            }
        }
        else
        {
            anim.SetBool("Shooting", false);
        }
    }
    bool AnyTurretActive()
    {
        // Kiểm tra xem còn bất kỳ turret nào đang hoạt động không
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("TuretSkill");
        foreach (GameObject turret in turrets)
        {
            if (turret.activeSelf)
            {
                return true;
            }
        }
        return false;
    }
    public void OnEnable()
    {
        timer = 0;
        canShoot = true;
        isShooting = false;
    }

    Transform[] GetChildPositions(GameObject parentObject)
    {
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>();
        return childTransforms;
    }

    void RotateBarrelTowardsTarget(Vector3 targetPosition)
    {       
        Vector3 direction = targetPosition - barrelTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;  
        barrelTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));  
    }

    public float timeBetweenShots = 0.2f;
    public float restTime = 2f;
    IEnumerator ShootBullet(Vector3 dir)
    {
        canShoot = false;
        isShooting = true;
        anim.SetBool("Shooting", true);
        Vector3 normalizedDirection = dir.normalized;

        for (int i = 0; i < numberOfBullet; i++)
        {
            GameObject bullet1 = GameManager.instance.pool.Get(30);
            bullet1.transform.position = spawnPoint[2].position;
            GameObject bullet2 = GameManager.instance.pool.Get(30);
            bullet2.transform.position = spawnPoint[3].position;

            
            Vector3 firingDirection = barrelTransform.right;

            bullet1.GetComponent<Bullet>().Init(30, 5, firingDirection, 5,0.33f);
            bullet2.GetComponent<Bullet>().Init(30, 5, firingDirection, 5,0.33f);

            yield return new WaitForSeconds(timeBetweenShots + Random.Range(-0.1f, 0.1f));
        }
        anim.SetBool("Shooting", false);
        yield return new WaitForSeconds(restTime);
        isShooting = false;
        canShoot = true;
    }
}
