using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public int level;
    float timer;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        timer += Time.deltaTime; // Timer tính giờ
        int currentLevel = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 60f), spawnData.Length - 1);
        if (currentLevel != level)
        {
            level = currentLevel;
            timer = 0f;
        }
        if (!spawnData[level].bossSpawned && timer > spawnData[level].spawnTime)
        {
            // Đánh dấu là đã spawn
            spawnData[level].bossSpawned = true;
            // Spawn boss
            SpawnBoss();
        }
    }
    void SpawnBoss()
    {
        GameObject boss = GameManager.instance.pool.Get(7); // Chỉ có 1 boss trong pool nên đổi luôn thành 1
        boss.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position; // Spawn random theo 14 điểm spawnPoint trong SampleScene
        boss.GetComponent<BossEnemy>().Init(spawnData[level], false); // Lấy dữ liệu đầu vào từ Enemy
    }

    [System.Serializable]
    public class SpawnData
    {
        public float spawnTime; //thời gian spawn 
        public int spriteType; // sprite của nhân vật được truyền trong enemy (kiểm tra prefab của enemy coi phần anim controller)
        public int health; // Máu của quái
        public float speed; //tốc độ quái
        public int expOnDefeat;
        public bool bossSpawned; // Whether a boss has been spawned for this level
        public SpawnData()
        {
            bossSpawned = false;
        }
    }
}
