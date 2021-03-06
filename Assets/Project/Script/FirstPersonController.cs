using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField] GameController gameController;
    [SerializeField] Text bci2000Input;
    [SerializeField] AssaultRifleController g_AssaultRifleController;
    [SerializeField] SniperRifleController g_SniperRifleController;
    [SerializeField] SSVEPStimulusController h_CoreStimulusController;
    [SerializeField] SSVEPStimulusController h_SkipStimulusController;
    [SerializeField] Image h_CoreCommandArea;
    [SerializeField] Image h_SkipCommandArea;
    [SerializeField] Image h_ShootCommandArea;
    [SerializeField] Image h_GunAndSkillCommandArea;
    [SerializeField] Image h_GunModeCommandArea;
    [SerializeField] Image h_GunModeAimCommandArea;
    [SerializeField] Image h_StimulusHighlightBlank;
    [SerializeField] Image s_SkillUseEffect;
    [SerializeField] Image s_SkillCooldownHUD;
    [SerializeField] Image g_GunHUD;
    [SerializeField] Image g_AssaultRifleIcon;
    [SerializeField] Image g_SniperRifleIcon;
    [SerializeField] float g_GunbobAmountX = 0.02f;
    [SerializeField] float g_GunbobAmountY = 0.007f;
    [SerializeField] float g_BackToNormalBobIn = 0.8f;
    [SerializeField] float g_HolsterDuration = 0.3f;
    [SerializeField] float s_SkillCooldown = 5f;

    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private AudioClip[] m_FootstepSounds;
    [SerializeField] private GameObject s_SkillUseAudioPrefab;
    [SerializeField] private GameObject s_SkillMissAudioPrefab;

    [SerializeField] private GameObject f_FixingCoreAudioPrefab;
    [SerializeField] private GameObject f_FinishedFixingCoreAudioPrefab;

    private GunController g_GunController;
    private Vector3 g_OriginalPosition;
    private Vector3 g_AimingStartPosition;
    private Vector3 g_AimingIntendedPosition;
    private Vector3 g_GunBobLeftSwayPosition;
    private Vector3 g_GunBobRightSwayPosition;
    private Vector3 g_HolsterStartPosition;
    private Vector3 g_HolsterIntendedPosition;
    private Vector3 g_RecoilStartPosition;
    private Vector3 g_RecoilIntendedPosition;
    private Vector3 g_RecoilSavedStartPosition;
    private Vector3 g_RecoilSavedIntendedPosition;
    private Quaternion g_HolsterStartAngle;
    private Quaternion g_HolsterIntendedAngle;
    private bool g_Aiming;
    private bool g_AimInterpolating;
    private bool g_IdleInterpolating;
    private bool g_SwayRight;
    private bool g_BackupSwayRight;
    private bool g_Kicking;
    private bool g_KickingUp;
    private bool g_BackingToNormalBob;
    private bool g_Switching;
    private bool g_SwitchingFromScope;
    private bool g_DelayingGunSwitching;
    private float g_AimingStartTime;
    private float g_ScopeFadingStartTime;
    private float g_InterpolatingDistance;
    private float g_BackingToNormalBobStartTime;
    private float g_HolsterStartTime;
    private float g_KickUpStartTime;
    private float g_AimingStartFOV;
    private float g_AimingIntendedFOV;
    private float g_FOVDelta;
    private float g_AimingStartAlpha;
    private float g_AimingIntendedAlpha;
    private float g_AlphaDelta;
    private float g_ScopeKickUpAcc;
    private string g_CurrentGun;
    private string g_PreviousGun;

    private Camera m_Camera;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private Vector3 m_OriginalCameraPosition;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_YRotation;
    private float m_StepCycle;
    private float m_NextStep;
    private float m_PreviousNextStep;
    private float m_BackupPreviousNextStep;
    private float m_PreviousSpeed;
    private float m_BackupPreviousStepCycle;
    private float m_LastTimeMoving;
    private bool m_Moving;

    private PostProcessingProfile postProcessingProfile;

    private Image s_SkillCooldownOverlay;
    private float s_SkillAvailableTime;
    private float s_SuccessFadeInTime;
    private float s_SuccessFadeOutTime;
    private float s_FailFadeInTime;
    private float s_FailFadeOutTime;

    private AudioSource audioSource;
    private GameObject fixingCoreAudioPrefab;

    private EnemyBehavior lockedOnEnemy;
    private EnemyBehavior currentEnemyInCrosshair;
    private bool notBlinkDuringShootingEnemy;
    private bool forceToChangeGun;
    private bool calibrating;
    private bool classifying;
    private float timeSinceLastBlink;
       
    private Vector3[] gunAndSkillCommandAreaCorners = new Vector3[4];
    private Vector3[] gunModeCommandAreaCorners = new Vector3[4];
    private Vector3[] gunModeAimCommandAreaCorners = new Vector3[4];
    private Vector3[] coreCommandStimulusAreaCorners = new Vector3[4];
    private Vector3[] skipCommandStimulusAreaCorners = new Vector3[4];
    private Vector3[] shootCommandStimulusAreaCorners = new Vector3[4];
    private Vector2 gunPanelPosition = new Vector2();
    private Vector2 skillPanelPosition = new Vector2();
    private Color stimulusHighlightBlankColor; 
    private float skipPressDuration;
    private float stimulusGazeDuration;
    private float savedStimulusGazeDuration;
    private float stimulusNotGazeDuration;
    private float gazeShiftDuration;
    private bool previousSsvepInput;
    // has 4 value: NONE, CORE, SKIP, ENEMY
    private string currentGazeZone = "NONE";

    private Color redColor = new Color(1f, 0.133f, 0.133f);
    private Color greenColor = new Color(0.318f, 0.965f, 0f);

    private DepthOfFieldModel.Settings ppSetting;
    private bool highlightingStimulus;
    private bool turningOffHighlight;
    private bool highlightOn;
    private float highlightStartTime;
    private float highlightDuration;
    private float highlightDefaultDuration = 0.075f;
    private float turnOffDefaultDuration = 0.1f;
    private float highlightStartFocus;
    private float highlightDefaultStartFocus = 4f;
    private float highlightStartAlpha;
    private float highlightDefaultAlpha = 0.8245f;

    // Use this for initialization
    private void Start()
    {
        calibrating = GameModeRecorder.calibrationMode;
        classifying = GameModeRecorder.classifyMode;

        PrepareCommandAreaCorners();
        SetUIPosition(g_GunHUD.gameObject.GetComponent<RectTransform>(), out gunPanelPosition);
        SetUIPosition(s_SkillCooldownHUD.gameObject.GetComponent<RectTransform>(), out skillPanelPosition);
        
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_NextStep = m_StepInterval;
        m_LastTimeMoving = -1f;
        audioSource = GetComponent<AudioSource>();
		m_MouseLook.Init(transform , m_Camera.transform);

        SetGunController("AR");
        g_PreviousGun = "AR";
        g_OriginalPosition = g_GunController.transform.localPosition;
        g_GunBobLeftSwayPosition = g_OriginalPosition - new Vector3(g_GunbobAmountX, g_GunbobAmountY, 0f);
        g_GunBobRightSwayPosition = g_OriginalPosition + new Vector3(g_GunbobAmountX, -g_GunbobAmountY, 0f);
        g_RecoilSavedIntendedPosition = g_OriginalPosition;

        g_SniperRifleController.transform.localPosition = g_OriginalPosition - new Vector3(0f, 0.275f, 0.17f);
        g_SniperRifleController.transform.localRotation = Quaternion.Euler(g_SniperRifleController.transform.localRotation.eulerAngles + new Vector3(40f, 0f, 10f));
        g_SniperRifleController.gameObject.SetActive(false);
        
        postProcessingProfile = m_Camera.gameObject.GetComponent<PostProcessingBehaviour>().profile;
        ppSetting = postProcessingProfile.depthOfField.settings;
        ppSetting.focusDistance = highlightDefaultStartFocus;
        postProcessingProfile.depthOfField.settings = ppSetting;
        postProcessingProfile.colorGrading.enabled = false;
        postProcessingProfile.depthOfField.enabled = false;

        s_SuccessFadeInTime = 0.12f;
        s_SuccessFadeOutTime = 0.75f;
        s_FailFadeInTime = 0.034f;
        s_FailFadeOutTime = 0.05f;
        s_SkillCooldownOverlay = s_SkillCooldownHUD.transform.Find("Overlay").gameObject.GetComponent<Image>();
        s_SkillCooldownOverlay.fillAmount = 1f;
        stimulusHighlightBlankColor = h_StimulusHighlightBlank.color;
        Color color = s_SkillCooldownOverlay.color;
        color.a = 0f;
        s_SkillCooldownOverlay.color = color;
        color = s_SkillUseEffect.color;
        color.a = 0f;
        s_SkillUseEffect.color = color;

        color = g_SniperRifleIcon.color;
        color.a = 0.1f;
        g_SniperRifleIcon.color = color;
    }

    private void Update()
    {
        if (GameController.IsPause() || GameController.IsGameOver())
        {
            timeSinceLastBlink = 0f;
            skipPressDuration = 0f;
            stimulusGazeDuration = 0f;
            notBlinkDuringShootingEnemy = false;
            return;
        }
        audioSource.pitch = Time.timeScale;
        // stop character when hitting the ground
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            m_MoveDir.y = 0f;
        }
        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        // stimulus start/stop flickering
        Ray ray = m_Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        Physics.Raycast(ray.origin, ray.direction, out hit);
        if (hit.transform.gameObject.tag == "Core" && (gameController.CanActivateLaserFence() || gameController.CanFixCore()))
        {
            if (!h_CoreStimulusController.IsFlickering())
            {
                h_CoreStimulusController.StartFlickering();
            }
        }
        else
        {
            if (h_CoreStimulusController.IsFlickering())
            {
                h_CoreStimulusController.StopFlickering();
            }
        }

        // useful variables
        bool rayHitEnemy = hit.transform.gameObject.tag == "Enemy";
        bool usingSniper = g_CurrentGun == "SR";
        bool scoping = usingSniper && g_Aiming && !g_AimInterpolating;
        if (rayHitEnemy)
        {
            EnemyBehavior tempEb = hit.transform.gameObject.GetComponent<EnemyBehavior>();
            if (currentEnemyInCrosshair != null)
            {
                if (hit.transform.gameObject.GetInstanceID() != currentEnemyInCrosshair.gameObject.GetInstanceID() && !tempEb.IsDestroyed())
                {
                    currentEnemyInCrosshair = tempEb;
                    notBlinkDuringShootingEnemy = currentEnemyInCrosshair.IsAtFullHealth() ? true : false;
                }
                else if (hit.transform.gameObject.GetInstanceID() != currentEnemyInCrosshair.gameObject.GetInstanceID() && tempEb.IsDestroyed())
                {
                    notBlinkDuringShootingEnemy = false;
                    currentEnemyInCrosshair = null;
                }
                else if (hit.transform.gameObject.GetInstanceID() == currentEnemyInCrosshair.gameObject.GetInstanceID() && tempEb.IsDestroyed())
                {
                    notBlinkDuringShootingEnemy = false;
                    currentEnemyInCrosshair = null;
                }
            }
            else if (currentEnemyInCrosshair == null)
            {
                if (!tempEb.IsDestroyed())
                {
                    currentEnemyInCrosshair = tempEb;
                    notBlinkDuringShootingEnemy = currentEnemyInCrosshair.IsAtFullHealth() ? true : false;
                }
                else
                {
                    notBlinkDuringShootingEnemy = false;
                    currentEnemyInCrosshair = null;
                }
            }
        }
        else
        {
            notBlinkDuringShootingEnemy = false;
            currentEnemyInCrosshair = null;
        }
        bool isCurrentEnemyNotNull = currentEnemyInCrosshair != null;

        // check eye tracker input
        bool eyeTrackerRunning = EyeTrackerController.GetDeviceStatus();
        bool ssvepRunning = InputUDP.GetInputAvailableStatus();
        bool blinkToAim = false;
        bool blinkToChangeGun = false;
        bool blinkToUseSkill = false;
        bool blinkToChangeMode = false;
        /*bool gazeInGunPanel = false;
        bool gazeInSkillPanel = false;*/
        bool gazeInCoreStimulus = false;
        bool gazeInSkipStimulus = false;
        bool gazeInEnemyStimulus = false;
        bool gazeToActivateCoreCommand = false;
        bool gazeToActivateSkipCommand = false;
        bool gazeToActivateShootCommand = false;
        bool pressToActivateSkipCommand = false;
        float unscaledDeltaTime = Time.deltaTime / Time.timeScale;
        BlinkStatus blinkStatus = EyeTrackerController.GetBlinkStatus();
        Vector2 gazePoint = Vector2.zero;
        bci2000Input.color = ssvepRunning ? greenColor : redColor;
        if (eyeTrackerRunning)
        {
            gazePoint = EyeTrackerController.GetCurrentGazePoint();
            gazeInCoreStimulus = IsPointInArea(gazePoint, coreCommandStimulusAreaCorners) && h_CoreStimulusController.IsFlickering();
            gazeInSkipStimulus = IsPointInArea(gazePoint, skipCommandStimulusAreaCorners) && h_SkipStimulusController.IsFlickering();
            gazeInEnemyStimulus = IsPointInArea(gazePoint, shootCommandStimulusAreaCorners) && rayHitEnemy && scoping;
            StimulusHighlightHandler(gazeInCoreStimulus, gazeInSkipStimulus);
            if (calibrating)
            {
                if (blinkStatus.oneEyedBlink || blinkStatus.twoEyedBlink)
                {
                    notBlinkDuringShootingEnemy = false;
                    timeSinceLastBlink = 0f;
                }
                else
                {
                    timeSinceLastBlink += unscaledDeltaTime;
                    /*if (timeSinceLastBlink >= EyeTrackerController.GetValidDurationSinceLastBlink())
                    {
                        if (gameController.IncreaseObjectiveCounter("STEP1", 3))
                        {
                            timeSinceLastBlink = 0f;
                        }
                    }*/
                }
            }
            if (blinkStatus.oneEyedBlink || blinkStatus.twoEyedBlink)
            {
                if (!ssvepRunning)
                {
                    stimulusGazeDuration = 0f;
                }
                if (blinkStatus.validOneEyedBlink)
                {
                    Vector2 blinkPoint = EyeTrackerController.GetBlinkPoint();
                    bool blinkInChangeGunAndSkillCA = IsPointInArea(blinkPoint, gunAndSkillCommandAreaCorners);
                    bool blinkInGunModeCA = IsPointInArea(blinkPoint, gunModeCommandAreaCorners);
                    bool blinkInGunModeAimCA = IsPointInArea(blinkPoint, gunModeAimCommandAreaCorners);
                    if (blinkInChangeGunAndSkillCA)
                    {
                        float d1 = Vector2.Distance(blinkPoint, gunPanelPosition);
                        float d2 = Vector3.Distance(blinkPoint, skillPanelPosition);
                        gameController.IncreaseBlinkInsideHudCommandCount();
                        if (d1 <= d2)
                        {
                            blinkToChangeGun = true;
                        }
                        else if (d2 < d1)
                        {
                            blinkToUseSkill = true;
                        }
                    }
                    else if ((blinkInGunModeCA && !g_Aiming) || (blinkInGunModeAimCA && g_Aiming))
                    {
                        blinkToChangeMode = true;
                        gameController.IncreaseBlinkOutsideHudCommandCount();
                    }
                    else
                    {
                        gameController.IncreaseBlinkOutsideHudCommandCount();
                        if (blinkStatus.left)
                        {
                            blinkToAim = true;
                        }
                        else
                        {
                            blinkToUseSkill = true;
                        }
                    }
                }
            }
            else if (!ssvepRunning)
            {
                bool tempGazeInEnemyStimulus = gazeInEnemyStimulus && isCurrentEnemyNotNull;
                if (gazeInCoreStimulus || gazeInSkipStimulus || tempGazeInEnemyStimulus)
                {
                    stimulusGazeDuration += unscaledDeltaTime;
                    if (stimulusGazeDuration >= EyeTrackerController.GetValidGazeDuration())
                    {
                        if (gazeInCoreStimulus)
                        {
                            gazeToActivateCoreCommand = true;
                        }
                        else if (gazeInSkipStimulus)
                        {
                            gazeToActivateSkipCommand = true;
                        }
                        else if (tempGazeInEnemyStimulus)
                        {
                            gazeToActivateShootCommand = true;
                        }
                    }
                }
                else
                {
                    stimulusGazeDuration = 0f;
                }
            }
            else if (ssvepRunning && (classifying || calibrating))
            {
                bool tempGazeInEnemyStimulus = gazeInEnemyStimulus && isCurrentEnemyNotNull;
                if (gazeInCoreStimulus)
                {
                    CurrentGazeZoneSetter("CORE", unscaledDeltaTime);
                }
                else if (gazeInSkipStimulus)
                {
                    CurrentGazeZoneSetter("SKIP", unscaledDeltaTime);
                }
                else if (tempGazeInEnemyStimulus)
                {
                    CurrentGazeZoneSetter("ENEMY", unscaledDeltaTime);
                }
                else
                {
                    CurrentGazeZoneSetter("NONE", unscaledDeltaTime);
                }
                if (currentGazeZone != "NONE")
                {
                    if (classifying)
                    {
                        bool ssvepReceived = false;
                        bool tempGazeInCoreStimulus = gazeInCoreStimulus && currentGazeZone == "CORE";
                        InputObject bciInput = InputUDP.GetNewBufferedInput();
                        stimulusGazeDuration += unscaledDeltaTime;
                        if (bciInput.input != "NULL")
                        {
                            if (tempGazeInCoreStimulus)
                            {
                                if (gameController.IsAlreadyStartFixingCore())
                                {
                                    InputUDP.LowerThreshold();
                                }
                                else
                                {
                                    InputUDP.ResetThreshold();
                                }
                            }
                            else
                            {
                                InputUDP.ResetThreshold();
                            }
                            ssvepReceived = isSSVEPdetected(bciInput);
                            previousSsvepInput = ssvepReceived;
                        }
                        else
                        {
                            ssvepReceived = tempGazeInCoreStimulus ? previousSsvepInput : false;
                        }
                        if (ssvepReceived)
                        {

                            if (tempGazeInCoreStimulus)
                            {
                                gazeToActivateCoreCommand = true;
                                savedStimulusGazeDuration = stimulusGazeDuration;
                                if (gameController.CanActivateLaserFence())
                                {
                                    gameController.UpdateSsvepCoreCommandDelay(stimulusGazeDuration);
                                }
                            }
                            else if (gazeInSkipStimulus && currentGazeZone == "SKIP")
                            {
                                gazeToActivateSkipCommand = true;
                                gameController.UpdateSsvepCoreCommandDelay(stimulusGazeDuration);
                            }
                            else if (tempGazeInEnemyStimulus && currentGazeZone == "ENEMY")
                            {
                                gazeToActivateShootCommand = true;
                                gameController.UpdateSsvepShootCommandDelay(stimulusGazeDuration);
                            }
                            stimulusGazeDuration = 0f;
                        }
                        /*bool ssvepReceived = false;
                        bool tempGazeInCoreStimulus = gazeInCoreStimulus && currentGazeZone == "CORE";
                        string bciBufferedInput = InputUDP.GetNewBufferedInput();
                        stimulusGazeDuration += unscaledDeltaTime;
                        if (bciBufferedInput != "NULL")
                        {
                            ssvepReceived = isSSVEPdetected(bciBufferedInput);
                            previousSsvepInput = ssvepReceived;
                        }
                        else
                        {
                            ssvepReceived = tempGazeInCoreStimulus ? previousSsvepInput : false;
                        }
                        if (ssvepReceived)
                        {
                            
                            if (tempGazeInCoreStimulus)
                            {
                                gazeToActivateCoreCommand = true;
                                savedStimulusGazeDuration = stimulusGazeDuration;
                            }
                            else if (gazeInSkipStimulus && currentGazeZone == "SKIP")
                            {
                                gazeToActivateSkipCommand = true;
                                gameController.UpdateAccCommandDelay(stimulusGazeDuration);
                            }
                            else if (tempGazeInEnemyStimulus && currentGazeZone == "ENEMY")
                            {
                                gazeToActivateShootCommand = true;
                                gameController.UpdateAccCommandDelay(stimulusGazeDuration);
                            }
                            stimulusGazeDuration = 0f;
                        }*/
                    }
                    else if (calibrating)
                    {
                        if (gazeInCoreStimulus || gazeInSkipStimulus || tempGazeInEnemyStimulus)
                        {
                            stimulusGazeDuration += unscaledDeltaTime;
                        }
                        if (stimulusGazeDuration >= EyeTrackerController.GetValidGazeDuration(true) &&
                            timeSinceLastBlink >= EyeTrackerController.GetValidDurationSinceLastBlink())
                        {
                            if (gazeInCoreStimulus)
                            {
                                /*gameController.IncreaseObjectiveCounter("STEP3", 2);
                                gameController.IncreaseObjectiveCounter("STEP6", 2);*/
                                gameController.IncreaseObjectiveCounter("STEP2_2", 2);
                                gameController.IncreaseObjectiveCounter("STEP5_2", 2);
                            }
                            else if (gazeInSkipStimulus)
                            {
                                gameController.IncreaseObjectiveCounter("STEP0");
                                /*gameController.IncreaseObjectiveCounter("STEP4", 2);
                                gameController.IncreaseObjectiveCounter("STEP7", 2);*/
                                gameController.IncreaseObjectiveCounter("STEP3_2", 2);
                                gameController.IncreaseObjectiveCounter("STEP6_2", 2);
                            }
                            else if (tempGazeInEnemyStimulus)
                            {
                                if (gameController.CanShootEnemy())
                                {
                                    gazeToActivateShootCommand = true;
                                }
                                else
                                {
                                    gameController.IncreaseObjectiveCounter("STEP4_2", 3);
                                }
                            }
                            stimulusGazeDuration = 0f;
                        }
                    }
                }
                else
                {
                    stimulusGazeDuration = 0f;
                }
            }
        }
        else
        {
            notBlinkDuringShootingEnemy = false;
            if (Input.GetKey(KeyCode.V) && h_SkipStimulusController.IsFlickering())
            {
                skipPressDuration += unscaledDeltaTime;
                if (skipPressDuration >= EyeTrackerController.GetValidGazeDuration())
                {
                    pressToActivateSkipCommand = true;
                }
            }
            else
            {
                skipPressDuration = 0f;
            }
        }

        // trigger slow motion
        if (eyeTrackerRunning)
        {
            if ((scoping && rayHitEnemy && isCurrentEnemyNotNull) || 
                (gazeInCoreStimulus && !gameController.CanFixCore()))
            {
                Time.timeScale = GameController.slowedTimeScale;
            }
            else if (!GameController.IsInWaveCompletedState() && Time.timeScale != GameController.defaultTimeScale)
            {
                Time.timeScale = GameController.defaultTimeScale;
            }
        }

        // rotate view & scope kick
        if (scoping)
        {
            if (g_KickingUp)
            {
                float scopeRecoilUp = g_SniperRifleController.GetScopeRecoilUp();
                float oldAcc = g_ScopeKickUpAcc;
                g_ScopeKickUpAcc = g_ScopeKickUpAcc >= (scopeRecoilUp * 2f) ? g_ScopeKickUpAcc : g_ScopeKickUpAcc + scopeRecoilUp;
                if (g_ScopeKickUpAcc - oldAcc > 0f)
                {
                    m_MouseLook.LookRotation(transform, m_Camera.transform, 1f, scopeRecoilUp);
                }
            }
            else if (!g_KickingUp && g_ScopeKickUpAcc > 0.001f)
            {
                float scopeRecoilDown = g_SniperRifleController.GetScopeRecoilDown();
                g_ScopeKickUpAcc -= scopeRecoilDown;
                m_MouseLook.LookRotation(transform, m_Camera.transform, 1f ,-scopeRecoilDown);
            }
            else
            {
                m_MouseLook.LookRotation(transform, m_Camera.transform, 0.4f);
            }
        }
        else
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }

        // use skill
        bool useSkillCommandIssued = eyeTrackerRunning ? (blinkToUseSkill && Time.time > s_SkillAvailableTime) : 
            (Input.GetKeyDown(KeyCode.F) && Time.time > s_SkillAvailableTime);
        if (useSkillCommandIssued)
        {
            bool useSkillSuccessfully = false;
            if (lockedOnEnemy != null)
            {
                if (!lockedOnEnemy.IsDestroyed())
                {
                    useSkillSuccessfully = true;
                    lockedOnEnemy.Freeze();
                }
                else
                {
                    useSkillSuccessfully = false;
                }
            }
            StartCoroutine(FlashSkillEffect(Time.time, useSkillSuccessfully));
        }

        // skip waiting state
        bool skipCommandIssued = eyeTrackerRunning ? gazeToActivateSkipCommand : pressToActivateSkipCommand;
        if (skipCommandIssued)
        {
            gameController.SkipWaitingState();
        }

        // fix core & activate laser
        if (eyeTrackerRunning)
        {
            if (gazeToActivateCoreCommand && gameController.CanActivateLaserFence())
            {
                gameController.ActivateLaserFence();
            }
            else if (gazeToActivateCoreCommand && gameController.CanFixCore())
            {
                if (fixingCoreAudioPrefab == null)
                {
                    fixingCoreAudioPrefab = Instantiate(f_FixingCoreAudioPrefab, Vector3.zero, Quaternion.identity);
                }
                gameController.IncreaseCoreHp(savedStimulusGazeDuration);
            }
            else
            {
                if (fixingCoreAudioPrefab != null)
                {
                    fixingCoreAudioPrefab.GetComponent<AudioPrefabFadeOutScript>().BeginFadeOut();
                    fixingCoreAudioPrefab = null;
                    if (gameController.IsCoreHpFull())
                    {
                        Instantiate(f_FinishedFixingCoreAudioPrefab, Vector3.zero, Quaternion.identity);
                    }
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q) && h_CoreStimulusController.IsFlickering() && gameController.CanActivateLaserFence())
            {
                gameController.ActivateLaserFence();
            }
            else if (Input.GetKey(KeyCode.Q) && h_CoreStimulusController.IsFlickering() && gameController.CanFixCore())
            {
                if (fixingCoreAudioPrefab == null)
                {
                    fixingCoreAudioPrefab = Instantiate(f_FixingCoreAudioPrefab, Vector3.zero, Quaternion.identity);
                }
                gameController.IncreaseCoreHp();
            }
            else
            {
                if (fixingCoreAudioPrefab != null)
                {
                    fixingCoreAudioPrefab.GetComponent<AudioPrefabFadeOutScript>().BeginFadeOut();
                    fixingCoreAudioPrefab = null;
                    if (gameController.IsCoreHpFull())
                    {
                        Instantiate(f_FinishedFixingCoreAudioPrefab, Vector3.zero, Quaternion.identity);
                    }
                }
            }
        }

        // change gun
        bool changeGunCommandIssued = eyeTrackerRunning ? (blinkToChangeGun && !g_Switching) : (Input.GetKeyDown(KeyCode.X) && !g_Switching);
        if (calibrating)
        {
            changeGunCommandIssued = false;
            if (forceToChangeGun)
            {
                changeGunCommandIssued = true;
                forceToChangeGun = false;
            }
        }
        if (changeGunCommandIssued)
        {
            BeginLoweringGun();
            return;
        }
        if (g_Switching)
        {
            if (usingSniper && g_SwitchingFromScope)
            {
                // freeze sniper lowering temp to fade out scope
                if (g_SniperRifleController.IsScopeActive())
                {
                    g_HolsterStartTime = Time.time;
                    g_AimingStartTime = Time.time;
                }
                float time = Time.time - g_AimingStartTime;
                float distance = time * g_GunController.aimSpeed;
                float interpolatingDistanceFrac = g_InterpolatingDistance != 0f ? distance / g_InterpolatingDistance : 1f;
                if (g_SniperRifleController.IsScopeActive())
                {
                    float scopeFadingRatio = (Time.time - g_ScopeFadingStartTime) / 0.15f;
                    g_SniperRifleController.AdjustBlankAlpha(scopeFadingRatio > 1f ? 1f : scopeFadingRatio);
                    if (scopeFadingRatio > 1f)
                    {
                        g_SniperRifleController.TurnOffScope();
                        g_SniperRifleController.ShowGunParts();
                        postProcessingProfile.colorGrading.enabled = false;
                    }
                }
                else
                {
                    if (interpolatingDistanceFrac > 1f)
                    {
                        interpolatingDistanceFrac = 1f;
                        g_HolsterStartPosition = g_GunController.transform.localPosition;
                        g_HolsterStartAngle = g_GunController.transform.localRotation;
                        g_HolsterIntendedAngle = Quaternion.Euler(g_HolsterStartAngle.eulerAngles + new Vector3(40f, 0f, 10f));
                        g_SwitchingFromScope = false;
                    }
                    g_HolsterStartTime = Time.time;
                    g_GunController.transform.localPosition = Vector3.Lerp(g_AimingStartPosition, g_AimingIntendedPosition, interpolatingDistanceFrac);
                    g_SniperRifleController.AdjustBlankAlpha(g_AimingStartAlpha - (g_AlphaDelta * interpolatingDistanceFrac));
                    m_Camera.fieldOfView = g_AimingStartFOV + (g_FOVDelta * interpolatingDistanceFrac);
                }
            }
            if (g_DelayingGunSwitching)
            {
                if (Time.time - g_HolsterStartTime < g_HolsterDuration + 0.3f)
                {
                    return;
                }
                else
                {
                    g_DelayingGunSwitching = false;
                    BeginPullingUpGun();
                    return;
                }
            }
            float passedTimeRatio = (Time.time - g_HolsterStartTime) / g_HolsterDuration;
            if (passedTimeRatio >= 1f)
            {
                if (g_PreviousGun == g_CurrentGun)
                {
                    g_DelayingGunSwitching = true;
                }
                else
                {
                    g_BackingToNormalBobStartTime = Time.time;
                    g_BackingToNormalBob = true;
                    g_Switching = false;
                    g_PreviousGun = g_CurrentGun;
                }
                passedTimeRatio = 1f;
            }
            if (!g_SwitchingFromScope)
            {
                g_GunController.transform.localPosition = Vector3.Lerp(g_HolsterStartPosition, g_HolsterIntendedPosition, Mathf.Sin(passedTimeRatio * (Mathf.PI / 2f)));
                g_GunController.transform.localRotation = Quaternion.Lerp(g_HolsterStartAngle, g_HolsterIntendedAngle, Mathf.Sin(passedTimeRatio * (Mathf.PI / 2f)));
            }
            return;
        }

        // change gun mode
        bool changeModeCommandIssued = eyeTrackerRunning ? blinkToChangeMode : Input.GetKeyDown(KeyCode.R);
        if (changeModeCommandIssued)
        {
            g_GunController.SwitchMode();
        }

        // aim down sight
        if (g_AimInterpolating)
        {
            // freeze sniper lowering temp to fade out scope
            if (!g_Aiming && g_SniperRifleController.IsScopeActive())
            {
                g_AimingStartTime = Time.time;
            }
            float time = Time.time - g_AimingStartTime;
            float distance = time * g_GunController.aimSpeed;
            float interpolatingDistanceFrac = g_InterpolatingDistance != 0f ? distance / g_InterpolatingDistance : 1f;
            // assault rifle aiming
            if (interpolatingDistanceFrac > 1f)
            {
                interpolatingDistanceFrac = 1f;
                g_AimInterpolating = (usingSniper && g_Aiming) ? true : false;
                if (!g_Aiming)
                {
                    g_BackingToNormalBobStartTime = Time.time;
                    g_BackingToNormalBob = true;
                }
            }
            g_GunController.transform.localPosition = Vector3.Lerp(g_AimingStartPosition, g_AimingIntendedPosition, interpolatingDistanceFrac);
            // sniper rifle aiming
            if (usingSniper)
            {
                if (g_Aiming)
                {
                    if (interpolatingDistanceFrac < 1f)
                    {
                        g_SniperRifleController.AdjustBlankAlpha(g_AimingStartAlpha + (g_AlphaDelta * interpolatingDistanceFrac));
                        m_Camera.fieldOfView = g_AimingStartFOV - (g_FOVDelta * interpolatingDistanceFrac);
                        g_ScopeFadingStartTime = Time.time;
                    }
                    else
                    {
                        if (!g_SniperRifleController.IsScopeActive())
                        {
                            g_SniperRifleController.TurnOnScope();
                            postProcessingProfile.colorGrading.enabled = true;
                        }
                        g_SniperRifleController.HideGunParts();
                        float scopeFadingRatio = (Time.time - g_ScopeFadingStartTime) / 0.15f;
                        if (scopeFadingRatio > 1f)
                        {
                            scopeFadingRatio = 1f;
                            g_AimInterpolating = false;
                        }
                        g_SniperRifleController.AdjustBlankAlpha(1f - scopeFadingRatio);
                    }
                }
                else
                {
                    if (g_SniperRifleController.IsScopeActive())
                    {
                        float scopeFadingRatio = (Time.time - g_ScopeFadingStartTime) / 0.15f;
                        g_SniperRifleController.AdjustBlankAlpha(scopeFadingRatio > 1f ? 1f: scopeFadingRatio);
                        if (scopeFadingRatio > 1f)
                        {
                            g_SniperRifleController.TurnOffScope();
                            g_SniperRifleController.ShowGunParts();
                            g_AimingStartTime = Time.time;
                            postProcessingProfile.colorGrading.enabled = false;
                        }
                    }
                    else
                    {
                        g_SniperRifleController.AdjustBlankAlpha(g_AimingStartAlpha - (g_AlphaDelta * interpolatingDistanceFrac));
                        m_Camera.fieldOfView = g_AimingStartFOV + (g_FOVDelta * interpolatingDistanceFrac);
                    }
                }
            }
        }
        bool aimCommandIssued = false;
        bool exitAimCommandIssued = false;
        aimCommandIssued = eyeTrackerRunning ? (blinkToAim && !g_Aiming) : (Input.GetMouseButtonDown(1) && !g_Aiming);
        exitAimCommandIssued = eyeTrackerRunning ? (blinkToAim && g_Aiming) : (Input.GetMouseButtonDown(1) && g_Aiming);
        if (aimCommandIssued)
        {
            g_Aiming = true;
            g_AimInterpolating = true;
            g_IdleInterpolating = false;
            g_AimingStartTime = Time.time;
            if (usingSniper)
            {
                g_ScopeFadingStartTime = Time.time;
                g_AimingStartFOV = m_Camera.fieldOfView;
                g_AimingIntendedFOV = 10f;
                g_FOVDelta = Mathf.Abs(g_AimingIntendedFOV - g_AimingStartFOV);
                g_AimingStartAlpha = g_SniperRifleController.GetBlankAlpha();
                g_AimingIntendedAlpha = 1f;
                g_AlphaDelta = Mathf.Abs(g_AimingIntendedAlpha - g_AimingStartAlpha);
            }
            g_AimingStartPosition = g_GunController.transform.localPosition;
            g_AimingIntendedPosition = g_GunController.aimPosition;
            g_InterpolatingDistance = Vector3.Distance(g_AimingStartPosition, g_AimingIntendedPosition);
            g_GunController.SwitchCrosshair();
        }
        else if (exitAimCommandIssued)
        {
            g_Aiming = false;
            g_AimInterpolating = true;
            g_IdleInterpolating = false;
            g_AimingStartTime = Time.time;
            if (usingSniper)
            {
                g_ScopeFadingStartTime = Time.time;
                g_AimingStartFOV = m_Camera.fieldOfView;
                g_AimingIntendedFOV = 60f;
                g_FOVDelta = Mathf.Abs(g_AimingIntendedFOV - g_AimingStartFOV);
                g_AimingStartAlpha = g_SniperRifleController.GetBlankAlpha();
                g_AimingIntendedAlpha = 0f;
                g_AlphaDelta = Mathf.Abs(g_AimingIntendedAlpha - g_AimingStartAlpha);
            }
            g_AimingStartPosition = g_GunController.transform.localPosition;
            g_AimingIntendedPosition = g_OriginalPosition;
            g_InterpolatingDistance = Vector3.Distance(g_AimingStartPosition, g_AimingIntendedPosition);
            g_GunController.SwitchCrosshair();
        }

        // shooting & recoil
        bool shoot = false;
        if (eyeTrackerRunning)
        {
            if (gazeToActivateShootCommand)
            {
                shoot = true;
            }
            else if (g_CurrentGun == "AR" && rayHitEnemy)
            {
                shoot = isCurrentEnemyNotNull;
            }
        }
        else
        {
            shoot = g_CurrentGun == "AR" ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        }
        if (shoot && !g_AimInterpolating)
        {
            bool shootSuccessfully = g_GunController.Shoot();
            if (shootSuccessfully)
            {
                g_ScopeKickUpAcc = 0f;
                if (!g_AimInterpolating)
                {
                    g_Kicking = true;
                    g_KickingUp = true;
                    g_KickUpStartTime = Time.time;
                }
                if (!g_AimInterpolating && !g_Aiming)
                {
                    g_RecoilStartPosition = g_Kicking ? g_RecoilSavedIntendedPosition : g_GunController.transform.localPosition;
                    g_RecoilIntendedPosition = g_RecoilStartPosition + g_GunController.hipRecoil;
                }
                else if (!g_AimInterpolating && g_Aiming)
                {
                    g_RecoilStartPosition = g_GunController.aimPosition;
                    g_RecoilIntendedPosition = g_GunController.aimPosition + g_GunController.aimRecoil;
                }
            }
        }
        if (g_Kicking)
        {
            if (g_KickingUp)
            {
                float kickUpRatio = (Time.time - g_KickUpStartTime) / g_GunController.kickUpTime;
                if (Time.time - g_KickUpStartTime > g_GunController.kickUpTime)
                {
                    kickUpRatio = 1f;
                    g_KickingUp = false;
                    g_KickUpStartTime = Time.time;
                    g_RecoilSavedStartPosition = g_RecoilIntendedPosition;
                }
                if (!g_AimInterpolating)
                {
                    g_GunController.transform.localPosition = Vector3.Lerp(g_RecoilStartPosition, g_RecoilIntendedPosition, Mathf.Sin(kickUpRatio * (Mathf.PI / 2f)));
                }
            }
            else
            {
                float lowerRatio = (Time.time - g_KickUpStartTime) / g_GunController.lowerTime;
                if (Time.time - g_KickUpStartTime > g_GunController.lowerTime)
                {
                    lowerRatio = 1f;
                    g_Kicking = false;
                }
                if (!g_AimInterpolating)
                {
                    g_GunController.transform.localPosition = g_Aiming ? Vector3.Lerp(g_RecoilSavedStartPosition, g_GunController.aimPosition, Mathf.Sin(lowerRatio * (Mathf.PI / 2f))) :
                    Vector3.Lerp(g_RecoilSavedStartPosition, g_RecoilSavedIntendedPosition, Mathf.Sin(lowerRatio * (Mathf.PI / 2f)));
                }
            }
        }
        if (calibrating)
        {
            if (g_CurrentGun == "AR" && isCurrentEnemyNotNull && currentEnemyInCrosshair.IsDestroyed() && notBlinkDuringShootingEnemy)
            {
                //gameController.IncreaseObjectiveCounter("STEP2", 3);
                gameController.IncreaseObjectiveCounter("STEP1_2", 3);
            }
            else if (g_CurrentGun == "SR" && isCurrentEnemyNotNull && currentEnemyInCrosshair.IsDestroyed())
            {
                //gameController.IncreaseObjectiveCounter("STEP5", 1);
                gameController.IncreaseObjectiveCounter("STEP7_2", 1);
            }
        }

        // gun-bob
        if (!g_Aiming && !g_AimInterpolating)
        {
            Vector3 gunBobRightSwayPosition = g_GunBobRightSwayPosition;
            Vector3 gunBobLeftSwayPosition = g_GunBobLeftSwayPosition;
            if (g_BackingToNormalBob)
            {
                float backingToNormalBobRatio = (Time.time - g_BackingToNormalBobStartTime) / g_BackToNormalBobIn;
                if (backingToNormalBobRatio > 0.95f)
                {
                    g_BackingToNormalBob = false;
                    backingToNormalBobRatio = 1f;
                }
                float temp_GunbobAmountX = g_GunbobAmountX * backingToNormalBobRatio;
                float temp_GunbobAmountY = g_GunbobAmountY * backingToNormalBobRatio;
                gunBobLeftSwayPosition = g_OriginalPosition - new Vector3(temp_GunbobAmountX, temp_GunbobAmountY, 0f);
                gunBobRightSwayPosition = g_OriginalPosition + new Vector3(temp_GunbobAmountX, -temp_GunbobAmountY, 0f);
            }
            float piCoefficient = 0f;
            if (m_Moving && !g_IdleInterpolating)
            {
                g_IdleInterpolating = false;
                piCoefficient = (m_StepCycle - m_PreviousNextStep) / m_StepInterval;
                if (piCoefficient > 1f)
                {
                    piCoefficient = 1f;
                }
                else if (piCoefficient < 0f)
                {
                    piCoefficient = 0f;
                }
                g_RecoilSavedIntendedPosition = Vector3.Lerp(g_OriginalPosition, g_SwayRight ? gunBobRightSwayPosition :
                        gunBobLeftSwayPosition, Mathf.Sin(piCoefficient * Mathf.PI));
                if (!g_Kicking)
                {
                    g_GunController.transform.localPosition = g_RecoilSavedIntendedPosition;
                }
            }
            else
            {
                if (!m_Moving && !g_IdleInterpolating)
                {
                    if (g_GunController.transform.localPosition == g_OriginalPosition)
                    {
                        g_RecoilSavedIntendedPosition = g_OriginalPosition;
                        if (!g_Kicking)
                        {
                            g_GunController.transform.localPosition = g_OriginalPosition;
                        }
                        ResetGunBob();
                        return;
                    }
                    if (m_BackupPreviousStepCycle != 0f || m_BackupPreviousNextStep != 0f)
                    {
                        piCoefficient = (m_BackupPreviousStepCycle - m_BackupPreviousNextStep) / m_StepInterval;
                        g_IdleInterpolating = true;
                        if (piCoefficient <= 0.5f)
                        {
                            m_PreviousSpeed = -m_PreviousSpeed;
                        }
                    }
                }
                else if (g_IdleInterpolating)
                {
                    m_BackupPreviousStepCycle += m_PreviousSpeed;
                    piCoefficient = (m_BackupPreviousStepCycle - m_BackupPreviousNextStep) / m_StepInterval;
                    if (piCoefficient > 1f)
                    {
                        bool tempBackupSwayRight = g_BackupSwayRight;
                        piCoefficient = 1f;
                        g_IdleInterpolating = false;
                        ResetGunBob();
                        if (m_Moving)
                        {
                            g_SwayRight = !tempBackupSwayRight;
                        }
                    }
                    else if (piCoefficient < 0f)
                    {
                        bool tempBackupSwayRight = g_BackupSwayRight;
                        piCoefficient = 0f;
                        g_IdleInterpolating = false;
                        ResetGunBob();
                        if (m_Moving)
                        {
                            g_SwayRight = !tempBackupSwayRight;
                        }
                    }
                    g_RecoilSavedIntendedPosition = Vector3.Lerp(g_OriginalPosition, g_BackupSwayRight ? gunBobRightSwayPosition :
                        gunBobLeftSwayPosition, Mathf.Sin(piCoefficient * Mathf.PI));
                    if (!g_Kicking)
                    {
                        g_GunController.transform.localPosition = g_RecoilSavedIntendedPosition;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameController.IsPause())
        {
            return;
        }
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x*speed;
        m_MoveDir.z = desiredMove.z*speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;
        }
        else
        {
            m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

        ProgressStepCycle(speed);
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_Moving = true;
            if (g_CurrentGun == "AR")
            {
                g_AssaultRifleController.EnlargeCrosshair();
            }
            if (!g_IdleInterpolating)
            {
                if (!m_Moving)
                {
                    if (Time.time - m_LastTimeMoving > 0.3f)
                    {
                        PlayFootStepAudio();
                    }
                    g_SwayRight = false;
                    m_NextStep = m_StepInterval;
                    m_PreviousNextStep = 0f;
                    m_StepCycle = 0f;
                }
                m_PreviousSpeed = (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                    Time.fixedDeltaTime;
                m_StepCycle += m_PreviousSpeed;
            }
        }
        else
        {
            if (g_CurrentGun == "AR")
            {
                g_AssaultRifleController.ShrinkCrosshair();
            }
            if (m_Moving && !g_IdleInterpolating)
            {
                m_BackupPreviousStepCycle = m_StepCycle;
                m_BackupPreviousNextStep = m_PreviousNextStep;
                m_LastTimeMoving = Time.time;
                g_BackupSwayRight = g_SwayRight;
            }
            m_Moving = false;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }
            
        g_SwayRight = !g_SwayRight;
        m_PreviousNextStep = m_NextStep;
        m_NextStep = m_StepCycle + m_StepInterval;
        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        audioSource.clip = m_FootstepSounds[n];
        audioSource.PlayOneShot(audioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = audioSource.clip;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        if (g_Kicking && !g_Aiming && !g_AimInterpolating)
        {
            speed = speed * 0.6f;
        }
        else if (g_Aiming)
        {
            speed = speed * 0.4f;
        }
        if (Time.timeScale != 1f)
        {
            speed = speed * Time.timeScale;
        }
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }

    private void SetGunController(string gun)
    {
        if (gun == "AR")
        {
            g_CurrentGun = "AR";
            g_GunController = g_AssaultRifleController;
            Color color = g_AssaultRifleIcon.color;
            color.a = 1f;
            g_AssaultRifleIcon.color = color;
            color = g_SniperRifleIcon.color;
            color.a = 0.1f;
            g_SniperRifleIcon.color = color;
        }
        else
        {
            g_CurrentGun = "SR";
            g_GunController = g_SniperRifleController;
            Color color = g_SniperRifleIcon.color;
            color.a = 1f;
            g_SniperRifleIcon.color = color;
            color = g_AssaultRifleIcon.color;
            color.a = 0.1f;
            g_AssaultRifleIcon.color = color;
        }
    }

    private void ResetGunBob()
    {
        g_SwayRight = false;
        g_BackupSwayRight = false;
        m_NextStep = m_StepInterval;
        m_PreviousNextStep = 0f;
        m_StepCycle = 0f;
        m_BackupPreviousStepCycle = 0f;
        m_BackupPreviousNextStep = 0f;
    }

    private void BeginLoweringGun()
    {
        g_Kicking = false;
        g_KickingUp = false;
        g_Switching = true;
        g_DelayingGunSwitching = false;
        g_HolsterStartTime = Time.time;
        g_HolsterStartPosition = g_GunController.transform.localPosition;
        g_HolsterIntendedPosition = g_OriginalPosition - new Vector3(0f, 0.325f, 0.17f);
        g_HolsterStartAngle = g_GunController.transform.localRotation;
        g_HolsterIntendedAngle = Quaternion.Euler(g_HolsterStartAngle.eulerAngles + new Vector3(40f, 0f, 10f));
        g_GunController.HideCrosshair();

        if (g_CurrentGun == "SR" && (g_Aiming || g_AimInterpolating))
        {
            g_SwitchingFromScope = true;
            g_AimingStartTime = Time.time;
            g_ScopeFadingStartTime = Time.time;
            g_AimingStartFOV = m_Camera.fieldOfView;
            g_AimingIntendedFOV = 60f;
            g_FOVDelta = Mathf.Abs(g_AimingIntendedFOV - g_AimingStartFOV);
            g_AimingStartAlpha = g_SniperRifleController.GetBlankAlpha();
            g_AimingIntendedAlpha = 0f;
            g_AlphaDelta = Mathf.Abs(g_AimingIntendedAlpha - g_AimingStartAlpha);
            g_AimingStartPosition = g_GunController.transform.localPosition;
            g_AimingIntendedPosition = g_OriginalPosition;
            g_InterpolatingDistance = Vector3.Distance(g_AimingStartPosition, g_AimingIntendedPosition);
        }
        
        g_Aiming = false;
        g_AimInterpolating = false;
        g_IdleInterpolating = false;
    }

    private void BeginPullingUpGun()
    {
        g_GunController.HideCrosshair();
        if (g_CurrentGun == "AR")
        {
            g_PreviousGun = "AR";
            g_SniperRifleController.gameObject.SetActive(true);
            SetGunController("SR");
            g_AssaultRifleController.gameObject.SetActive(false);
        }
        else
        {
            g_PreviousGun = "SR";
            g_AssaultRifleController.gameObject.SetActive(true);
            SetGunController("AR");
            g_SniperRifleController.gameObject.SetActive(false);
        }
        g_HolsterStartTime = Time.time;
        g_HolsterStartPosition = g_OriginalPosition - new Vector3(0f, 0.275f, 0.17f);
        g_HolsterIntendedPosition = g_OriginalPosition;
        g_HolsterStartAngle = g_GunController.transform.localRotation;
        g_HolsterIntendedAngle = Quaternion.Euler(g_HolsterStartAngle.eulerAngles - new Vector3(40f, 0f, 10f));
        g_GunController.SwitchToDefaultCrosshair();
    }
    
    private float coreStimulusStartAlpha;
    private float skipStimulusStartAlpha;
    private float stimulusFadeAlpha = 0.35f;
    private bool coreStimulusFadingIn;
    private bool coreStimulusFadingOut;
    private bool skipStimulusFadingIn;
    private bool skipStimulusFadingOut;
    private void StimulusHighlightHandler(bool gazeInCoreStimulus, bool gazeInSkipStimulus)
    {
        if ((gazeInCoreStimulus || gazeInSkipStimulus) && !highlightOn && !highlightingStimulus)
        {
            highlightStartAlpha = h_StimulusHighlightBlank.color.a;
            coreStimulusStartAlpha = h_CoreStimulusController.GetAlpha();
            skipStimulusStartAlpha = h_SkipStimulusController.GetAlpha();
            highlightStartFocus = postProcessingProfile.depthOfField.settings.focusDistance;
            highlightDuration = ((highlightDefaultAlpha - highlightStartAlpha) / highlightDefaultAlpha) * highlightDefaultDuration;
            postProcessingProfile.depthOfField.enabled = true;
            highlightStartTime = Time.time;
            highlightingStimulus = true;
            turningOffHighlight = false;
            if (gazeInCoreStimulus/* && h_SkipStimulusController.IsFlickering()*/)
            {
                coreStimulusFadingIn = true;
                coreStimulusFadingOut = false;
                skipStimulusFadingIn = false;
                skipStimulusFadingOut = true;
            }
            else if (gazeInSkipStimulus/* && h_CoreStimulusController.IsFlickering()*/)
            {
                coreStimulusFadingIn = false;
                coreStimulusFadingOut = true;
                skipStimulusFadingIn = true;
                skipStimulusFadingOut = false;
            }
        }
        else if (!gazeInCoreStimulus && !gazeInSkipStimulus && (highlightingStimulus || highlightOn))
        {
            highlightStartAlpha = h_StimulusHighlightBlank.color.a;
            coreStimulusStartAlpha = h_CoreStimulusController.GetAlpha();
            skipStimulusStartAlpha = h_SkipStimulusController.GetAlpha();
            highlightStartFocus = postProcessingProfile.depthOfField.settings.focusDistance;
            highlightDuration = (highlightStartAlpha / highlightDefaultAlpha) * turnOffDefaultDuration;
            postProcessingProfile.depthOfField.enabled = true;
            highlightStartTime = Time.time;
            highlightOn = false;
            highlightingStimulus = false;
            turningOffHighlight = true;
            coreStimulusFadingIn = true;
            coreStimulusFadingOut = false;
            skipStimulusFadingIn = true;
            skipStimulusFadingOut = false;
        }
        else if ((gazeInCoreStimulus || gazeInSkipStimulus) && (highlightOn || highlightingStimulus))
        {
            if (gazeInCoreStimulus && !coreStimulusFadingIn)
            {
                coreStimulusFadingIn = true;
                coreStimulusFadingOut = false;
                skipStimulusFadingIn = false;
                skipStimulusFadingOut = true;
            }
            else if (gazeInSkipStimulus && !skipStimulusFadingIn)
            {
                coreStimulusFadingIn = false;
                coreStimulusFadingOut = true;
                skipStimulusFadingIn = true;
                skipStimulusFadingOut = false;
            }
        }
        if (highlightingStimulus && !highlightOn)
        {
            float aPlus = 0f;
            float fdMinus = 0f;
            float maxAPlus = highlightDefaultAlpha - highlightStartAlpha;
            float maxFdMinus = highlightStartFocus - 0.1f;
            aPlus = (Time.time - highlightStartTime) * maxAPlus / highlightDuration;
            fdMinus = (Time.time - highlightStartTime) * maxFdMinus / highlightDuration;
            bool nanCheck = float.IsNaN(fdMinus) || float.IsNaN(aPlus) || float.IsNaN(maxFdMinus) || float.IsNaN(maxAPlus);
            if (fdMinus >= maxFdMinus)
            {
                highlightOn = true;
                highlightingStimulus = false;
                aPlus = maxAPlus;
                fdMinus = maxFdMinus;
            }
            else if (nanCheck)
            {
                highlightOn = true;
                highlightingStimulus = false;
                stimulusHighlightBlankColor.a = highlightDefaultAlpha;
                h_StimulusHighlightBlank.color = stimulusHighlightBlankColor;
                ppSetting.focusDistance = 0.1f;
                postProcessingProfile.depthOfField.settings = ppSetting;
            }
            if (!nanCheck)
            {
                stimulusHighlightBlankColor.a = highlightStartAlpha + aPlus;
                h_StimulusHighlightBlank.color = stimulusHighlightBlankColor;
                ppSetting.focusDistance = highlightStartFocus - fdMinus;
                postProcessingProfile.depthOfField.settings = ppSetting;
            }
        }
        else if (turningOffHighlight)
        {
            float aMinus = 0f;
            float fdPlus = 0f;
            float maxAMinus = highlightStartAlpha;
            float maxFdPlus = highlightDefaultStartFocus - highlightStartFocus;
            aMinus = (Time.time - highlightStartTime) * maxAMinus / highlightDuration;
            fdPlus = (Time.time - highlightStartTime) * maxFdPlus / highlightDuration;
            bool nanCheck = float.IsNaN(fdPlus) || float.IsNaN(aMinus) || float.IsNaN(maxFdPlus) || float.IsNaN(maxAMinus);
            if (fdPlus >= maxFdPlus)
            {
                turningOffHighlight = false;
                aMinus = maxAMinus;
                fdPlus = maxFdPlus;
                postProcessingProfile.depthOfField.enabled = false;
            }
            else if (nanCheck)
            {
                turningOffHighlight = false;
                stimulusHighlightBlankColor.a = 0f;
                h_StimulusHighlightBlank.color = stimulusHighlightBlankColor;
                ppSetting.focusDistance = highlightDefaultStartFocus;
                postProcessingProfile.depthOfField.settings = ppSetting;
                postProcessingProfile.depthOfField.enabled = false;
            }
            if (!nanCheck)
            {
                stimulusHighlightBlankColor.a = highlightStartAlpha - aMinus;
                h_StimulusHighlightBlank.color = stimulusHighlightBlankColor;
                ppSetting.focusDistance = highlightStartFocus + fdPlus;
                postProcessingProfile.depthOfField.settings = ppSetting;
            }
        }
        if (coreStimulusFadingIn && !coreStimulusFadingOut)
        {
            float aPlus = 0f;
            float maxAPlus = 1f - coreStimulusStartAlpha;
            aPlus = (Time.time - highlightStartTime) * maxAPlus / highlightDuration;
            bool nanCheck = float.IsNaN(aPlus) || float.IsNaN(maxAPlus);
            if (aPlus >= maxAPlus)
            {
                coreStimulusFadingIn = false;
                aPlus = maxAPlus;
            }
            else if (nanCheck)
            {
                coreStimulusFadingIn = false;
                h_CoreStimulusController.SetAlpha(1f);
            }
            if (!nanCheck)
            {
                h_CoreStimulusController.SetAlpha(coreStimulusStartAlpha + aPlus);
            }
        }
        else if (!coreStimulusFadingIn && coreStimulusFadingOut)
        {
            float aMinus = 0f;
            float maxAMinus = coreStimulusStartAlpha - 0.2f;
            aMinus = (Time.time - highlightStartTime) * maxAMinus / highlightDuration;
            bool nanCheck = float.IsNaN(aMinus) || float.IsNaN(maxAMinus);
            if (aMinus >= maxAMinus)
            {
                coreStimulusFadingOut = false;
                aMinus = maxAMinus;
            }
            else if (nanCheck)
            {
                coreStimulusFadingOut = false;
                h_CoreStimulusController.SetAlpha(0.2f);
            }
            if (!nanCheck)
            {
                h_CoreStimulusController.SetAlpha(coreStimulusStartAlpha - aMinus);
            }
        }
        if (skipStimulusFadingIn && !skipStimulusFadingOut)
        {
            float aPlus = 0f;
            float maxAPlus = 1f - skipStimulusStartAlpha;
            aPlus = (Time.time - highlightStartTime) * maxAPlus / highlightDuration;
            bool nanCheck = float.IsNaN(aPlus) || float.IsNaN(maxAPlus);
            if (aPlus >= maxAPlus)
            {
                skipStimulusFadingIn = false;
                aPlus = maxAPlus;
            }
            else if (nanCheck)
            {
                skipStimulusFadingIn = false;
                h_SkipStimulusController.SetAlpha(1f);
            }
            if (!nanCheck)
            {
                h_SkipStimulusController.SetAlpha(skipStimulusStartAlpha + aPlus);
            }
        }
        else if (!skipStimulusFadingIn && skipStimulusFadingOut)
        {
            float aMinus = 0f;
            float maxAMinus = skipStimulusStartAlpha - 0.2f;
            aMinus = (Time.time - highlightStartTime) * maxAMinus / highlightDuration;
            bool nanCheck = float.IsNaN(aMinus) || float.IsNaN(maxAMinus);
            if (aMinus >= maxAMinus)
            {
                skipStimulusFadingOut = false;
                aMinus = maxAMinus;
            }
            else if (nanCheck)
            {
                skipStimulusFadingOut = false;
                h_SkipStimulusController.SetAlpha(0.2f);
            }
            if (!nanCheck)
            {
                h_SkipStimulusController.SetAlpha(skipStimulusStartAlpha - aMinus);
            }
        }
    }

    private IEnumerator FlashSkillEffect(float startTime, bool success)
    {
        float fadeInTime = 0f;
        float fadeOutTime = 0f;
        if (success)
        {
            Instantiate(s_SkillUseAudioPrefab, Vector3.zero, Quaternion.identity);
            fadeInTime = s_SuccessFadeInTime;
            fadeOutTime = s_SuccessFadeOutTime;
        }
        else
        {
            Instantiate(s_SkillMissAudioPrefab, Vector3.zero, Quaternion.identity);
            fadeInTime = s_FailFadeInTime;
            fadeOutTime = s_FailFadeOutTime;
        }
        float alpha = 0f;
        while (Time.time - startTime <= fadeInTime || alpha < 1f)
        {
            alpha = (Time.time - startTime) / fadeInTime;
            if (Time.time - startTime > fadeInTime)
            {
                alpha = 1f;
            }
            Color color = s_SkillCooldownOverlay.color;
            color.a = alpha;
            s_SkillCooldownOverlay.color = color;
            color = s_SkillUseEffect.color;
            color.a = alpha;
            s_SkillUseEffect.color = color;
            while (GameController.IsPause())
            {
                yield return null;
            }
            yield return null;
        }
        startTime = Time.time;
        StartCoroutine(ReduceSkillOverlay(success));
        while (Time.time - startTime <= fadeOutTime || alpha > 0f)
        {
            alpha = 1f - ((Time.time - startTime) / fadeOutTime);
            if (Time.time - startTime > fadeOutTime)
            {
                alpha = 0f;
            }
            Color color = s_SkillUseEffect.color;
            color.a = alpha;
            s_SkillUseEffect.color = color;
            while (GameController.IsPause())
            {
                yield return null;
            }
            yield return null;
        }
    }

    private IEnumerator ReduceSkillOverlay(bool success)
    {
        float ratio = 1f;
        float cooldown = success ? s_SkillCooldown : s_SkillCooldown / 2f;
        s_SkillAvailableTime = Time.time + cooldown;
        while (Time.time < s_SkillAvailableTime)
        {
            ratio = Mathf.Abs(s_SkillAvailableTime - Time.time) / cooldown;
            s_SkillCooldownOverlay.fillAmount = ratio;
            while (GameController.IsPause())
            {
                yield return null;
            }
            yield return null;
        }
        Color color = s_SkillCooldownOverlay.color;
        color.a = 0f;
        s_SkillCooldownOverlay.color = color;
        s_SkillCooldownOverlay.fillAmount = 1f;
    }

    private void PrepareCommandAreaCorners()
    {
        h_GunAndSkillCommandArea.rectTransform.GetWorldCorners(gunAndSkillCommandAreaCorners);
        h_GunModeCommandArea.rectTransform.GetWorldCorners(gunModeCommandAreaCorners);
        h_GunModeAimCommandArea.rectTransform.GetWorldCorners(gunModeAimCommandAreaCorners);
        h_CoreCommandArea.rectTransform.GetWorldCorners(coreCommandStimulusAreaCorners);
        h_SkipCommandArea.rectTransform.GetWorldCorners(skipCommandStimulusAreaCorners);
        h_ShootCommandArea.rectTransform.GetWorldCorners(shootCommandStimulusAreaCorners);

        int windowHeight = 0;
#if UNITY_EDITOR
        windowHeight = 911;
#else
        windowHeight = 1080;
#endif
        gunAndSkillCommandAreaCorners[0].y = windowHeight - gunAndSkillCommandAreaCorners[0].y;
        gunAndSkillCommandAreaCorners[1].y = windowHeight - gunAndSkillCommandAreaCorners[1].y;
        gunAndSkillCommandAreaCorners[2].y = windowHeight - gunAndSkillCommandAreaCorners[2].y;
        gunAndSkillCommandAreaCorners[3].y = windowHeight - gunAndSkillCommandAreaCorners[3].y;

        gunModeCommandAreaCorners[0].y = windowHeight - gunModeCommandAreaCorners[0].y;
        gunModeCommandAreaCorners[1].y = windowHeight - gunModeCommandAreaCorners[1].y;
        gunModeCommandAreaCorners[2].y = windowHeight - gunModeCommandAreaCorners[2].y;
        gunModeCommandAreaCorners[3].y = windowHeight - gunModeCommandAreaCorners[3].y;

        gunModeAimCommandAreaCorners[0].y = windowHeight - gunModeAimCommandAreaCorners[0].y;
        gunModeAimCommandAreaCorners[1].y = windowHeight - gunModeAimCommandAreaCorners[1].y;
        gunModeAimCommandAreaCorners[2].y = windowHeight - gunModeAimCommandAreaCorners[2].y;
        gunModeAimCommandAreaCorners[3].y = windowHeight - gunModeAimCommandAreaCorners[3].y;

        coreCommandStimulusAreaCorners[0].y = windowHeight - coreCommandStimulusAreaCorners[0].y;
        coreCommandStimulusAreaCorners[1].y = windowHeight - coreCommandStimulusAreaCorners[1].y;
        coreCommandStimulusAreaCorners[2].y = windowHeight - coreCommandStimulusAreaCorners[2].y;
        coreCommandStimulusAreaCorners[3].y = windowHeight - coreCommandStimulusAreaCorners[3].y;

        skipCommandStimulusAreaCorners[0].y = windowHeight - skipCommandStimulusAreaCorners[0].y;
        skipCommandStimulusAreaCorners[1].y = windowHeight - skipCommandStimulusAreaCorners[1].y;
        skipCommandStimulusAreaCorners[2].y = windowHeight - skipCommandStimulusAreaCorners[2].y;
        skipCommandStimulusAreaCorners[3].y = windowHeight - skipCommandStimulusAreaCorners[3].y;

        shootCommandStimulusAreaCorners[0].y = windowHeight - shootCommandStimulusAreaCorners[0].y;
        shootCommandStimulusAreaCorners[1].y = windowHeight - shootCommandStimulusAreaCorners[1].y;
        shootCommandStimulusAreaCorners[2].y = windowHeight - shootCommandStimulusAreaCorners[2].y;
        shootCommandStimulusAreaCorners[3].y = windowHeight - shootCommandStimulusAreaCorners[3].y;
    }

    private void SetUIPosition(RectTransform rectTransform, out Vector2 uiPosition)
    {
        Vector2 position = new Vector2(rectTransform.position.x, rectTransform.position.y);
#if UNITY_EDITOR 
        position.y = 911 - position.y;
#else
        position.y = 1080 - position.y;
#endif
        uiPosition = position;
    }

    private bool IsPointInArea(Vector2 blinkPoint, Vector3[] area)
    {
        int x = (int)blinkPoint.x;
        int y = (int)blinkPoint.y;
        if (x >= area[0].x && x <= area[2].x && y >= area[1].y && y <= area[0].y)
        {
            return true;
        }
        return false;
    }

    private bool isSSVEPdetected(InputObject inputObject)
    {
        int ssvepCounter = 0;
        string input = inputObject.input;
        float grad1 = inputObject.grad1;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i].Equals('1'))
            {
                ssvepCounter++;
            }
        }
        if (ssvepCounter >= InputUDP.GetThreshold() /*&& grad1 < 0f*/)
        {
            return true;
        }
        return false;
    }
    /*private bool isSSVEPdetected(string input)
    {
        int ssvepCounter = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i].Equals('1'))
            {
                ssvepCounter++;
            }
        }
        if (ssvepCounter >= InputUDP.GetThreshold())
        {
            return true;
        }
        return false;
    }*/

    private void CurrentGazeZoneSetter(string gazeZone, float unscaledDeltaTime)
    {
        if (gazeZone != "NONE")
        {
            if (currentGazeZone == "NONE")
            {
                currentGazeZone = gazeZone;
                gazeShiftDuration = 0f;
                InputUDP.UnlockInput();
            }
            else if (currentGazeZone != gazeZone)
            {
                gazeShiftDuration += unscaledDeltaTime;
                if (gazeShiftDuration >= EyeTrackerController.GetBlinkDurationAllowed())
                {
                    currentGazeZone = gazeZone;
                    gazeShiftDuration = 0f;
                    InputUDP.UnlockInput();
                }
            }
            else
            {
                gazeShiftDuration = 0f;
            }
        }
        else
        {
            if (currentGazeZone != "NONE")
            {
                gazeShiftDuration += unscaledDeltaTime;
                if (gazeShiftDuration >= EyeTrackerController.GetBlinkDurationAllowed())
                {
                    currentGazeZone = gazeZone;
                    gazeShiftDuration = 0f;
                    InputUDP.LockInput();
                }
            }
        }
    }

    public bool IsAiming()
    {
        return g_Aiming;
    }

    public void EnablePauseDofEffect()
    {
        ppSetting.focusDistance = 0.1f;
        postProcessingProfile.depthOfField.settings = ppSetting;
        postProcessingProfile.depthOfField.enabled = true;
    }

    public void DisablePauseDofEffect()
    {
        if (!highlightingStimulus && !highlightOn && !turningOffHighlight)
        {
            ppSetting.focusDistance = 4f;
            postProcessingProfile.depthOfField.settings = ppSetting;
            postProcessingProfile.depthOfField.enabled = false;
        }
    }

    public void EnableStimulusHighlightBg()
    {
        h_StimulusHighlightBlank.gameObject.SetActive(true);
    }

    public void DisableStimulusHighlightBg()
    {
        h_StimulusHighlightBlank.gameObject.SetActive(false);
    }

    public void SetLockonEnemy(EnemyBehavior enemy)
    {
        lockedOnEnemy = enemy;
    }

    public void ClearLockonEnemy()
    {
        lockedOnEnemy = null;
    }
    
    public void StartSkipStimulus()
    {
        h_SkipStimulusController.StartFlickering();
    }

    public void StopSkipStimulus()
    {
        h_SkipStimulusController.StopFlickering();
    }

    public void ForceToChangeGun()
    {
        forceToChangeGun = true;
    }

    public Vector3 GetCameraPosition()
    {
        return m_Camera.transform.position;
    }

    public Vector3 GetLookingDirection()
    {
        return m_Camera.transform.forward;
    }
}
