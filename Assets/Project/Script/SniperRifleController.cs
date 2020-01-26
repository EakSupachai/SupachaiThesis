using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SniperRifleController : GunController
{
    // default hip recoil (0f, 0.03f, -0.06f)
    // default aim recoil (0f, 0.03f, -0.06f)
    // default aim position (0, -0.072f, 0.41f or 0.35f)
    [SerializeField] private Image scope;
    [SerializeField] private Image scopeCrosshair;
    [SerializeField] private Image blank;
    [SerializeField] private ParticleSystem orangeMuzzleFlash;
    [SerializeField] private GameObject orangeImpactParticle;
    [SerializeField] private GameObject shotFiredAudioPrefab;
    [SerializeField] private SSVEPStimulusController crosshairStimulusController;
    [SerializeField] private int rpm = 120;
    [SerializeField] private int damage = 50;
    [SerializeField] private float scopeRecoilUp = 5.4f;
    [SerializeField] private float scopeRecoilDown = 0.6f;

    private float spr;
    private float timeLastRound;
    private bool isRaycastHit;
    private List<GameObject> gunParts;
    private EnemyBehavior currentEnemyBehavior;
    private Vector3 impactPoint;
    private Vector3 impactNormal;

    private Camera fpcCamera;

    private void Start()
    {
        spr = 60f / rpm;
        kickUpTime = spr * 0.07f;
        lowerTime = spr * 0.84f;
        timeLastRound = -1f;
        fpcCamera = Camera.main;
        gunParts = new List<GameObject>();
        foreach (Transform gunPart in transform)
        {
            if (gunPart.gameObject.tag == "Gun Part")
            {
                gunParts.Add(gunPart.gameObject);
            }
        }
    }

    private void Update()
    {
        if (GameController.IsPause() || GameController.IsGameOver())
        {
            if (GameModeRecorder.shootingStimulusMode == 1 && crosshairStimulusController.IsFlickering())
            {
                crosshairStimulusController.StopFlickering();
            }
            return;
        }
        Ray ray = fpcCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            isRaycastHit = true;
            impactPoint = hit.point;
            impactNormal = hit.normal;
            if (IsScopeActive())
            {
                EnemyBehavior enemyBehavior = hit.transform.gameObject.GetComponent<EnemyBehavior>();
                if (enemyBehavior != null && !enemyBehavior.IsDestroyed())
                {
                    StartCrosshairFlickering();
                    if (currentEnemyBehavior == null)
                    {
                        enemyBehavior.StartFlickering();
                        currentEnemyBehavior = enemyBehavior;
                    }
                    else
                    {
                        if (currentEnemyBehavior.gameObject.GetInstanceID() != enemyBehavior.gameObject.GetInstanceID())
                        {
                            currentEnemyBehavior.StopFlickering();
                            enemyBehavior.StartFlickering();
                            currentEnemyBehavior = enemyBehavior;
                        }
                    }
                }
                else
                {
                    StopCrosshairFlickering();
                    ClearEnemyBehavior();
                }
            }
        }
        else
        {
            isRaycastHit = false;
            ClearEnemyBehavior();
        }
    }

    private void ClearEnemyBehavior()
    {
        if (currentEnemyBehavior != null)
        {
            currentEnemyBehavior.StopFlickering();
            currentEnemyBehavior = null;
        }
    }

    private void StartCrosshairFlickering()
    {
        if (GameModeRecorder.shootingStimulusMode == 1 && !crosshairStimulusController.IsFlickering())
        {
            crosshairStimulusController.StartFlickering();
        }
    }

    private void StopCrosshairFlickering()
    {
        if (GameModeRecorder.shootingStimulusMode == 1 && crosshairStimulusController.IsFlickering())
        {
            crosshairStimulusController.StopFlickering();
        }
    }

    public override bool Shoot()
    {
        if (Time.time - timeLastRound >= spr && scope.gameObject.activeSelf)
        {
            timeLastRound = Time.time;
            if (isRaycastHit)
            {
                GameObject impactEffect;
                impactEffect = Instantiate(orangeImpactParticle, impactPoint + (impactNormal * 0.05f), Quaternion.LookRotation(impactNormal));
                Destroy(impactEffect, 0.5f);
                if (currentEnemyBehavior != null)
                {
                    currentEnemyBehavior.TakeDamage(damage, "ORANGE", fpcCamera.transform.position, impactPoint);
                }
            }
            Instantiate(shotFiredAudioPrefab, transform.position, Quaternion.identity);
            return true;
        }
        return false;
    }

    public override void SwitchCrosshair() {}

    public override void SwitchToDefaultCrosshair() {}

    public override void HideCrosshair() {}

    public override void SwitchMode() {}

    public void AdjustBlankAlpha(float alpha)
    {
        if (alpha > 1f)
        {
            alpha = 1f;
        }
        else if (alpha < 0f)
        {
            alpha = 0f;
        }
        Color color = blank.color;
        color.a = alpha;
        blank.color = color;
    }

    public void TurnOnScope()
    {
        scope.gameObject.SetActive(true);
        scopeCrosshair.gameObject.SetActive(true);
    }

    public void TurnOffScope()
    {
        scope.gameObject.SetActive(false);
        scopeCrosshair.gameObject.SetActive(false);
        //crosshairStimulusController.StopFlickering();
        StopCrosshairFlickering();
        ClearEnemyBehavior();
    }

    public void ShowGunParts()
    {
        foreach (GameObject gunPart in gunParts)
        {
            gunPart.SetActive(true);
        }
    }

    public void HideGunParts()
    {
        foreach (GameObject gunPart in gunParts)
        {
            gunPart.SetActive(false);
        }
    }

    public float GetBlankAlpha()
    {
        return blank.color.a;
    }

    public float GetScopeRecoilUp()
    {
        return scopeRecoilUp;
    }

    public float GetScopeRecoilDown()
    {
        return scopeRecoilDown;
    }

    public bool IsScopeActive()
    {
        return scope.gameObject.activeSelf;
    }
}
