using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
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
    [SerializeField] private Image coreHpBar;
    [SerializeField] private Image laserFenceCooldownIcon;
    [SerializeField] private Text waveTimerText;
    [SerializeField] private Text laserFenceRemainText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text congratText;
    [SerializeField] private Text oneLineText;
    [SerializeField] private Text twoLineText;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text objectiveTargetText;
    [SerializeField] private Text pauseTestResultText;
    [SerializeField] private Text gameOverTestResultText;
    [SerializeField] private Text gameCompleteTestResultText;
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

    [SerializeField] private Text fpsText;

    private static string currentState;
    private static float coreHp;
    private static float coreFullHp;
    private bool classifyMode;
    private bool testMode;
    private bool calibrationMode;

    public static bool pause;
    public static bool gameOver;
    public static float defaultTimeScale;

    private bool stateStarted;
    private float currentStateDuration;
    private float previousSpawnTime;
    private int consecutiveSpawn;
    private int enemyOnScreen;

    private float startDuration;
    private float instructionDuration;
    private float waitingDuration;
    private float waveDuration;
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
    private int objectiveCounter;
    private int objectiveTargetCounter;
    private int enemiesTakenOutByAR;
    private int enemiesTakenOutBySR;
    private int enemiesTakenOutByLaser;
    private int ssvepCommandCount;
    private float accSsvepCommandDelay;

    private List<float> spawnTimeLimits = new List<float>();
    private List<float> spawnTimers = new List<float>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        classifyMode = GameModeRecorder.classifyMode;
        testMode = GameModeRecorder.testMode;
        calibrationMode = GameModeRecorder.calibrationMode;

        pause = false;
        gameOver = false;
        LockCursor();
        defaultTimeScale = 1f;
        Time.timeScale = defaultTimeScale;

        currentState = "START";
        startDuration = 5.9f;
        instructionDuration = 4.25f;
        waitingDuration = 10.9f;
        waveDuration = 30.9f;

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
        nextWave = testMode ? 2 : 1;

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
        objectiveText.gameObject.SetActive(true);
        objectiveTargetText.gameObject.SetActive(true);
        waveTimerText.text = "W0  0:00";
        laserFenceRemainText.text = "" + laserFenceAvailable;
        scoreText.text = "0";
        congratText.text = "";
        oneLineText.text = "";
        twoLineText.text = "";
        countdownText.text = "";
        objectiveText.text = "";
        objectiveTargetText.text = "";
        if (calibrationMode)
        {
            oneLineText.text = "Welcome to the calibration phase.";
        }
        else if (testMode)
        {
            oneLineText.text = "Prepare for the enemies...";
            countdownText.text = "" + (int)startDuration;
        }
        else
        {
            oneLineText.text = "Prepare for the first wave...";
            countdownText.text = "" + (int)startDuration;
        }

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

    private float fpsTime;
    private int frameCount = -1;
    private void Update()
    {
        if (fpsText != null)
        {
            if (fpsTime >= 1f)
            {
                fpsText.text = "" + frameCount;
                frameCount = -1;
                fpsTime = 0;
            }
            frameCount++;
            fpsTime += Time.deltaTime / Time.timeScale;
            //fpsText.text = "" + ((int)Math.Round(1.0f / Time.deltaTime, 0));
        }
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
                    DisableHUD();
                    pauseCanvas.gameObject.SetActive(true);
                    EyeTrackerController.TurnOffBlinkRecording();
                }
                else
                {
                    LockCursor();
                    coreAudioSource.UnPause();
                    player.TurnOffDOF();
                    EnableHUD();
                    pauseCanvas.gameObject.SetActive(false);
                    EyeTrackerController.TurnOnBlinkRecording();
                }
            }
        }
        if (pause)
        {
            double avgCommandDelay = ssvepCommandCount == 0 ? 0 : Math.Round(accSsvepCommandDelay / ssvepCommandCount, 3);
            pauseTestResultText.text = "Enemies taken out by AR: " + enemiesTakenOutByAR +
                "\nEnemies taken out by SR: " + enemiesTakenOutBySR +
                "\nEnemies taken out by Laser: " + enemiesTakenOutByLaser +
                "\nAvg SSVEP command delay: " + avgCommandDelay + " sec";
            return;
        }

        switch (currentState)
        {
            case "START":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = startDuration;
                    OutputUDP.OpenConnection();
                    OutputUDP.SetRecordingState(0);
                    if (classifyMode)
                    {
                        OutputUDP.SetClassifyingState(1);
                    }
                    else if (calibrationMode)
                    {
                        OutputUDP.SetClassifyingState(0);
                    }
                }
                currentStateDuration -= Time.deltaTime;
                if (!calibrationMode)
                {
                    countdownText.text = currentStateDuration < 0f ? "0" : "" + (int)currentStateDuration;
                }
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    oneLineText.text = "";
                    twoLineText.text = "";
                    countdownText.text = "";
                    if (!calibrationMode)
                    {
                        StartEnemyWave();
                    }
                    else
                    {
                        currentState = "STEP0";
                    }
                }
                break;
            // calibration state
            case "STEP0":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 1;
                    oneLineText.text = "Pay attention to the stimulus to start.";
                    player.StartSkipStimulus();
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    oneLineText.text = "";
                    currentState = "STEP1 INS";
                    player.StopSkipStimulus();
                }
                break;
            case "STEP1 INS":
                CalibrationInsStepHandler("Step 1:\nTry to walk around without blinking\nfor " + 
                    EyeTrackerController.GetValidDurationSinceLastBlink() + " second.", false, "STEP1");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 1:\nTry to walk around without blinking\nfor 2.5 second.";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "STEP1";
                }*/
                break;
            case "STEP1":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 4;
                    objectiveText.text = "Try to walk around\nwithout blinking for " + EyeTrackerController.GetValidDurationSinceLastBlink() + " second.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "STEP2 INS";
                }
                break;
            case "STEP":
                break;
            case "STEP2 INS":
                CalibrationInsStepHandler("Step 2:\nDestroy the enemies with auto rifle.\nDon't blink while shooting.");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 2:\nDestroy the enemies with auto rifle.\nDon't blink while shooting.";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    StartEnemyWave();
                }*/
                break;
            case "STEP2":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 4;
                    objectiveText.text = "Destroy the enemies with auto rifle.\nDon't blink while shooting.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                }
                if (ProgressEnemyWave())
                {
                    stateStarted = false;
                    currentState = "WAVE COMPLETED";
                }
                else
                {
                    if (objectiveCounter >= objectiveTargetCounter)
                    {
                        objectiveText.text = "Destroy the remaining enemies.";
                        objectiveTargetText.text = "";
                    }
                }
                break;
            case "STEP3 INS":
                CalibrationInsStepHandler("Step 3:\nLook at the core and pay attention to the stimulus.\n", false, "STEP3");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 3:\nLook at the core and pay attention to the stimulus.\n";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "STEP3";
                }*/
                break;
            case "STEP3":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 1;
                    objectiveText.text = "Look at the core and\npay attention to the stimulus.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "STEP4 INS";
                }
                break;
            case "STEP4 INS":
                CalibrationInsStepHandler("Step 4:\nPay attention to the stimulus\nat the buttom of the screen.", false, "STEP4");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 4:\nPay attention to the stimulus\nat the buttom of the screen.";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "STEP4";
                }*/
                break;
            case "STEP4":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 1;
                    objectiveText.text = "Pay attention to the stimulus\nat the buttom of the screen.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                    player.StartSkipStimulus();
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "STEP5 INS";
                    player.StopSkipStimulus();
                }
                break;
            case "STEP5 INS":
                CalibrationInsStepHandler("Step 5:\nUse sniper rifle to aim at the enemies and\npay attention to the stimulus.", false, "STEP5", true);
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 5:\nUse sniper rifle to aim at the enemies and\npay attention to the stimulus.";
                    currentStateDuration = instructionDuration;
                    player.ForceToChangeGun();
                    player.EnableADS();
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    StartEnemyWave();
                }*/
                break;
            case "STEP5":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 4;
                    objectiveText.text = "Use sniper rifle to aim at the enemies\nand pay attention to the stimulus.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                }
                if (ProgressEnemyWave())
                {
                    stateStarted = false;
                    currentState = "WAVE COMPLETED";
                }
                else
                {
                    if (objectiveCounter >= objectiveTargetCounter)
                    {
                        objectiveText.text = "Destroy the remaining enemies.";
                        objectiveTargetText.text = "";
                    }
                }
                break;
            case "STEP6 INS":
                CalibrationInsStepHandler("Step 6:\nLook at the core and pay attention to the stimulus.\n", false, "STEP6");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 6:\nLook at the core and pay attention to the stimulus.\n";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "STEP6";
                }*/
                break;
            case "STEP6":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 1;
                    objectiveText.text = "Look at the core and\npay attention to the stimulus.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "STEP7 INS";
                }
                break;
            case "STEP7 INS":
                CalibrationInsStepHandler("Step 7:\nPay attention to the stimulus\nat the buttom of the screen.", false, "STEP7");
                /*if (!stateStarted)
                {
                    stateStarted = true;
                    twoLineText.text = "Step 7:\nPay attention to the stimulus\nat the buttom of the screen.";
                    currentStateDuration = instructionDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    twoLineText.text = "";
                    currentState = "STEP7";
                }*/
                break;
            case "STEP7":
                if (!stateStarted)
                {
                    stateStarted = true;
                    objectiveCounter = 0;
                    objectiveTargetCounter = 1;
                    objectiveText.text = "Pay attention to the stimulus\nat the buttom of the screen.";
                    objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                    player.StartSkipStimulus();
                }
                if (objectiveCounter >= objectiveTargetCounter)
                {
                    stateStarted = false;
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "FINAL STEP";
                    player.StopSkipStimulus();
                }
                break;
            case "FINAL STEP":
                if (!stateStarted)
                {
                    stateStarted = true;
                    congratText.text = "CALIBRATION COMPLETED";
                    currentStateDuration = startDuration;
                }
                currentStateDuration -= Time.deltaTime;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    congratText.text = "";
                    SceneManager.LoadScene(0);
                }
                break;
            // normal state
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
                    objectiveText.text = "";
                    objectiveTargetText.text = "";
                    currentState = "CONGRAT";
                }
                break;
            case "CONGRAT":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = congratStateDuration;
                    if (calibrationMode)
                    {
                        congratText.text = nextWave == 2 ? "STEP 2 COMPLETED" : "STEP 5 COMPLETED";

                    }
                    else
                    {
                        congratText.text = "WAVE COMPLETED";
                    }
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
                    twoLineText.text = "We are reducing some core HP.\nFixing it will cost you " + fixingCost + " points.\n";
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
                    if (calibrationMode)
                    {
                        currentState = nextWave == 2 ? "STEP3 INS" : "STEP6 INS";
                    }
                    else
                    {
                        currentState = "WAITING";
                    }
                }
                break;
            case "WAITING":
                if (!stateStarted)
                {
                    stateStarted = true;
                    currentStateDuration = waitingDuration;
                    oneLineText.text = "Prepare for the next wave...";
                    player.StartSkipStimulus();
                }
                currentStateDuration -= Time.deltaTime;
                countdownText.text = currentStateDuration < 0f ? "0" : "" + (int)currentStateDuration;
                if (currentStateDuration <= 0f)
                {
                    stateStarted = false;
                    oneLineText.text = "";
                    twoLineText.text = "";
                    countdownText.text = "";
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
                    DisableHUD();
                    double avgCommandDelay = ssvepCommandCount == 0 ? 0 : Math.Round(accSsvepCommandDelay / ssvepCommandCount, 3);
                    gameOverTestResultText.text = "Enemies taken out by AR: " + enemiesTakenOutByAR +
                        "\nEnemies taken out by SR: " + enemiesTakenOutBySR +
                        "\nEnemies taken out by Laser: " + enemiesTakenOutByLaser +
                        "\nAvg SSVEP command delay: " + avgCommandDelay + " sec";
                    gameOverCanvas.gameObject.SetActive(true);
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
                    DisableHUD();
                    Text finalScore = gameCompletedCanvas.transform.Find("Score").GetComponent<Text>();
                    finalScore.text = "Score: " + score;
                    double avgCommandDelay = ssvepCommandCount == 0 ? 0 : Math.Round(accSsvepCommandDelay / ssvepCommandCount, 3);
                    gameCompleteTestResultText.text = "Enemies taken out by AR: " + enemiesTakenOutByAR +
                        "\nEnemies taken out by SR: " + enemiesTakenOutBySR +
                        "\nEnemies taken out by Laser: " + enemiesTakenOutByLaser +
                        "\nAvg SSVEP command delay: " + avgCommandDelay + " sec";
                    gameCompletedCanvas.gameObject.SetActive(true);
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

    private bool timesUpFlag;
    private void StartEnemyWave()
    {
        timesUpFlag = false;
        currentStateDuration = waveDuration;
        if (!calibrationMode)
        {
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
        }
        else
        {
            currentState = nextWave == 1 ? "STEP2" : "STEP5";
        }
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            spawnTimers[i] = 0f;
            if (currentState == "WAVE1")
            {
                spawnTimeLimits[i] = Random.Range(3f, wave1_maxSpawnTime - wave1_minSpawnTime + 3f);
            }
            else if (currentState == "WAVE2")
            {
                spawnTimeLimits[i] = Random.Range(3f, wave2_maxSpawnTime - wave2_minSpawnTime + 3f);
            }
            else if (currentState == "WAVE3")
            {
                spawnTimeLimits[i] = Random.Range(3f, wave3_maxSpawnTime - wave3_minSpawnTime + 3f);
            }
            else
            {
                spawnTimeLimits[i] = Random.Range(3f, wave1_maxSpawnTime - wave1_minSpawnTime + 3f);
            }
        }
        int rand = Random.Range(0, spawnPoints.Count);
        spawnTimeLimits[rand] = Random.Range(3f, 4f);
        previousSpawnTime = 0f;
        enemyOnScreen = 0;
        objectiveCounter = 0;
        nextWave++;
    }

    private List<GameObject> enemies = new List<GameObject>();
    private int lastSpawnPointIndex = 0;
    private bool ProgressEnemyWave()
    {
        if (!testMode && !calibrationMode)
        {
            currentStateDuration -= Time.deltaTime;
            if (currentStateDuration <= 0f)
            {
                currentStateDuration = 0f;
            }
        }
        else if (calibrationMode && objectiveCounter >= objectiveTargetCounter)
        {
            currentStateDuration = 0f;
        }

        if (currentState == "WAVE1")
        {
            waveTimerText.text = testMode ? "W0  0:00" : "W1  " + ConvertSecondToTimeFormat(currentStateDuration);
        }
        else if (currentState == "WAVE2")
        {
            waveTimerText.text = testMode ? "W0  0:00" : "W2  " + ConvertSecondToTimeFormat(currentStateDuration);
        }
        else if (currentState == "WAVE3")
        {
            waveTimerText.text = testMode ? "W0  0:00" : "W3  " + ConvertSecondToTimeFormat(currentStateDuration);
        }
        else
        {
            waveTimerText.text = "W0  0:00";
        }

        if (!timesUpFlag)
        {
            float rand = Random.Range(0, 100);
            int smallEnemySpawnChance = 0;
            int mediumEnemySpawnChance = 0;
            int largeEnemySpawnChance = 0;
            int consecutiveSpawnLimit = 0;
            if (currentStateDuration <= 0f && enemyOnScreen > 0)
            {
                timesUpFlag = true;
            }
            else if (currentStateDuration <= 0f)
            {
                timesUpFlag = true;
                int randInt = Random.Range(0, spawnPoints.Count);
                while (randInt == lastSpawnPointIndex)
                {
                    randInt = Random.Range(0, spawnPoints.Count);
                }
                if (currentState == "WAVE1")
                {
                    smallEnemySpawnChance = wave1_smallEnemySpawnChance;
                    mediumEnemySpawnChance = wave1_mediumEnemySpawnChance;
                    largeEnemySpawnChance = wave1_largeEnemySpawnChance;
                }
                else if (currentState == "WAVE2")
                {
                    smallEnemySpawnChance = wave2_smallEnemySpawnChance;
                    mediumEnemySpawnChance = wave2_mediumEnemySpawnChance;
                    largeEnemySpawnChance = wave2_largeEnemySpawnChance;
                }
                else if (currentState == "WAVE3")
                {
                    smallEnemySpawnChance = wave3_smallEnemySpawnChance;
                    mediumEnemySpawnChance = wave3_mediumEnemySpawnChance;
                    largeEnemySpawnChance = wave3_largeEnemySpawnChance;
                }
                else
                {
                    smallEnemySpawnChance = 0;
                    mediumEnemySpawnChance = 0;
                    largeEnemySpawnChance = 100;
                }

                enemyOnScreen++;
                GameObject enemy = null;
                if (rand <= smallEnemySpawnChance)
                {
                    enemy = Instantiate(smallEnemy, spawnPoints[randInt].position, Quaternion.LookRotation(spawnPoints[randInt].position - core.transform.position));
                    enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[randInt].position);
                }
                else if (rand <= mediumEnemySpawnChance)
                {
                    enemy = Instantiate(mediumEnemy, spawnPoints[randInt].position, Quaternion.LookRotation(spawnPoints[randInt].position - core.transform.position));
                    enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[randInt].position);
                }
                else if (rand <= largeEnemySpawnChance)
                {
                    enemy = Instantiate(largeEnemy, spawnPoints[randInt].position, Quaternion.LookRotation(spawnPoints[randInt].position - core.transform.position));
                    enemy.GetComponent<EnemyBehavior>().GiveInstruction(this, decelerationPoints[randInt].position);
                }
                enemies.Add(enemy);
            }
            else
            {
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    spawnTimers[i] = spawnTimers[i] + Time.deltaTime;
                    if (spawnTimers[i] >= spawnTimeLimits[i])
                    {
                        rand = Random.Range(0, 100);
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
                        else if (currentState == "WAVE3")
                        {
                            spawnTimeLimits[i] = Random.Range(wave3_minSpawnTime, wave3_maxSpawnTime);
                            smallEnemySpawnChance = wave3_smallEnemySpawnChance;
                            mediumEnemySpawnChance = wave3_mediumEnemySpawnChance;
                            largeEnemySpawnChance = wave3_largeEnemySpawnChance;
                            consecutiveSpawnLimit = wave3_consecutiveSpawnLimit;
                        }
                        else
                        {
                            spawnTimeLimits[i] = Random.Range(wave1_minSpawnTime, wave1_maxSpawnTime);
                            smallEnemySpawnChance = 0;
                            mediumEnemySpawnChance = 0;
                            largeEnemySpawnChance = 100;
                            consecutiveSpawnLimit = wave1_consecutiveSpawnLimit;
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
                            lastSpawnPointIndex = i;
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

        if ((timesUpFlag && enemyOnScreen <= 0) || coreHp == 0f)
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
        if (testMode || calibrationMode)
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

    public void IncreaseCoreHp(float stimulusGazeDuration = -1f)
    {
        if (!waiting_alreadyBeginFixing)
        {
            waiting_alreadyBeginFixing = true;
            score = score - fixingCost;
            scoreText.text = "" + score;
            if (stimulusGazeDuration != -1f)
            {
                UpdateAccCommandDelay(stimulusGazeDuration);
            }
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

    private bool crosshairSavedStatus = false;
    private bool aimCrosshairSavedStatus = false;
    private bool coreStimulusSavedStatus = false;
    private bool skipStimulusSavedStatus = false;
    private bool laserFenceCooldownSavedStatus = false;
    private void DisableHUD()
    {
        GameObject temp = hudCanvas.transform.Find("Crosshair").gameObject;
        if (temp.activeSelf)
        {
            crosshairSavedStatus = true;
        }
        temp.SetActive(false);
        temp = hudCanvas.transform.Find("AimCrosshair").gameObject;
        if (temp.activeSelf)
        {
            aimCrosshairSavedStatus = true;
        }        
        temp.SetActive(false);
        hudCanvas.transform.Find("CoreHpBarBg").gameObject.SetActive(false);
        hudCanvas.transform.Find("CoreHpBar").gameObject.SetActive(false);
        hudCanvas.transform.Find("SkillCooldown").gameObject.SetActive(false);
        hudCanvas.transform.Find("Gun").gameObject.SetActive(false);
        hudCanvas.transform.Find("Info").gameObject.SetActive(false);
        hudCanvas.transform.Find("Score").gameObject.SetActive(false);
        temp = hudCanvas.transform.Find("LaserFenceStimulus").gameObject;
        if (temp.activeSelf)
        {
            coreStimulusSavedStatus = true;
        }
        temp.SetActive(false);
        temp = hudCanvas.transform.Find("LaserFenceCooldown").gameObject;
        if (temp.activeSelf)
        {
            laserFenceCooldownSavedStatus = true;
        }
        temp.SetActive(false);
        temp = hudCanvas.transform.Find("SkipStimulus").gameObject;
        if (temp.activeSelf)
        {
            skipStimulusSavedStatus = true;
        }
        temp.SetActive(false);
        hudCanvas.transform.Find("ObjectiveText").gameObject.SetActive(false);
        hudCanvas.transform.Find("ObjectiveTargetText").gameObject.SetActive(false);
        hudCanvas.transform.Find("CongratText").gameObject.SetActive(false);
        hudCanvas.transform.Find("1LineMessageText").gameObject.SetActive(false);
        hudCanvas.transform.Find("2LineMessageText").gameObject.SetActive(false);
        hudCanvas.transform.Find("Countdown").gameObject.SetActive(false);
    }

    private void EnableHUD()
    {
        if (crosshairSavedStatus)
        {
            crosshairSavedStatus = false;
            hudCanvas.transform.Find("Crosshair").gameObject.SetActive(true);
        }
        if (aimCrosshairSavedStatus)
        {
            aimCrosshairSavedStatus = true;
            hudCanvas.transform.Find("AimCrosshair").gameObject.SetActive(true);
        }
        hudCanvas.transform.Find("CoreHpBarBg").gameObject.SetActive(true);
        hudCanvas.transform.Find("CoreHpBar").gameObject.SetActive(true);
        hudCanvas.transform.Find("SkillCooldown").gameObject.SetActive(true);
        hudCanvas.transform.Find("Gun").gameObject.SetActive(true);
        hudCanvas.transform.Find("Info").gameObject.SetActive(true);
        hudCanvas.transform.Find("Score").gameObject.SetActive(true);
        if (coreStimulusSavedStatus)
        {
            coreStimulusSavedStatus = false;
            hudCanvas.transform.Find("LaserFenceStimulus").gameObject.SetActive(true);
        }
        if (laserFenceCooldownSavedStatus)
        {
            laserFenceCooldownSavedStatus = false;
            hudCanvas.transform.Find("LaserFenceCooldown").gameObject.SetActive(true);
        }
        if (skipStimulusSavedStatus)
        {
            skipStimulusSavedStatus = false;
            hudCanvas.transform.Find("SkipStimulus").gameObject.SetActive(true);
        }
        hudCanvas.transform.Find("ObjectiveText").gameObject.SetActive(true);
        hudCanvas.transform.Find("ObjectiveTargetText").gameObject.SetActive(true);
        hudCanvas.transform.Find("CongratText").gameObject.SetActive(true);
        hudCanvas.transform.Find("1LineMessageText").gameObject.SetActive(true);
        hudCanvas.transform.Find("2LineMessageText").gameObject.SetActive(true);
        hudCanvas.transform.Find("Countdown").gameObject.SetActive(true);
    }

    private void CalibrationInsStepHandler(string message, bool startWave = true, string nextState = "", bool changeGun = false)
    {
        if (!stateStarted)
        {
            stateStarted = true;
            twoLineText.text = message;
            currentStateDuration = instructionDuration;
            if (changeGun)
            {
                player.ForceToChangeGun();
            }
        }
        currentStateDuration -= Time.deltaTime;
        if (currentStateDuration <= 0f)
        {
            stateStarted = false;
            twoLineText.text = "";
            if (startWave)
            {
                StartEnemyWave();
            }
            else
            {
                currentState = nextState;
            }
        }
    }

    public bool IsCoreHpFull()
    {
        return coreHp >= coreFullHp;
    }

    public bool CanActivateLaserFence()
    {
        if ((currentState == "WAVE1" || currentState == "WAVE2" || currentState == "WAVE3") && stateStarted && laserFenceAvailable > 0 && !laserFence.activeSelf)
        {
            if (waveDuration - currentStateDuration > 1.5f)
            {
                return true;
            }
            else if (testMode)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanFixCore()
    {
        if ((currentState == "WAITING" || currentState == "STEP3" || currentState == "STEP6") && stateStarted)
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

    public void AddScore(int addedScore)
    {
        score += addedScore;
        scoreText.text = "" + score;
    }

    public void IncreaseEnemiesTakenOutByAR()
    {
        enemiesTakenOutByAR++;
    }

    public void IncreaseEnemiesTakenOutBySR()
    {
        enemiesTakenOutBySR++;
    }

    public void IncreaseEnemiesTakenOutByLaser()
    {
        enemiesTakenOutByLaser++;
    }

    public void UpdateAccCommandDelay(float delay)
    {
        accSsvepCommandDelay += delay;
        ssvepCommandCount++;
    }

    public void SkipWaitingState()
    {
        if (currentState == "WAITING")
        {
            currentStateDuration = 0f;
        }
    }

    public bool IncreaseObjectiveCounter(string state, int recordingState = 0)
    {
        if (state == currentState && objectiveCounter < objectiveTargetCounter)
        {
            objectiveCounter++;
            if (recordingState != 0)
            {
                objectiveTargetText.text = objectiveCounter + " / " + objectiveTargetCounter;
                OutputUDP.SetRecordingState(recordingState);
            }
            return true;
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

    public static bool IsPause()
    {
        return pause;
    }

    public static bool IsGameOver()
    {
        return gameOver;
    }
}
