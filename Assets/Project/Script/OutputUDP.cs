using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Bci2000Api;

public class OutputUDP : MonoBehaviour
{
    private static IntPtr BCI2000API;

    private static int ip1 = 192;
    private static int ip2 = 168;
    private static int ip3 = 1;
    private static int ip4 = 54;
    private static int port = 20320;

    private void Start()
    {
        BCI2000API = Interop.Create();
    }

    private void OnApplicationQuit()
    {
        if (Interop.UseIsSendConnectionOpen(BCI2000API))
        {
            SetClassifyingState(0);
            SetRecordingState(0);
            Interop.UseCloseSendConnection(BCI2000API);
        }
    }

    public static void AssignSendAddress()
    {
        Interop.UseAssignSendAddress(BCI2000API, ip1, ip2, ip3, ip4, port);
    }

    public static void OpenConnection()
    {
        Interop.UseOpenSendConnection(BCI2000API);
    }

    public static void CloseConnection()
    {
        Interop.UseCloseSendConnection(BCI2000API);
    }

    public static void SetRecordingState(int value)
    {
        Interop.UseSetRecordingState(BCI2000API, value);
    }

    public static void SetClassifyingState(int value)
    {
        Interop.UseSetClassifyingState(BCI2000API, value);
    }

    public static bool IsConnectionOpen()
    {
        return Interop.UseIsSendConnectionOpen(BCI2000API);
    }
}
