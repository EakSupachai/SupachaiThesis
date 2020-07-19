using UnityEngine;
using UnityEngine.UI;

public class AssaultRifleController : GunController
{
    // default hip recoil (0f, 0.01f, -0.02f)
    // default aim recoil (0f, 0.004f, -0.015f)
    // default aim position (0, -0.09f, 0.5f)
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Image aimCrosshair;
    [SerializeField] private Image modeIcon;
    [SerializeField] private Sprite yellowModeSprite;
    [SerializeField] private Sprite orangeModeSprite;
    [SerializeField] private Sprite redModeSprite;
    [SerializeField] private ParticleSystem redMuzzleFlash;
    [SerializeField] private ParticleSystem yellowMuzzleFlash;
    [SerializeField] private ParticleSystem orangeMuzzleFlash;
    [SerializeField] private GameObject redImpactParticle;
    [SerializeField] private GameObject yellowImpactParticle;
    [SerializeField] private GameObject orangeImpactParticle;
    [SerializeField] private GameObject shotFiredAudioPrefab;
    [SerializeField] private int rpm = 540;
    [SerializeField] private int damage = 1;
    [SerializeField] private Material redLight;
    [SerializeField] private Material yellowLight;
    [SerializeField] private Material orangeLight;
    [SerializeField] private GameObject modeChangeAudioPrefab;

    private float spr;
    private float timeLastRound;
    private float aimSpreadRadius;
    private float aimSpreadRadiusSquare;
    private string currentMode;
    private GameObject typeDisplayC;
    private GameObject typeDisplayL;
    private GameObject typeDisplayR;
    private Animator crosshairAnimator;
    private RectTransform crosshairRectTransform;

    private Camera fpcCamera;

    private void Start()
    {
        spr = 60f / rpm;
        kickUpTime = spr * 0.3f;
        lowerTime = spr * 0.7f;
        timeLastRound = -1f;

        crosshairRectTransform = aimCrosshair.gameObject.GetComponent<RectTransform>();
        Vector2 size = Vector2.Scale(crosshairRectTransform.rect.size, crosshairRectTransform.lossyScale);
        Rect crosshairRect = new Rect((Vector2)crosshairRectTransform.position - (size * 0.5f), size);
        aimSpreadRadius = crosshairRect.width / 2f;
        aimSpreadRadiusSquare = Mathf.Pow(aimSpreadRadius, 2f);

        crosshairRectTransform = crosshair.GetComponent<RectTransform>();
        RectTransform subCrosshairRectTransform = crosshair.transform.Find("UpperLeft").gameObject.GetComponent<RectTransform>();
        subCrosshairRectTransform.sizeDelta = new Vector2(crosshairRectTransform.rect.width, crosshairRectTransform.rect.height);
        subCrosshairRectTransform = crosshair.transform.Find("UpperRight").gameObject.GetComponent<RectTransform>();
        subCrosshairRectTransform.sizeDelta = new Vector2(crosshairRectTransform.rect.width, crosshairRectTransform.rect.height);
        subCrosshairRectTransform = crosshair.transform.Find("BottomLeft").gameObject.GetComponent<RectTransform>();
        subCrosshairRectTransform.sizeDelta = new Vector2(crosshairRectTransform.rect.width, crosshairRectTransform.rect.height);
        subCrosshairRectTransform = crosshair.transform.Find("BottomRight").gameObject.GetComponent<RectTransform>();
        subCrosshairRectTransform.sizeDelta = new Vector2(crosshairRectTransform.rect.width, crosshairRectTransform.rect.height);

        fpcCamera = Camera.main;
        crosshair.SetActive(true);
        aimCrosshair.gameObject.SetActive(false);

        crosshairAnimator = crosshair.GetComponent<Animator>();

        currentMode = "ORANGE";
        modeIcon.sprite = orangeModeSprite;
        typeDisplayC = transform.Find("TypeDisplayC").gameObject;
        typeDisplayL = transform.Find("TypeDisplayL").gameObject;
        typeDisplayR = transform.Find("TypeDisplayR").gameObject;
        typeDisplayC.GetComponent<Renderer>().material = orangeLight;
        typeDisplayL.GetComponent<Renderer>().material = orangeLight;
        typeDisplayR.GetComponent<Renderer>().material = orangeLight;
    }

    public override bool Shoot ()
    {
        if (Time.time - timeLastRound >= spr)
        {
            timeLastRound = Time.time;
            GameObject impactParticle;
            if (currentMode == "RED")
            {
                redMuzzleFlash.Play();
                impactParticle = redImpactParticle;
            }
            else if (currentMode == "YELLOW")
            {
                yellowMuzzleFlash.Play();
                impactParticle = yellowImpactParticle;
            }
            else
            {
                orangeMuzzleFlash.Play();
                impactParticle = orangeImpactParticle;
            }
            float spreadRadius = 0f;
            float spreadRadiusSquare = 0f;
            if (aimCrosshair.gameObject.activeSelf)
            {
                spreadRadius = aimSpreadRadius;
                spreadRadiusSquare = aimSpreadRadiusSquare;
            }
            else
            {
                Vector2 size = Vector2.Scale(crosshairRectTransform.rect.size, crosshairRectTransform.lossyScale);
                Rect crosshairRect = new Rect((Vector2)crosshairRectTransform.position - (size * 0.5f), size);
                spreadRadius = crosshairRect.width / 2f;
                spreadRadiusSquare = Mathf.Pow(spreadRadius, 2f);
            }
            float spreadX = Random.Range(0f, 2 * spreadRadius) - spreadRadius;
            float spreadYRange = Mathf.Sqrt(spreadRadiusSquare - Mathf.Pow(spreadX, 2f));
            float spreadY = Random.Range(0f, 2 * spreadYRange) - spreadYRange;
            Ray ray = fpcCamera.ScreenPointToRay(new Vector3(Screen.width / 2f + spreadX, Screen.height / 2f + spreadY));
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                GameObject ip;
                ip = Instantiate(impactParticle, hit.point + (hit.normal * 0.05f), Quaternion.LookRotation(hit.normal));
                Destroy(ip, 0.5f);
                if (hit.transform.gameObject.tag == "Enemy")
                {
                    EnemyBehavior enemyBehavior = hit.transform.gameObject.GetComponent<EnemyBehavior>();
                    enemyBehavior.TakeDamage(damage, currentMode, fpcCamera.transform.position, hit.point);
                }
            }
            Instantiate(shotFiredAudioPrefab, transform.position, Quaternion.identity);
            return true;
        }
        return false;
    }

    public override void SwitchCrosshair()
    {
        if (crosshair.activeSelf)
        {
            crosshair.SetActive(false);
            aimCrosshair.gameObject.SetActive(true);
            return;
        }
        crosshair.SetActive(true);
        aimCrosshair.gameObject.SetActive(false);
    }

    public override void SwitchToDefaultCrosshair()
    {
        crosshair.SetActive(true);
        aimCrosshair.gameObject.SetActive(false);
    }

    public override void HideCrosshair()
    {
        crosshair.SetActive(false);
        aimCrosshair.gameObject.SetActive(false);
    }

    public override void SwitchMode(string mode)
    {
        if (mode == "AUTO")
        {
            if (currentMode == "RED")
            {
                mode = "ORANGE";
                modeIcon.sprite = orangeModeSprite;
            }
            else if (currentMode == "ORANGE") 
            {
                mode = "YELLOW";
                modeIcon.sprite = yellowModeSprite;
            }
            else if (currentMode == "YELLOW")
            {
                mode = "RED";
                modeIcon.sprite = redModeSprite;
            }
        }
        if (currentMode != mode)
        {
            Instantiate(modeChangeAudioPrefab, Vector3.zero, Quaternion.identity);
            if (mode == "RED")
            {
                currentMode = "RED";
                modeIcon.sprite = redModeSprite;
                typeDisplayC.GetComponent<Renderer>().material = redLight;
                typeDisplayL.GetComponent<Renderer>().material = redLight;
                typeDisplayR.GetComponent<Renderer>().material = redLight;
                return;
            }
            else if (mode == "YELLOW")
            {
                currentMode = "YELLOW";
                modeIcon.sprite = yellowModeSprite;
                typeDisplayC.GetComponent<Renderer>().material = yellowLight;
                typeDisplayL.GetComponent<Renderer>().material = yellowLight;
                typeDisplayR.GetComponent<Renderer>().material = yellowLight;
                return;
            }
            else if (mode == "ORANGE")
            {
                currentMode = "ORANGE";
                modeIcon.sprite = orangeModeSprite;
                typeDisplayC.GetComponent<Renderer>().material = orangeLight;
                typeDisplayL.GetComponent<Renderer>().material = orangeLight;
                typeDisplayR.GetComponent<Renderer>().material = orangeLight;
                return;
            }
        }
    }

    public void EnlargeCrosshair()
    {
        crosshairAnimator.SetBool("m_Moving", true);
    }

    public void ShrinkCrosshair()
    {
        crosshairAnimator.SetBool("m_Moving", false);
    }
}
