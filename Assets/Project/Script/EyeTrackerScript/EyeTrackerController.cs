using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.StreamEngine;

public class EyeTrackerController : MonoBehaviour
{
    [SerializeField] private Image tracker;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Sprite validStatusSprite;
    [SerializeField] private Sprite invalidStatusSprite;
    
    private static IntPtr apiContext;
    private static IntPtr deviceContext;
    private static tobii_error_t result;

    private static bool deviceReady = false;
    private static bool recordBlinking = false;
    private static int leftBlinkStatusCount = 0;
    private static int rightBlinkStatusCount = 0;
    private static int validBlinkDuration = 5;
    private static float validGazeDuration = 0.8f;
    private static BlinkStatus blinkStatus = new BlinkStatus();
    private static Vector2 currentGazePoint = new Vector2(-1, -1);
    private static Vector2 blinkPoint = new Vector2(-1, -1);

    // Start is called before the first frame update
    private void Start()
    {
        deviceReady = false;
        statusIcon.sprite = invalidStatusSprite;
        result = Interop.tobii_api_create(out apiContext, null);
        bool apiCreated = result == tobii_error_t.TOBII_ERROR_NO_ERROR;
        //Debug.Log("Initialized API: " + apiCreated);

        List<string> urls;
        result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
        bool deviceFound = (result == tobii_error_t.TOBII_ERROR_NO_ERROR) && (urls.Count > 0);
        //Debug.Log("Found Device: " + deviceFound);

        if (deviceFound)
        {
            //IntPtr deviceContext;
            result = Interop.tobii_device_create(apiContext, urls[0], out deviceContext);
            bool deviceCreated = result == tobii_error_t.TOBII_ERROR_NO_ERROR;
            //Debug.Log("Create Device: " + deviceCreated);

            tobii_gaze_point_callback_t gpCallback = new tobii_gaze_point_callback_t(OnGazePoint);
            result = Interop.tobii_gaze_point_subscribe(deviceContext, gpCallback);
            bool gazePointSubscribed = result == tobii_error_t.TOBII_ERROR_NO_ERROR;
            //Debug.Log("Subscribing Gaze Point Callback: " + gazePointSubscribed);

            tobii_gaze_origin_callback_t goCallback = new tobii_gaze_origin_callback_t(OnGazeOrigin);
            result = Interop.tobii_gaze_origin_subscribe(deviceContext, goCallback);
            bool gazeOriginSubscribed = result == tobii_error_t.TOBII_ERROR_NO_ERROR;
            //Debug.Log("Subscribing Gaze Origin Callback: " + gazeOriginSubscribed);
            if (apiCreated && deviceFound && deviceCreated && gazePointSubscribed && gazeOriginSubscribed)
            {
                deviceReady = true;
                recordBlinking = true;
                //Debug.Log("-----Initialized Eye Tracker Successfully-----");
            }
            if (statusIcon != null && deviceReady)
            {
                statusIcon.sprite = validStatusSprite;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (deviceReady)
        {
            result = Interop.tobii_device_process_callbacks(deviceContext);
            if (tracker != null && tracker.gameObject.activeSelf)
            {
                tracker.GetComponent<RectTransform>().position = new Vector3(currentGazePoint.x, 911 - currentGazePoint.y, 0f);
            }
        }
    }

    private void OnGazePoint(ref tobii_gaze_point_t gazePoint)
    {
        float x = gazePoint.position.x * 1920f;
        float y = gazePoint.position.y * 1080f;
        currentGazePoint.x = (int)x;
        currentGazePoint.y = (int)y;

#if UNITY_EDITOR
        currentGazePoint.x = currentGazePoint.x - 150;
        currentGazePoint.y = currentGazePoint.y - 110;
        if (currentGazePoint.x < 0)
        {
            currentGazePoint.x = 0;
        }
        if (currentGazePoint.y < 0)
        {
            currentGazePoint.y = 0;
        }
        if (currentGazePoint.x > 1620)
        {
            currentGazePoint.x = 1620;
        }
        if (currentGazePoint.y > 911)
        {
            currentGazePoint.y = 911;
        }
#endif
    }

    private void OnGazeOrigin(ref tobii_gaze_origin_t gazeOrigin)
    {
        if (recordBlinking)
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
                else if (leftBlinkStatusCount == validBlinkDuration)
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
                else if (rightBlinkStatusCount == validBlinkDuration)
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
    }

    public static bool GetDeviceStatus()
    {
        return deviceReady;
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

    public static float GetValidGazeDuration()
    {
        return validGazeDuration;
    }

    public static void TurnOnBlinkRecording()
    {
        recordBlinking = true;
    }

    public static void TurnOffBlinkRecording()
    {
        leftBlinkStatusCount = 0;
        rightBlinkStatusCount = 0;
        blinkPoint = new Vector2(-1, -1);
        blinkStatus.blinked = false;
        blinkStatus.left = false;
        blinkStatus.right = false;
        recordBlinking = false;
    }

    public static void CleanUp()
    {
        if (deviceReady)
        {
            deviceReady = false;
            result = Interop.tobii_gaze_point_unsubscribe(deviceContext);
            result = Interop.tobii_device_destroy(deviceContext);
            result = Interop.tobii_api_destroy(apiContext);
        }
    }
}
