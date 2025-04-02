using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public GameObject PointWave;
    private bool checkBossSpawn = false;

    public int mapId;

    float[] Rotation = { 0, 45, 90, 135, 180, 225, 270, 315, 160 };
    float[] RotantionEventPlus = { 0, 90 };

    public int level;
    float timer;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
        mapId = GameManager.instance.mapid;
        Debug.Log(mapId);
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 30f), spawnData.Length - 1);

        if (level == 1/*spawnData.Length*/ - 1 && !checkBossSpawn)
        {
            SpawnFinalBoss();
        }

        if (level < spawnData.Length)
        {
            if (timer > spawnData[level].spawnTime)
            {
                timer = 0;
                Spawn();

                if (spawnData[level].EventWave)
                {
                    StartCoroutine(SpawnEvent(spawnData[level].EventWaveDetails));
                    spawnData[level].EventWave = false;
                }
            }
        }
    }

    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0); // Default enemy index
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position; // Random spawn point
        enemy.GetComponent<Enemy>().Init(spawnData[level], false); // Initialize enemy with spawn data
    }

    IEnumerator SpawnEvent(List<EventWaveDetails> eventWaveDetails)
    {
        foreach (var waveDetail in eventWaveDetails)
        {
            for (int i = 0; i < waveDetail.Count; i++)
            {
                yield return StartCoroutine(SpawnSingleEvent(waveDetail.TypeEvent));
                yield return new WaitForSeconds(3f);
            }
        }
    }

    IEnumerator SpawnSingleEvent(int TypeEvent)
    {
        switch (TypeEvent)
        {
            case 1:
                int ran = Random.Range(0, Rotation.Length);
                GameObject Wave = GameManager.instance.pool.Get(8);
                GameObject PointWave = GameManager.instance.pool.Get(11);

                PointWave.transform.position = GameManager.instance.player.transform.position;
                PointWave.transform.Rotate(0, 0, Rotation[ran]);

                Transform[] childTransforms = GetChildPositions(PointWave);
                Wave.transform.position = childTransforms[1].position;
                Wave.GetComponent<EventWave>().Inti(childTransforms[2], childTransforms[1], TypeEvent, false);
                break;

            case 2:
                GameObject waveType2 = GameManager.instance.pool.Get(10);
                waveType2.transform.position = GameManager.instance.player.transform.position;
                waveType2.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                break;

            case 3:
                int randomRotation = Random.Range(0, RotantionEventPlus.Length);
                Transform position = GameManager.instance.player.transform;

                for (int i = -50; i < 51; i += 2)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(15);

                    if (RotantionEventPlus[randomRotation] == 0)
                    {
                        waveType3.transform.position = new Vector3(position.position.x, position.position.y - i, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 0);
                    }
                    else
                    {
                        waveType3.transform.position = new Vector3(position.position.x - i, position.position.y, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 90);
                    }

                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, RotantionEventPlus[randomRotation] != 0);
                }
                break;

            case 4:
                Transform positionEvent5 = GameManager.instance.player.transform;
                for (float i = -50; i < 51; i += 4f)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(17);
                    waveType3.transform.position = new Vector3(positionEvent5.position.x, positionEvent5.position.y - i, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 0);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                }
                break;
        }

        yield return null;
    }

    private void SpawnFinalBoss()
    {
        int bossIndex = mapId == 1 ? 12 : 19; // Select boss index based on mapId
        GameObject FinalBoss = GameManager.instance.pool.Get(bossIndex);

        FinalBoss.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position; // Spawn at a random spawn point
        checkBossSpawn = true;
    }

    Transform[] GetChildPositions(GameObject parentObject)
    {
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>();
        return childTransforms;
    }

    [System.Serializable]
    public class SpawnData
    {
        public float spawnTime;
        public int spriteType;
        public int health;
        public float speed;
        public int expOnDefeat;
        public bool EventWave;
        public List<EventWaveDetails> EventWaveDetails; // Details for each type of EventWave
    }

    [System.Serializable]
    public class EventWaveDetails
    {
        public int TypeEvent;
        public int Count; // Number of EventWaves of this type
    }
    /*
    IEnumerator SpawnEvent(float time, int level, int TypeEvent)
    {
        switch (TypeEvent)
        {
            case 1:
                for (int i = 0; i < spawnData[level].CountEvent; i++)
                {
                    int ran = Random.Range(0, Rotation.Length);
                    GameObject Wave = GameManager.instance.pool.Get(8);
                    GameObject PointWave = GameManager.instance.pool.Get(11);

                    PointWave.transform.position = GameManager.instance.player.transform.position;
                    PointWave.transform.Rotate(0, 0, Rotation[ran]);

                    Transform[] childTransforms = GetChildPositions(PointWave);
                    Wave.transform.position = childTransforms[1].position;
                    Wave.GetComponent<EventWave>().Inti(childTransforms[2], childTransforms[1], TypeEvent, false);

                    yield return new WaitForSeconds(7f);
                }
                break;

            case 2:
                GameObject waveType2 = GameManager.instance.pool.Get(10);
                waveType2.transform.position = GameManager.instance.player.transform.position;
                waveType2.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                break;

            case 3:
                int randomRotation = Random.Range(0, RotantionEventPlus.Length);
                Transform position = GameManager.instance.player.transform;

                for (int i = -50; i < 51; i += 2)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(15);

                    if (RotantionEventPlus[randomRotation] == 0)
                    {
                        waveType3.transform.position = new Vector3(position.position.x, position.position.y - i, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 0);
                    }
                    else
                    {
                        waveType3.transform.position = new Vector3(position.position.x - i, position.position.y, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 90);
                    }

                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, RotantionEventPlus[randomRotation] != 0);
                }
                break;
            case 4:
                Transform positionEvent5 = GameManager.instance.player.transform;
                for (float i = -50; i < 51; i += 4f)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(17);  
                    waveType3.transform.position = new Vector3(positionEvent5.position.x, positionEvent5.position.y - i, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 0);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false );
                }
                break;
        }

        yield return new WaitForSeconds(3f);
    }

    */
}
