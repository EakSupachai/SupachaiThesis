using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    // type 1 = small, 2 = medium, 3 = large
    [SerializeField] private int type = 1;
    [SerializeField] private float fullHp  = 15;
    [SerializeField] private float speed = 5;
    [SerializeField] private float freezeTime = 7f;
    [SerializeField] private float explosionScale = 1f;
    [SerializeField] private bool isRedType;
    [SerializeField] private Material redTypeMaterial;
    [SerializeField] private Material yellowTypeMaterial;
    [SerializeField] private Material blackStimulusMaterial;
    [SerializeField] private Material whiteStimulusMaterial;
    [SerializeField] private Material thrustMaterial;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject explosionAudioPrefab;
    [SerializeField] private GameObject scoreCanvas;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image freezeBar;

    private bool flickering;
    private bool materialFlag;
    private bool forceFlag;
    private bool freezing;
    private bool fadeInFreezing;
    private bool fadeInOuterFreezing;
    private bool fadeOutOuterFreezing;
    private bool fadeOutFreezing;
    private bool passedDecelerationPoint;
    private int frameCount;
    private int defaultLayer;
    private int eyeLayer;
    private float hp;
    private float fadeOutTime;
    private float freezeStartTime;
    private float freezeEffectFadeInTime;
    private float freezeEffectFadeOutTime;
    private float forceMagnitude;
    private float spawnTime;
    private float distanceToDestination;
    private float distanceToDeceleration;
    private GameController gameController;
    private AudioSource audioSource;
    private Vector3 startPosition;
    private Vector3 destinationPosition;
    private Vector3 decelerationPosition;
    private GameObject freezeEffect;
    private GameObject outerFreezeEffect;
    private Transform frontImpactPoint;
    private Transform backImpactPoint;
    private Transform scoreSpawnPoint;
    private ParticleSystem freezeEffectParticle;
    private ParticleSystem outerFreezeEffectParticle;
    private Vector3 forceDirection;
    private Vector3 forcePosition;
    private MeshRenderer bodyMeshRenderer;
    private List<MeshRenderer> meshRenderers;
    private List<Material> defaultMaterials;
    private List<ParticleSystemRenderer> thrusts;
    private Material defaultParticleMaterial;
    private Rigidbody body;

    private void Start()
    {
        frameCount = 1;
        defaultLayer = LayerMask.NameToLayer("Default");
        eyeLayer = LayerMask.NameToLayer("PostProcessing");
        hp = fullHp;
        fadeOutTime = 0.5f;
        freezeEffectFadeInTime = 0.034f;
        freezeEffectFadeOutTime = 0.6f;
        forceMagnitude = 100f;
        spawnTime = Time.time;
        startPosition = transform.position;
        destinationPosition = new Vector3(0f, 15f, 0f);
        freezeEffect = transform.Find("FreezeEffect").gameObject;
        freezeEffect.SetActive(false);
        freezeEffectParticle = freezeEffect.GetComponent<ParticleSystem>();
        outerFreezeEffect = transform.Find("OuterFreezeEffect").gameObject;
        outerFreezeEffect.SetActive(false);
        outerFreezeEffectParticle = outerFreezeEffect.GetComponent<ParticleSystem>();
        frontImpactPoint = transform.Find("FrontImpactPoint");
        backImpactPoint = transform.Find("BackImpactPoint");
        scoreSpawnPoint = transform.Find("ScoreSpawnPoint");
        meshRenderers = new List<MeshRenderer>();
        defaultMaterials = new List<Material>();
        thrusts = new List<ParticleSystemRenderer>();

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "Body")
            {
                bodyMeshRenderer = child.gameObject.GetComponent<MeshRenderer>();
            }
            if (child.childCount > 0)
            {
                foreach (Transform grandChild in child)
                {
                    InitializeDataList(grandChild);
                }
            }
            else
            {
                InitializeDataList(child);
            }
        }
        foreach (ParticleSystemRenderer ps in thrusts)
        {
            ps.material = thrustMaterial;
        }

        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        //Debug.Log(bodyMeshRenderer.isVisible);
        if (gameController != null && (GameController.IsPause() || GameController.IsGameOver()))
        {
            audioSource.Pause();
            return;
        }
        if (!audioSource.isPlaying && !IsDestroyed())
        {
            audioSource.UnPause();
        }
        if (canvas != null && hpBar != null && freezeBar != null)
        {
            Vector3 hpBarDirection = transform.position - gameController.GetPlayerCameraPosition();
            hpBarDirection.y = 0f;
            canvas.transform.rotation = Quaternion.LookRotation(hpBarDirection);
            hpBar.fillAmount = hp / fullHp;
            if (freezing)
            {
                float remainingFreezeTime = freezeTime - (Time.time - freezeStartTime);
                freezeBar.fillAmount = remainingFreezeTime / freezeTime;
            }
            else
            {
                freezeBar.fillAmount = 0f;
            }
        }
        if (!IsDestroyed() && decelerationPosition != Vector3.zero)
        {
            float timePassed = Time.time - spawnTime;
            float distanceRatio = 0f;
            if (passedDecelerationPoint)
            {
                distanceRatio = (speed * timePassed) / distanceToDestination;
                distanceRatio = distanceRatio > 1f ? 1 : distanceRatio;
                if (!freezing)
                {
                    transform.position = Vector3.Lerp(decelerationPosition, destinationPosition, distanceRatio);
                }
                else
                {
                    decelerationPosition = transform.position;
                    distanceToDestination = Vector3.Distance(decelerationPosition, destinationPosition);
                    spawnTime = Time.time;
                }
            }
            else
            {
                distanceRatio = (speed * timePassed * 10f) / distanceToDeceleration;
                distanceRatio = distanceRatio > 1f ? 1 : distanceRatio;
                if (!freezing)
                {
                    transform.position = Vector3.Lerp(startPosition, decelerationPosition, distanceRatio);
                }
                else
                {
                    startPosition = transform.position;
                    distanceToDeceleration = Vector3.Distance(startPosition, decelerationPosition);
                    spawnTime = Time.time;
                }
            }
        }
        if (flickering)
        {
            if (frameCount > 4)
            {
                frameCount = 0;
                foreach (MeshRenderer mr in meshRenderers)
                {
                    mr.material = materialFlag ? whiteStimulusMaterial : blackStimulusMaterial;
                }
                foreach (ParticleSystemRenderer ps in thrusts)
                {
                    Color color = ps.material.color;
                    if (materialFlag)
                    {
                        color.r = 1f;
                        color.g = 1f;
                        color.b = 1f;
                    }
                    else
                    {
                        color.r = 0f;
                        color.g = 0f;
                        color.b = 0f;
                    }
                    ps.material.color = color;
                }
                materialFlag = !materialFlag;
            }
            frameCount++;
        }
        if ((freezing && Time.time - freezeStartTime > freezeTime) || IsDestroyed())
        {
            if (IsDestroyed())
            {
                freezeEffect.SetActive(false);
                outerFreezeEffect.SetActive(false);
            }
            else
            {
                freezing = false;
                fadeInFreezing = false;
                fadeOutFreezing = true;
                freezeStartTime = Time.time;
            }
        }
        else if ((freezing && fadeInFreezing) || (!freezing && fadeOutFreezing))
        {
            var mm = freezeEffectParticle.main;
            var omm = outerFreezeEffectParticle.main;
            float alpha = 0f;
            Color color;
            if (fadeInFreezing && !fadeOutFreezing)
            {
                if (fadeInOuterFreezing)
                {
                    alpha = (Time.time - freezeStartTime) / freezeEffectFadeInTime;
                    if (alpha >= 1f)
                    {
                        alpha = 1f;
                        fadeInOuterFreezing = false;
                        fadeOutOuterFreezing = true;
                        freezeStartTime = Time.time;
                    }
                    color = new Color(0.7843137f, 1f, 1f, alpha);
                    omm.startColor = new ParticleSystem.MinMaxGradient(color);
                    color = new Color(0.2078431f, 0.9686275f, 1f, alpha);
                    mm.startColor = new ParticleSystem.MinMaxGradient(color);
                }
                else if (fadeOutOuterFreezing)
                {
                    alpha = 1f - ((Time.time - freezeStartTime) / freezeEffectFadeOutTime);
                    if (alpha <= 0f)
                    {
                        alpha = 0f;
                        fadeInFreezing = false;
                        fadeOutOuterFreezing = false;
                    }
                    color = new Color(0.7843137f, 1f, 1f, alpha);
                    omm.startColor = new ParticleSystem.MinMaxGradient(color);
                    if (alpha <= 0f)
                    {
                        outerFreezeEffect.SetActive(false);
                    }
                }
            }
            else if (!fadeInFreezing && fadeOutFreezing)
            {
                alpha = 1f - ((Time.time - freezeStartTime) / freezeEffectFadeOutTime);
                if (alpha <= 0f)
                {
                    alpha = 0f;
                    fadeOutFreezing = false;
                    freezeEffect.SetActive(false);
                }
                color = new Color(0.2078431f, 0.9686275f, 1f, alpha);
                mm.startColor = new ParticleSystem.MinMaxGradient(color);
            }
        }
    }

    private void FixedUpdate()
    {
        if (gameController != null && GameController.IsPause())
        {
            return;
        }
        if (IsDestroyed() && !forceFlag)
        {
            body.AddForceAtPosition(forceDirection * forceMagnitude, forcePosition);
            forceFlag = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Core")
        {
            if (!IsDestroyed())
            {
                hp = 0f;
                Explode(false);
                gameController.ReduceCoreHp(type);
            }
        }
        else if (other.gameObject.tag == "DecelerationPoint")
        {
            spawnTime = Time.time;
            passedDecelerationPoint = true;
        }
        else if (other.gameObject.tag == "Enemy")
        {
            EnemyBehavior eb = other.GetComponent<EnemyBehavior>();
            float distanceToCore = Vector3.Distance(transform.position, destinationPosition);
            float otherDistanceToCore = Vector3.Distance(other.transform.position, destinationPosition);
            Vector3 fd = Vector3.zero;
            Vector3 fp = Vector3.zero;
            if (distanceToCore < otherDistanceToCore)
            {
                fd = (destinationPosition - transform.position).normalized;
                fp = backImpactPoint.position;
            }
            else
            {
                fd = (transform.position - destinationPosition).normalized;
                fp = frontImpactPoint.position;
            }

            if (type > eb.type)
            {
                hp = hp - (fullHp / ((type - eb.type) + 1));
                if (hp <= 0f)
                {
                    hp = 0f;
                    forceDirection = fd;
                    forcePosition = fp;
                    Explode(true);
                }
            }
            else
            {
                hp = 0f;
                forceDirection = fd;
                forcePosition = fp;
                Explode(true);
            }
        }
    }

    private void Explode(bool showScore)
    {
        StopFlickering();
        audioSource.Pause();
        canvas.gameObject.SetActive(false);
        gameController.RemoveEnemyOnScreen();
        if (showScore)
        {
            gameController.AddScore(type);
            Vector3 scorePanelDirection = transform.position - gameController.GetPlayerCameraPosition();
            scorePanelDirection.y = 0f;
            GameObject score = Instantiate(scoreCanvas, scoreSpawnPoint.position, Quaternion.LookRotation(scorePanelDirection));
            Destroy(score, 2f);
        }
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameObject explosionAudio = Instantiate(explosionAudioPrefab, transform.position, Quaternion.identity);
        explosion.transform.localScale = new Vector3(explosionScale, explosionScale, explosionScale);
        Destroy(explosion, 5f);
        Destroy(explosionAudio, 2f);
        body.useGravity = true;
        body.isKinematic = false;
        foreach (MeshRenderer mr in meshRenderers)
        {
            Material m = mr.material;
            m.SetFloat("_Mode", 2);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }
        StartCoroutine("FadeOut", Time.time);
        foreach (ParticleSystemRenderer ps in thrusts)
        {
            ps.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOut(float startTime)
    {
        float alpha = 1f;
        while (Time.time - startTime < fadeOutTime || alpha > 0f)
        {
            alpha = 1f - ((Time.time - startTime) / fadeOutTime);
            foreach (MeshRenderer mr in meshRenderers)
            {
                Color color = mr.material.color;
                color.a = alpha;
                mr.material.color = color;
            }
            if (gameController != null && GameController.IsPause())
            {
                yield return null;
            }
            yield return null;
        }
        Destroy(this.gameObject);
    }

    private void InitializeDataList(Transform objectTransform)
    {
        MeshRenderer mr = objectTransform.gameObject.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            meshRenderers.Add(mr);
            if (objectTransform.gameObject.tag == "Eye")
            {
                if (isRedType)
                {
                    mr.material = redTypeMaterial;
                    defaultMaterials.Add(redTypeMaterial);
                }
                else
                {
                    mr.material = yellowTypeMaterial;
                    defaultMaterials.Add(yellowTypeMaterial);
                }
            }
            else
            {
                defaultMaterials.Add(mr.material);
            }
        }
        ParticleSystemRenderer ps = objectTransform.gameObject.GetComponent<ParticleSystemRenderer>();
        if (ps != null && (ps.gameObject.name != "FreezeEffect" && ps.gameObject.name != "OuterFreezeEffect"))
        {
            thrusts.Add(ps);
            defaultParticleMaterial = ps.gameObject.GetComponent<ParticleSystemRenderer>().material;
        }
    }

    public void TakeDamage(int damage, string ammoType, Vector3 cameraPosition, Vector3 impactPosition)
    {
        if (hp > 0)
        {
            if (isRedType)
            {
                hp = ammoType == "YELLOW" ? hp - (damage * 0.6f) : hp - damage;
            }
            else
            {
                hp = ammoType == "RED" ? hp - (damage * 0.6f) : hp - damage;
            }
            forceDirection = (impactPosition - cameraPosition).normalized;
            forcePosition = impactPosition;
            if (hp <= 0)
            {
                hp = 0f;
                Explode(true);
            }
            else
            {
                forceDirection = Vector3.zero;
                forcePosition = Vector3.zero;
            }
        }
    }

    public void Freeze()
    {
        freezing = true;
        fadeInFreezing = true;
        fadeInOuterFreezing = true;
        fadeOutFreezing = false;
        fadeOutOuterFreezing = false;
        freezeStartTime = Time.time;
        freezeEffect.SetActive(true);
        outerFreezeEffect.SetActive(true);
        var mm = freezeEffectParticle.main;
        Color color = new Color(0.2078431f, 0.9686275f, 1f, 0f);
        mm.startColor = new ParticleSystem.MinMaxGradient(color);
        var omm = outerFreezeEffectParticle.main;
        color = new Color(0.7843137f, 1f, 1f, 0f);
        omm.startColor = new ParticleSystem.MinMaxGradient(color);
    }

    public void StartFlickering()
    {
        if (!IsDestroyed() && !flickering)
        {
            frameCount = 1;
            flickering = true;
            materialFlag = false;
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.material = whiteStimulusMaterial;
                mr.receiveShadows = false;
                if (mr.gameObject.tag == "Eye")
                {
                    mr.gameObject.layer = defaultLayer;
                }
            }
            foreach (ParticleSystemRenderer ps in thrusts)
            {
                ps.material = defaultParticleMaterial;

            }
        }
    }

    public void StopFlickering()
    {
        if (flickering)
        {
            flickering = false;
            int index = 0;
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.material = defaultMaterials[index];
                mr.receiveShadows = true;
                if (mr.gameObject.tag == "Eye")
                {
                    mr.gameObject.layer = eyeLayer;
                }
                if (IsDestroyed())
                {
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                index++;
            }
            foreach (ParticleSystemRenderer ps in thrusts)
            {
                ps.material = thrustMaterial;

            }
        }
    }

    public bool IsDestroyed()
    {
        return hp <= 0;
    }

    public void GiveInstruction(GameController controller, Vector3 deceleration)
    {
        gameController = controller;
        decelerationPosition = deceleration;
        distanceToDeceleration = Vector3.Distance(startPosition, decelerationPosition);
        distanceToDestination = Vector3.Distance(decelerationPosition, destinationPosition);
        isRedType = Random.value > 0.5f;
    }

    private float angleToCamera = 0f;

    public bool IsInLockedOnVicinity()
    {
        Vector3 playerPosition = gameController.GetPlayerCameraPosition();
        Vector3 enemyDirection = transform.position - gameController.GetPlayerCameraPosition();
        angleToCamera = Vector3.Angle(gameController.GetPlayerLookingDirection(), enemyDirection);
        if (angleToCamera <= 20f)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerPosition, enemyDirection, out hit))
            {
                if (hit.transform.gameObject.tag == "Enemy")
                {
                    Debug.Log("---------------------------------------");
                    return true;
                }
            }
        }
        /*hpBarDirection.y = 0f;
        canvas.transform.rotation = Quaternion.LookRotation(hpBarDirection);
        hpBar.fillAmount = hp / fullHp;
        if (freezing)
        {
            float remainingFreezeTime = freezeTime - (Time.time - freezeStartTime);
            freezeBar.fillAmount = remainingFreezeTime / freezeTime;
        }
        else
        {
            freezeBar.fillAmount = 0f;
        }*/
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        return false;
    }

    public float GetAngleToCamera()
    {
        return angleToCamera;
        //return Vector3.Distance(transform.position, gameController.GetPlayerCameraPosition());
    }
}
