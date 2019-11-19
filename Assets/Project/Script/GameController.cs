using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [SerializeField] private bool classifyMode;
    [SerializeField] private bool testMode;

    [SerializeField] private FirstPersonController player;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> decelerationPoints = new List<Transform>();
    [SerializeField] private List<Animator> emergencyLightAnimators = new List<Animator>();
    [SerializeField] private GameObject core;
    [SerializeField] private GameObject laserFence;
    [SerializeField] private GameObject smallEnemy;
    [SerializeField] private GameObject mediumEnemy;
    [SerializeField] private GameObject largeEnemy;
    [SerializeField] private GameObject emergencyAudioPrefab;
    [SerializeField] private float increaseCoreHpPerSec = 20f;
    [SerializeField] private float decreaseCoreHpPerSec = 10f;
    [SerializeField] private float laserFenceDuration = 7f;
    [SerializeField] private int laserFenceAvailable = 2;
    [SerializeField] private int smallEnemyDamage = 10;
    [SerializeField] private int mediumEnemyDamage = 15;
    [SerializeField] private int largeEnemyDamage = 20;
    [SerializeField] private int smallEnemyScore = 100;
    [SerializeField] private int mediumEnemyScore = 150;
    [SerializeField] private int largeEnemyScore = 200;
    [SerializeField] private Image coreHpBar;
    [SerializeField] private Image laserFenceCooldownIcon;
    [SerializeField] private Text waveTimerText;
    [SerializeField] private Text laserFenceRemainText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text congratText;
    [SerializeField] private Text oneLineText;
    [SerializeField] private Text twoLineText;
    [SerializeField] private Text countdownText;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Canvas gameCompletedCanvas;
    [SerializeField] private GameObject pauseAudioPrefab;
    [SerializeField] private GameObject decreaseCoreHpAudioPrefab;
    [SerializeField] private GameObject finishedDecreaseCoreHpAudioPrefab;
    [SerializeField] private GameObject coreDamagedAudioPrefab;
    [SerializeField] private GameObject laserFenceActivatedAudioPrefab;
    [SerializeField] private GameObject laserFenceHummingAudioPrefab;

    private static string currentState;
    private static float coreHp;
    private static float coreFullHp;

    public static bool pause;
    public static bool gameOver;
    public static float defaultTimeScale;

    private bool stateStarted;
    private float currentStateDuration;
    private float previousSpawnTime;
    private int consecutiveSpawn;
    private int enemyOnScreen;

    private float start_duration;
    private float waiting_duration;
    private float wave_duration;
    private bool waiting_alreadyBeginFixing;

    private float wave1_minSpawnTime;
    private float wave1_maxSpawnTime;
    private float wave1_duration;
    private int wave1_smallEnemySpawnChance;
    private int wave1_mediumEnemySpawnChance;
    private int wave1_largeEnemySpawnChance;
    private int wave1_consecutiveSpawnLimit;

    private float wave2_minSpawnTime;
    private float wave2_maxSpawnTime;
    private float wave2_duration;
    private int wave2_smallEnemySpawnChance;
    private int wave2_mediumEnemySpawnChance;
    private int wave2_largeEnemySpawnChance;
    private int wave2_consecutiveSpawnLimit;

    private float wave3_minSpawnTime;
    private float wave3_maxSpawnTime;
    private float wave3_duration;
    private int wave3_smallEnemySpawnChance;
    private int wave3_mediumEnemySpawnChance;
    private int wave3_largeEnemySpawnChance;
    private int wave3_consecutiveSpawnLimit;

    private float waveCompleteStateDuration;
    private float congratStateDuration;
    private float removeCoreHpStateDuration;
    private float coreHpToDecrease;
    private int nextWave;
    
    private float emergencyCoreHp;
    private float laserFenceAvailableTime;
    private float coreAudioTimer;
    private float coreAudioTimeStamp;
    private float coreAudioFadeOutTimeStamp;
    private float coreAudioTimeInterval;
    private float currentTimeScale;
    private AudioSource coreAudioSource;
    private GameObject currentEmergencyAudioPrefab;
    private GameObject currentDecreaseCoreHpAudioPrefab;

    private int score;
    private int fixingCost;

    private List<float> spawnTimeLimits = new List<float>();
    private List<float> spawnTimers = new List<float>();

    private void Start()
    {
        pause = false;
        gameOver = false;
        LockCursor();
        defaultTimeScale = 1f;
        Time.timeScale = defaultTimeScale;

        currentState = "START";
        start_duration = 5.9f;
        waiting_duration = 10.9f;
        wave_duration = 7.9f;

        wave1_minSpawnTime = 8f;
        wave1_maxSpawnTime = 20f;
        wave1_smallEnemySpawnChance = 60;
        wave1_mediumEnemySpawnChance = 100;
        wave1_largeEnemySpawnChance = 110;
        wave1_consecutiveSpawnLimit = 1;

        wave2_minSpawnTime = 10f;
        wave2_maxSpawnTime = 22f;
        wave2_smallEnemySpawnChance = 40;
        wave2_mediumEnemySpawnChance = 90;
        wave2_largeEnemySpawnChance = 100;
        wave2_consecutiveSpawnLimit = 2;

        wave3_minSpawnTime = 8f;
        wave3_maxSpawnTime = 22f;
        wave3_smallEnemySpawnChance = 30;
        wave3_mediumEnemySpawnChance = 75;
        wave3_largeEnemySpawnChance = 100;
        wave3_consecutiveSpawnLimit = 2;

        waveCompleteStateDuration = 0.6f;
        congratStateDuration = 3f;
        removeCoreHpStateDuration = 5.25f;
        nextWave = 1;

        coreFullHp = 100f;
        coreHp = coreFullHp;
        emergencyCoreHp = coreFullHp * 0.25f;
        coreHpBar.fillAmount = 1f;
        coreAudioTimeStamp = 10f;
        coreAudioFadeOutTimeStamp = 10f;
        coreAudioTimeInterval = 2f;
        coreAudioSource = core.GetComponent<AudioSource>();

        score = 0;
        fixingCost = 100;
        
        oneLineText.gameObject.SetActive(true);
        twoLineText.gameObject.SetActive(true);
        congratText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(true);
        waveTimerText.text = "W0  0:00";
        laserFenceRemainText.text = "" + laserFenceAvailable;
        scoreText.text = "0";
        oneLineText.text = testMode ? "Prepare for the enemies..." : "Prepare for the first wave...";
        twoLineText.text = "";
        congratText.text = "";
        countdownText.text = "" + (int)start_duration;

        laserFence.SetActive(false);
        laserFenceCooldownIcon.gameObject.SetActive(false);

        hudCanvas.gameObject.SetActive(true);
        pauseCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
        gameCompletedCanvas.gameObject.SetActive(false);

        foreach (Transform point in spawnPoints)
        {
            spawnTimeLimits.Add(0f);
            spawnTimers.Add(0f);
        }
    }
    
    private void Update()
    {
        if (currentState != "GAME OVER" && currentState != "END")
        {
            if (Input.GetKeyDown(KeyCode.Escape) || ButtonController.IsResumePressed())
            {
                if (!pause)
                {
                    currentTimeScale = Time.timeScale;
                }
                pause = !pause;
                Time.timeScale = pause ? 0 : currentTimeScale;
                coreAudioSource.pitch = Time.timeScale;
                Instantiate(pauseAudioPrefab, Vector3.zero, Quaternion.identity);
                if (pause)
                {
                    UnlockCursor();
                    coreAudioSource.Pause();
                    player.TurnOnDOF();
                    hudCanvas.gameObject.SetActive(false);
                    pauseCanvas.gameObject.SetActive(true);
                    EyeTrackerController.TurnOffBlinkRecording();
                }
                else
                {
                    LockCursor();
                    coreAudioSource.UnPause();
                    player.TurnOffDOF();
                    hudCanvas.gameObject.SetActive(true);
                    pauseCanvas.gameObject.SetActive(false);
                    EyeTrackerController.TurnOnBlinkRecording();
                }
            }
        }
        if (pause)
        {
            return;
        }

        switch (currentState)
        {
            case "START":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = start_duration;
                    OutputUDP.OpenConnection();
                    if (classifyMode)
                    {
                        OutputUDP.SetClassifyingState(1);
                    }
                }
                currentStateDuration -= Time.deltaTime;
                countdownText.text = currentStateDuration < 0f ? "0" : "" + (int)currentStateDuration;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    StartEnemyWave();
                }
                break;
            case "WAVE1":
                if (!stateStarted)
                {
                    stateStarted = true;
                }
                if (ProgressEnemyWave())
                {
                    stateStarted = false;
                    currentState = coreHp == 0f ? "GAME OVER" : "WAVE COMPLETED";
                }
                break;
            case "WAVE2":
                if (!stateStarted)
                {
                    stateStarted = true;
                }
                if (ProgressEnemyWave())
                {
                    stateStarted = false;
                    currentState = coreHp == 0f ? "GAME OVER" : "WAVE COMPLETED";
                }
                break;
            case "WAVE3":
                if (!stateStarted)
                {
                    stateStarted = true;
                }
                if (ProgressEnemyWave())
                {
                    stateStarted = false;
                    currentState = coreHp == 0f ? "GAME OVER" : "END";
                }
                break;
            case "WAVE COMPLETED":
                if (!stateStarted)
                {
                    Time.timeScale = 0.15f;
                    stateStarted = true;
                    waiting_alreadyBeginFixing = false;
                    currentStateDuration = waveCompleteStateDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    Time.timeScale = defaultTimeScale;
                    stateStarted = false;
                    currentState = "CONGRAT";
                }
                break;
            case "CONGRAT":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = congratStateDuration;
                    congratText.text = "WAVE COMPLETED";
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    congratText.text = "";
                    currentState = "REMOVE CORE HP";
                }
                break;
            case "REMOVE CORE HP":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = removeCoreHpStateDuration;
                    coreHpToDecrease = coreHp - (coreHp * 0.3f);
                    currentDecreaseCoreHpAudioPrefab = Instantiate(decreaseCoreHpAudioPrefab, Vector3.zero, Quaternion.identity);
                    twoLineText.text = "We are reducing some core HP.\nFixing it will cost you " + fixingCost + " points.";
                }
                currentStateDuration -= Time.deltaTime;
                if (coreHp > coreHpToDecrease)
                {
                    if (coreHp > 0f)
                    {
                        float fps = 1 / Time.deltaTime;
                        coreHp -= (decreaseCoreHpPerSec / fps);
                        if (coreHp < 0f)
                        {
                            coreHp = 0f;
                        }
                        coreHpBar.fillAmount = coreHp / coreFullHp;
                    }
                }
                else if (coreHp <= coreHpToDecrease && currentDecreaseCoreHpAudioPrefab != null)
                {
                    currentDecreaseCoreHpAudioPrefab.GetComponent<AudioPrefabFadeOutScript>().BeginFadeOut();
                    currentDecreaseCoreHpAudioPrefab = null;
                    Instantiate(finishedDecreaseCoreHpAudioPrefab, Vector3.zero, Quaternion.identity);
                }
                if (currentStateDuration <= 0f && coreHp <= coreHpToDecrease)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "WAITING";
                }
                break;
            case "WAITING":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = waiting_duration;
                    player.StartSkipStimulus();
                    oneLineText.text = "Prepare for the next wave...";
                }
                currentStateDuration -= Time.deltaTime;
                countdownText.text = currentStateDuration < 0f ? "0" : "" + (int)currentStateDuration;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    player.StopSkipStimulus();
                    StartEnemyWave();
                }
                break;
            case "GAME OVER":
                if (!stateStarted)
                {
                    stateStarted = true;
                    gameOver = true;
                    Time.timeScale = 0f;
                    UnlockCursor();
                    player.TurnOnDOF();
                    coreAudioSource.Pause();
                    hudCanvas.gameObject.SetActive(false);
                    gameOverCanvas.gameObject.SetActive(true);
                    OutputUDP.SetClassifyingState(0);
                    OutputUDP.CloseConnection();
                }
                break;
            case "END":
                if (!stateStarted)
                {
                    stateStarted = true;
                    gameOver = true;
                    Time.timeScale = 0f;
                    UnlockCursor();
                    player.TurnOnDOF();
                    coreAudioSource.Pause();
                    hudCanvas.gameObject.SetActive(false);
                    Text finalScore = gameCompletedCanvas.transform.Find("Score").GetComponent<Text>();
                    finalScore.text = "Score: " + score;
                    gameCompletedCanvas.gameObject.SetActive(true);
                    OutputUDP.SetClassifyingState(0);
                    OutputUDP.CloseConnection();
                }
                break;
            default:
                break;
        }

        if (coreHp <= emergencyCoreHp && (currentState != "GAME OVER" && currentState != "END"))
        {
            if (!emergencyLightAnimators[0].GetBool("isEmergency"))
            {
                currentEmergencyAudioPrefab = Instantiate(emergencyAudioPrefab, core.transform.position, Quaternion.identity);
                coreAudioTimer = Time.time;
                coreAudioTimeStamp = Time.time + coreAudioTimeInterval;
                coreAudioFadeOutTimeStamp = Time.time + (coreAudioTimeInterval * 0.5f);
                foreach (Animator animator in emergencyLightAnimators)
                {
                    animator.SetBool("isEmergency", true);
                }
            }
            if (coreAudioTimer >= coreAudioFadeOutTimeStamp && currentEmergencyAudioPrefab != null)
            {
                currentEmergencyAudioPrefab.GetComponent<AudioPrefabFadeOutScript>().BeginFadeOut();
            }
            if (coreAudioTimer >= coreAudioTimeStamp)
            {
                currentEmergencyAudioPrefab = Instantiate(emergencyAudioPrefab, core.transform.position, Quaternion.identity);
                coreAudioTimer = Time.time;
                coreAudioTimeStamp = Time.time + coreAudioTimeInterval;
                coreAudioFadeOutTimeStamp = Time.time + (coreAudioTimeInterval * 0.5f);
            }
            else
            {
                coreAudioTimer += Time.deltaTime;
            }
        }
        else
        {
            if (emergencyLightAnimators[0].GetBool("isEmergency"))
            {
                foreach (Animator animator in emergencyLightAnimators)
                {
                    animator.SetBool("isEmergency", false);
                }
            }
        }
    }

    private void StartEnemyWave()
    {
        oneLineText.text = "";
        countdownText.text = "";
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            spawnTimers[i] = 0f;
            if (nextWave == 1)
            {

                spawnTimeLimits[i] = Random.Range(3f, wave1_maxSpawnTime - wave1_minSpawnTime + 3f);
            }
            else if (nextWave == 2)
            {
                spawnTimeLimits[i] = Random.Range(3f, wave2_maxSpawnTime - wave2_minSpawnTime + 3f);
            }
            else
            {
                spawnTimeLimits[i] = Random.Range(3f, wave3_maxSpawnTime - wave3_minSpawnTime + 3f);
            }
        }
        int rand = Random.Range(0, spawnPoints.Count);
        spawnTimeLimits[rand] = Random.Range(3f, 4f);
        currentStateDuration = wave_duration;
        if (nextWave == 1)
        {
            currentState = "WAVE1";
        }
        else if (nextWave == 2)
        {
            currentState = "WAVE2";
        }
        else
        {
            currentState = "WAVE3";
        }
        previousSpawnTime = 0f;
        enemyOnScreen = 0;
        nextWave++;
    }

    private List<GameObject> enemies = new List<GameObject>();

    private bool ProgressEnemyWave()
    {
        currentStateDuration -= Time.deltaTime;
        if (currentStateDuration <= 0f)
        {
            currentStateDuration = testMode ? wave_duration : 0f;
        }

        if (currentState == "WAVE1")
        {
            waveTimerText.text = testMode ? "W0  0:00" : "W1  " + ConvertSecondToTimeFormat(currentStateDuration);
        }
        else if (currentState == "WAVE2")
        {
            waveTimerText.text = "W2  " + ConvertSecondToTimeFormat(currentStateDuration);
        }
        else
        {
            waveTimerText.text = "W3  " + ConvertSecondToTimeFormat(currentStateDuration);
        }

        if (currentStateDuration > 0f)
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                spawnTimers[i] = spawnTimers[i] + Time.deltaTime;
                if (spawnTimers[i] >= spawnTimeLimits[i])
                {
                    float rand = Random.Range(0, 100);
                    int smallEnemySpawnChance = 0;
                    int mediumEnemySpawnChance = 0;
                    int largeEnemySpawnChance = 0;
                    int consecutiveSpawnLimit = 0;
                    spawnTimers[i] = 0f;

                    if (currentState == "WAVE1")
                    {
                        spawnTimeLimits[i] = Random.Range(wave1_minSpawnTime, wave1_maxSpawnTime);
                        smallEnemySpawnChance = wave1_smallEnemySpawnChance;
                        mediumEnemySpawnChance = wave1_mediumEnemySpawnChance;
                        largeEnemySpawnChance = wave1_largeEnemySpawnChance;
                        consecutiveSpawnLimit = wave1_consecutiveSpawnLimit;
                    }
                    else if (currentState == "WAVE2")
                    {
                        spawnTimeLimits[i] = Random.Range(wave2_minSpawnTime, wave2_maxSpawnTime);
                        smallEnemySpawnChance = wave2_smallEnemySpawnChance;
                        mediumEnemySpawnChance = wave2_mediumEnemySpawnChance;
                        largeEnemySpawnChance = wave2_largeEnemySpawnChance;
                        consecutiveSpawnLimit = wave2_consecutiveSpawnLimit;
                    }
                    else
                    {
                        spawnTimeLimits[i] = Random.Range(wave3_minSpawnTime, wave3_maxSpawnTime);
                        smallEnemySpawnChance = wave3_smallEnemySpawnChance;
                        mediumEnemySpawnChance = wave3_mediumEnemySpawnChance;
                        largeEnemySpawnChance = wave3_largeEnemySpawnChance;
                        consecutiveSpawnLimit = wave3_consecutiveSpawnLimit;
                    }

                    bool canSpawn = false;
                    if (Time.time - previousSpawnTime > 5f)
                    {
                        consecutiveSpawn = 1;
                        canSpawn = true;
                        previousSpawnTime = Time.time;
                    }
                    else
                    {
                        if (consecutiveSpawn < consecutiveSpawnLimit)
                        {
                            consecutiveSpawn++;
                            canSpawn = true;
                            previousSpawnTime = Time.time + (consecutiveSpawn * 1.5f);
                        }
                    }

                    if (canSpawn)
                    {
                        enemyOnScreen++;
                        GameObject enemy = null;
                        if (rand <= smallEnemySpawnChance)
                        {
                            enemy = Instantiate(smallEnemy, spawnPoints[i].position, Quaternion.LookRotation(spawnPoints[i].position - core.transform.position));
                            enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[i].position);
                        }
                        else if (rand <= mediumEnemySpawnChance)
                        {
                            enemy = Instantiate(mediumEnemy, spawnPoints[i].position, Quaternion.LookRotation(spawnPoints[i].position - core.transform.position));
                            enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[i].position);
                        }
                        else if (rand <= largeEnemySpawnChance)
                        {
                            enemy = Instantiate(largeEnemy, spawnPoints[i].position, Quaternion.LookRotation(spawnPoints[i].position - core.transform.position));
                            enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[i].position);
                        }
                        enemies.Add(enemy);
                    }
                }
            }
        }

        List<GameObject> enemiesInSight = new List<GameObject>();
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
            }
            else
            {
                EnemyBehavior eb = enemies[i].GetComponent<EnemyBehavior>();
                eb.TurnOffLockonCanvas();
                if (eb.IsInLockedOnVicinity())
                {
                    enemiesInSight.Add(enemies[i]);
                }
            }
        }
        bool lockedTarget = false;
        int lockedTargetIndex = 0;
        if (enemiesInSight.Count == 1)
        {
            lockedTarget = true;
            float smallestAngle = enemiesInSight[0].GetComponent<EnemyBehavior>().GetAngleToCamera();
        }
        else if (enemiesInSight.Count > 1)
        {
            lockedTarget = true;
            float smallestAngle = enemiesInSight[0].GetComponent<EnemyBehavior>().GetAngleToCamera();
            for (int i = 1; i < enemiesInSight.Count; i++)
            {
                float angle = enemiesInSight[i].GetComponent<EnemyBehavior>().GetAngleToCamera();
                if (angle < smallestAngle)
                {
                    lockedTargetIndex = i;
                }
            }
        }
        if (lockedTarget)
        {
            EnemyBehavior eb = enemiesInSight[lockedTargetIndex].GetComponent<EnemyBehavior>();
            eb.TurnOnLockonCanvas();
            player.SetLockonEnemy(eb);
        }
        else
        {
            player.ClearLockonEnemy();
        }

        if ((currentStateDuration == 0f && enemyOnScreen <= 0) || coreHp == 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private string ConvertSecondToTimeFormat(float second)
    {
        int min = (int)(second / 60f);
        int sec = (int)(second % 60f);
        string s_min = min == 0 ? "0" : "" + min;
        string s_sec = "" + sec;
        if (sec == 0)
        {
            s_sec = "00";
        }
        else if (sec < 10)
        {
            s_sec = "0" + sec;
        }
        return s_min + ":" + s_sec;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReduceCoreHp(int enemyType)
    {
        if (testMode)
        {
            return;
        }
        if (enemyType == 1)
        {
            coreHp -= smallEnemyDamage;
        }
        else if (enemyType == 2)
        {
            coreHp -= mediumEnemyDamage;
        }
        else if (enemyType == 3)
        {
            coreHp -= largeEnemyDamage;
        }
        else
        {
            coreHp -= (coreHp * 0.3f);
        }
        coreHp = coreHp < 0f ? 0f : coreHp;
        coreHpBar.fillAmount = coreHp / coreFullHp;
        Instantiate(coreDamagedAudioPrefab, transform.position, Quaternion.identity);
    }

    public void IncreaseCoreHp()
    {
        if (!waiting_alreadyBeginFixing)
        {
            waiting_alreadyBeginFixing = true;
            score = score - fixingCost;
            scoreText.text = "" + score;
        }
        if (coreHp < coreFullHp)
        {
            float fps = 1 / Time.deltaTime;
            coreHp += (increaseCoreHpPerSec / fps);
            if (coreHp > coreFullHp)
            {
                coreHp = coreFullHp;
            }
            coreHpBar.fillAmount = coreHp / coreFullHp;
        }
    }

    public void ActivateLaserFence()
    {
        if (laserFenceAvailable <= 0) {
            return;
        }
        if (!testMode)
        {
            laserFenceAvailable--;
        }
        laserFenceRemainText.text = "" + laserFenceAvailable;
        StartCoroutine(ActivateLaserFenceAndCooldownIcon());
    }

    private IEnumerator ActivateLaserFenceAndCooldownIcon()
    {
        GameObject hummingAudio = Instantiate(laserFenceHummingAudioPrefab, Vector3.zero, Quaternion.identity);
        Instantiate(laserFenceActivatedAudioPrefab, Vector3.zero, Quaternion.identity);
        laserFence.SetActive(true);
        laserFenceCooldownIcon.gameObject.SetActive(true);
        float ratio = 1f;
        laserFenceAvailableTime = Time.time + laserFenceDuration;
        while (Time.time < laserFenceAvailableTime)
        {
            ratio = Mathf.Abs(laserFenceAvailableTime - Time.time) / laserFenceDuration;
            laserFenceCooldownIcon.fillAmount = ratio;
            while (IsPause())
            {
                yield return null;
            }
            yield return null;
        }
        hummingAudio.GetComponent<AudioPrefabFadeOutScript>().BeginFadeOut();
        Instantiate(laserFenceActivatedAudioPrefab, Vector3.zero, Quaternion.identity);
        laserFence.SetActive(false);
        laserFenceCooldownIcon.gameObject.SetActive(false);
        laserFenceCooldownIcon.fillAmount = 1f;
    }

    public bool IsCoreHpFull()
    {
        return coreHp >= coreFullHp;
    }

    public bool CanActivateLaserFence()
    {
        if ((currentState == "WAVE1" || currentState == "WAVE2" || currentState == "WAVE3") && stateStarted && laserFenceAvailable > 0 && !laserFence.activeSelf)
        {
            if (wave_duration - currentStateDuration > 1.5f)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanFixCore()
    {
        if (currentState == "WAITING" && stateStarted)
        {
            if (waiting_alreadyBeginFixing)
            {
                if (coreHp < coreFullHp)
                {
                    return true;
                }
            }
            else
            {
                if (score >= fixingCost && coreHp < coreFullHp)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsInWaveCompletedState()
    {
        if (currentState == "WAVE COMPLETED")
        {
            return true;
        }
        return false;
    }

    public static bool IsInWaitingState()
    {
        if (currentState == "WAITING")
        {
            return true;
        }
        return false;
    }

    public static bool IsPause()
    {
        return pause;
    }

    public static bool IsGameOver()
    {
        return gameOver;
    }

    public Vector3 GetPlayerCameraPosition()
    {
        return player.GetCameraPosition();
    }

    public Vector3 GetPlayerLookingDirection()
    {
        return player.GetLookingDirection();
    }

    public void RemoveEnemyOnScreen()
    {
        enemyOnScreen--;
    }

    public void AddScore(int type)
    {
        if (type == 1)
        {
            score += smallEnemyScore;
        }
        else if (type == 2)
        {
            score += mediumEnemyScore;
        }
        else
        {
            score += largeEnemyScore;
        }
        scoreText.text = "" + score;
    }

    public void SkipWaitingState()
    {
        if (currentState == "WAITING")
        {
            currentStateDuration = 0f;
        }
    }
}
