using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.StreamEngine;

public class EyeTrackerController : MonoBehaviour
{
    private tobii_error_t result;
    private IntPtr deviceContext;

    private static bool deviceStatus = false;
    private static bool blinkStatus = false;
    private static int blinkStatusCount = 0;
    private static Vector2 currentGazePoint = new Vector2();
    private static Vector2 blinkPoint = new Vector2();

    // Start is called before the first frame update
    void Start()
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
    void Update()
    {
        result = Interop.tobii_device_process_callbacks(deviceContext);
    }

    void OnGazePoint(ref tobii_gaze_point_t gazePoint)
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
        /*Debug.Log(gazePoint.position.x * Screen.width);
        Debug.Log(gazePoint.position.y * Screen.height);
        Debug.Log("--------------------------");*/
    }

    void OnGazeOrigin(ref tobii_gaze_origin_t gazeOrigin)
    {
        bool left = gazeOrigin.left_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
        bool right = gazeOrigin.right_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
        if ((!left && right) || (left && !right))
        {
            blinkStatusCount++;
            if (blinkStatusCount == 1)
            {
                blinkPoint = currentGazePoint;
            }
            else if (blinkStatusCount == 5)
            {
                blinkStatus = true;
            }
        }
        else
        {
            blinkStatusCount = 0;
        }
    }

    public static bool GetDeviceStatus()
    {
        return deviceStatus;
    }

    public static bool GetBlinkStatus()
    {
        bool status = blinkStatus;
        blinkStatus = false;
        return status;
    }
}
