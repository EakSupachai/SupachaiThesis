using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLightController : MonoBehaviour
{
    [SerializeField] private GameObject menuLight;
    [SerializeField] private Transform startPointTransform;
    [SerializeField] private Transform endPointTransform;
    [SerializeField] private float speed;
    [SerializeField] private float period;

    private float periodStart;
    private float maxDistance;
    private Vector3 startPoint;
    private Vector3 endPoint;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        startPoint = startPointTransform.position;
        endPoint = endPointTransform.position;
        menuLight.transform.position = startPoint;
        maxDistance = Vector3.Distance(endPoint, startPoint);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < periodStart)
        {
            return;
        }

        float time = Time.time - periodStart;
        float distance = time * speed;
        float distanceRatio = distance >= maxDistance ? 1f : distance / maxDistance;
        menuLight.transform.position = Vector3.Lerp(startPoint, endPoint, distanceRatio);
        
        if (distanceRatio >= 1f)
        {
            periodStart = Time.time + period;
        }
    }
}
