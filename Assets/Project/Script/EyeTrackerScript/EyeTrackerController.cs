using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.StreamEngine;

public class EyeTrackerController : MonoBehaviour
{
    [SerializeField] private Image tracker;

    private tobii_error_t result;
    private IntPtr deviceContext;

    private static bool deviceStatus = false;
    //private static bool blinkStatus = false;
    private static int leftBlinkStatusCount = 0;
    private static int rightBlinkStatusCount = 0;
    private static BlinkStatus blinkStatus = new BlinkStatus();
    private static Vector2 currentGazePoint = new Vector2();
    private static Vector2 blinkPoint = new Vector2();

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log(Screen.width);
        Debug.Log(Screen.height);
        deviceStatus = false;
        IntPtr apiContext;
        result = Interop.tobii_api_create(out apiContext, null);
        Debug.Log("Initialized API: " + (result == tobii_error_t.TOBII_ERROR_NO_ERROR));

        List<string> urls;
        result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
        Debug.Log("Found Device: " + (result == tobii_error_t.TOBII_ERROR_NO_ERROR));
        if (urls.Count == 0)
        {
            Debug.Log("Error: No device(s) found");
        }

        //IntPtr deviceContext;
        result = Interop.tobii_device_create(apiContext, urls[0], out deviceContext);
        Debug.Log("Create Device: " + (result == tobii_error_t.TOBII_ERROR_NO_ERROR));

        tobii_gaze_point_callback_t gpCallback = new tobii_gaze_point_callback_t(OnGazePoint);
        result = Interop.tobii_gaze_point_subscribe(deviceContext, gpCallback);
        Debug.Log("Subscribing Gaze Point Callback: " + (result == tobii_error_t.TOBII_ERROR_NO_ERROR));

        tobii_gaze_origin_callback_t goCallback = new tobii_gaze_origin_callback_t(OnGazeOrigin);
        result = Interop.tobii_gaze_origin_subscribe(deviceContext, goCallback);
        Debug.Log("Subscribing Gaze Origin Callback: " + (result == tobii_error_t.TOBII_ERROR_NO_ERROR));
        if (result == tobii_error_t.TOBII_ERROR_NO_ERROR)
        {
            deviceStatus = true;
            Debug.Log("-----Initialized Eye Tracker Successfully-----");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        result = Interop.tobii_device_process_callbacks(deviceContext);
        tracker.GetComponent<RectTransform>().position = new Vector3(currentGazePoint.x, 911 - currentGazePoint.y, 0f);
    }

    private void OnGazePoint(ref tobii_gaze_point_t gazePoint)
    {
        float x = gazePoint.position.x * 1920f;
        float y = gazePoint.position.y * 1080f;
        currentGazePoint.x = (int)x - 150;
        currentGazePoint.y = (int)y - 110;
        if (currentGazePoint.x > 1620)
        {
            currentGazePoint.x = 1620;
        }
        if (currentGazePoint.y > 911)
        {
            currentGazePoint.y = 911;
        }
    }

    private void OnGazeOrigin(ref tobii_gaze_origin_t gazeOrigin)
    {
        bool left = gazeOrigin.left_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
        bool right = gazeOrigin.right_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
        if (!left && right)
        {
            leftBlinkStatusCount++;
            rightBlinkStatusCount = 0;
            if (leftBlinkStatusCount == 1)
            {
                blinkPoint = currentGazePoint;
            }
            else if (leftBlinkStatusCount == 5)
            {
                blinkStatus.blinked = true;
                blinkStatus.left = true;
                blinkStatus.right = false;
            }
        }
        else if (left && !right)
        {
            rightBlinkStatusCount++;
            leftBlinkStatusCount = 0;
            if (rightBlinkStatusCount == 1)
            {
                blinkPoint = currentGazePoint;
            }
            else if (rightBlinkStatusCount == 5)
            {
                blinkStatus.blinked = true;
                blinkStatus.left = false;
                blinkStatus.right = true;
            }
        }
        else
        {
            leftBlinkStatusCount = 0;
            rightBlinkStatusCount = 0;
        }
    }

    public static bool GetDeviceStatus()
    {
        return deviceStatus;
    }

    public static BlinkStatus GetBlinkStatus()
    {
        BlinkStatus tempBlinkStatus = new BlinkStatus(blinkStatus.blinked, blinkStatus.left, blinkStatus.right);
        blinkStatus.blinked = false;
        blinkStatus.left = false;
        blinkStatus.right = false;
        return tempBlinkStatus;
    }

    public static Vector2 GetCurrentGazePoint()
    {
        return currentGazePoint;
    }

    public static Vector2 GetBlinkPoint()
    {
        return blinkPoint;
    }
}
