using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;

    public GameObject warningiconL;
    public GameObject warningiconR;
    public GameObject warningiconT;
    public GameObject warningiconB;

    [Header("# Note")]
    public string Event1 = "1 là Event Chạy";
    public string Event2 = "2 là Event Vòng tròn";
    public string Event3 = "3 là Event Ép";
    public string Event4 = "4 là Event Chạy xen kẽ Trái và Phải";
    public string Event5 = "5 là Event Cắt ";
    public string Event6 = "6 là Event Chừa đường(Phai)";
    public string Event7 = "7 là Event Chừa đường(Trai)";
    public string Event8 = "8 là Event Ep 2 Huong";
    public string Event9 = "9 là Event triệu hồi có vòng tròn";
    public string Event10 = "10 là Event Chạy xen kẽ Trên và Xuống";

    public SpawnData[] spawnData;
    public GameObject PointWave;
    private bool checkBossSpawn = false;
    private Camera mainCamera;

    private float nextTime;
    public float timeDeafult = 2f;

    

    public int mapId;

    float[] Rotation = { 0, 45, 90, 135, 180, 225, 270, 315, 160 };
    float[] RotantionEventPlus = { 0, 90 };

    public int level;
    float timer;
    
    void Awake()
    {
        mainCamera = Camera.main;
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

        if (level == spawnData.Length - 1 && !checkBossSpawn)
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
                
                switch (waveDetail.TypeEvent)
                {
                    case 2:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        warningiconR.SetActive(true);
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        break;
                    case 8:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        warningiconR.SetActive(true);
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        break;
                    case 9:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        warningiconR.SetActive(true);
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        break;
                    case 6:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconR.SetActive(true);
                        break;                     
                    case 7:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        break;
                    case 4:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        warningiconR.SetActive(true);
                        break;
                    case 10:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        break;
                    default:
                        AudioManager.instance.PlaySfx(AudioManager.Sfx.warning);
                        warningiconL.SetActive(true);
                        warningiconR.SetActive(true);
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        break;
                }
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(SpawnSingleEvent(waveDetail.TypeEvent));
                yield return new WaitForSeconds(nextTime);
            }
        }
    }

    IEnumerator SpawnSingleEvent(int TypeEvent)
    {
        if(TypeEvent != 5)
        {
            nextTime = timeDeafult;
        }
        else
        {
            nextTime = 0.3f;
        }
    
        if (TypeEvent == 4 || TypeEvent == 9 || TypeEvent == 10)
        {
            nextTime = 0.7f;
        }

        switch (TypeEvent)
        {
            //1 là event chạy băng qua 
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
            // 2 là event vong tron
            case 2:
                GameObject waveType2 = GameManager.instance.pool.Get(10);
                waveType2.transform.position = GameManager.instance.player.transform.position;
                waveType2.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                break;

            //3 là event event ép 
            case 3:
                
                int randomRotation = Random.Range(0, RotantionEventPlus.Length);
                Transform position = GameManager.instance.player.transform;

                for (int i = -25; i < 26; i += 2)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(15);

                    if (RotantionEventPlus[randomRotation] == 0)
                    {
                        warningiconL.SetActive(true);                        
                        warningiconR.SetActive(true);
                        waveType3.transform.position = new Vector3(position.position.x, position.position.y - i, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 0);
                    }
                    else
                    {
                        warningiconT.SetActive(true);
                        warningiconB.SetActive(true);
                        waveType3.transform.position = new Vector3(position.position.x - i, position.position.y, 0);
                        waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                        waveType3.transform.Rotate(0, 0, 90);
                    }

                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, RotantionEventPlus[randomRotation] != 0);
                }
                break;

            //4 là event chạy xen kẽ
            case 4:
                Transform positionEvent5 = GameManager.instance.player.transform;
                for (float i = -20; i < 21; i += 4f)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(17);
                    waveType3.transform.position = new Vector3(positionEvent5.position.x, positionEvent5.position.y - i, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 0);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                }
                break;


            //5 là event đâm thẳng có đường báo
            case 5:

                int randomCount = Random.Range(2, 5);
                for (int i = 0; i < randomCount; i++)
                {
                    int random = Random.Range(0, Rotation.Length);
                    Transform positionEvent6 = GameManager.instance.player.transform;
                    GameObject waveType5 = GameManager.instance.pool.Get(23);
                    Transform[] childTransformss = GetChildPositions(waveType5);
                    waveType5.transform.position = GameManager.instance.player.transform.position;
                    waveType5.transform.Rotate(0, 0, Rotation[random]);
                    waveType5.GetComponent<EventWave>().Inti(childTransformss[1].transform, childTransformss[2].transform, TypeEvent, false);
                }
                break;

            //6 là event zich zac
            case 6:
                Transform Position = GameManager.instance.player.transform;
                int check = 1;
                int count = 0;
                int randomd;
                if (GameManager.instance.mapid == 1)
                {
                     randomd = Random.Range(-2, 3);
                }
                else if (GameManager.instance.mapid == 2)
                {
                     randomd = 0;
                }
                else
                {
                    randomd = (int) Position.position.y;
                }
                
                int caseEV = Random.Range(0,3);
                switch (caseEV)
                {
                    case 0:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(33);
                            waveType6.transform.position = new Vector3(Position.position.x + j, randomd + check, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            if (count < 4)
                            {
                                check++;
                            }
                            else
                            {
                                check--;
                            }
                            if (count > 8)
                            {
                                count = 0;
                            }

                            count++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                    case 1:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(33);
                            waveType6.transform.position = new Vector3(Position.position.x + j, randomd + check, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            check++;
                            count++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                    case 2:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(33);
                            waveType6.transform.position = new Vector3(Position.position.x + j, randomd + check, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            check--;
                            count++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                }
                break;

                case 7:
                Transform Position7 = GameManager.instance.player.transform;
                int check7 = 1;
                int count7 = 0;
                int randomd7;
                if (GameManager.instance.mapid == 1)
                {
                    randomd7 = Random.Range(-2, 3);
                }
                else if (GameManager.instance.mapid == 2)
                {
                    randomd7 = 0;
                }
                else
                {
                    randomd7 = (int)Position7.position.y;
                }
                int caseEV7 = Random.Range(0, 3);
                switch (caseEV7)
                {
                    case 0:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(34);
                            waveType6.transform.position = new Vector3( Position7.position.x - j, randomd7 + check7, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            if (count7 < 4)
                            {
                                check7++;
                            }
                            else
                            {
                                check7--;
                            }
                            if (count7 > 8)
                            {
                                count7 = 0;
                            }

                            count7++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                    case 1:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(34);
                            waveType6.transform.position = new Vector3( Position7.position.x - j, randomd7 + check7, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            check7++;
                            count7++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                    case 2:
                        for (int j = 0; j < 31; j += 2)
                        {
                            GameObject waveType6 = GameManager.instance.pool.Get(34);
                            waveType6.transform.position = new Vector3(Position7.position.x - j, randomd7 + check7, 0);
                            waveType6.GetComponent<EventWave>().Inti(null, null, TypeEvent, false);
                            check7--;
                            count7++;
                            yield return new WaitForSeconds(0.07f);
                        }
                        break;
                }
                break;

            case 8:
                int randomRotation8 = Random.Range(0, RotantionEventPlus.Length);
                Transform position8 = GameManager.instance.player.transform;

                for (int i = -20; i < 21; i += 2)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(15);
                    // First rotation scenario (rotation 0)
                    waveType3.transform.position = new Vector3(position8.position.x, position8.position.y - i, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 0);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, 3, false);

                    // Second rotation scenario (rotation 90)
                    waveType3 = GameManager.instance.pool.Get(15); // Get a new instance for the second scenario
                    waveType3.transform.position = new Vector3(position8.position.x - i, position8.position.y, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 90);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, 3, true);
                }

                break;
            case 9:
                //int numberOfSummon = 5;

                //for (int i = 0; i < numberOfSummon; i++)
                //{
                //    GameObject CircleLine = GameManager.instance.pool.Get(35);
                //    CircleLine.transform.position = GetRandomPositionInCamera();
                //    yield return new WaitForSeconds(0.5f);
                //    GameObject Lightnight = GameManager.instance.pool.Get(36);
                //    Lightnight.transform.position = new Vector3(CircleLine.transform.position.x, CircleLine.transform.position.y + 3.5f, 0);
                //    yield return new WaitForSeconds(0.5f);
                //    GameObject enemy = GameManager.instance.pool.Get(0); 
                //    enemy.transform.position = CircleLine.transform.position; 
                //    enemy.GetComponent<Enemy>().Init(spawnData[level], false); 
                //}
                int numberOfSummon = 10;

                // Chạy các đối tượng spawn đồng thời
                for (int i = 0; i < numberOfSummon; i++)
                {
                    StartCoroutine(SpawnEntity(i)); // Gọi coroutine để spawn từng đối tượng
                }
                break;

            case 10:
                Transform positionEvent10 = GameManager.instance.player.transform;
                for (float i = -20; i < 21; i += 4f)
                {
                    GameObject waveType3 = GameManager.instance.pool.Get(17);
                    waveType3.transform.position = new Vector3(positionEvent10.position.x - i, positionEvent10.position.y, 0);
                    waveType3.transform.rotation = Quaternion.identity; // Reset rotation
                    waveType3.transform.Rotate(0, 0, 90);
                    waveType3.GetComponent<EventWave>().Inti(GameManager.instance.player.transform, null, TypeEvent, false);
                }
                break;

        }
        yield return null;
    }

    IEnumerator SpawnEntity(int i)
    {
        GameObject CircleLine = GameManager.instance.pool.Get(35);
        CircleLine.transform.position = GetRandomPositionInCamera();
        yield return new WaitForSeconds(0.5f);
        GameObject Lightnight = GameManager.instance.pool.Get(36);
        Lightnight.transform.position = new Vector3(CircleLine.transform.position.x, CircleLine.transform.position.y + 3.5f, 0);
        yield return new WaitForSeconds(0.5f);
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = CircleLine.transform.position;
        enemy.GetComponent<Enemy>().Init(spawnData[level], false);
    }

    bool IsInCameraView()
    {

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    Vector3 GetRandomPositionInCamera()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float randomX = Random.Range(mainCamera.transform.position.x - cameraWidth / 2, mainCamera.transform.position.x + cameraWidth / 2);
        float randomY = Random.Range(mainCamera.transform.position.y - cameraHeight / 2, mainCamera.transform.position.y + cameraHeight / 2);

        return new Vector3(randomX, randomY, transform.position.z);
    }

    private void SpawnFinalBoss()
    {
        int bossIndex = mapId == 1 ? 12 : 21; // Select boss index based on mapId
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
}
