using UnityEngine;
using System;
using Bci2000Api;

public class OutputUDP : MonoBehaviour
{
    private static IntPtr BCI2000API;

    private void Start()
    {
        BCI2000API = Interop.Create();
    }

    private void OnApplicationQuit()
    {
        if (Interop.UseIsSendConnectionOpen(BCI2000API))
        {
            Interop.UseCloseSendConnection(BCI2000API);
        }
    }

    public static void OpenConnection()
    {
        Interop.UseAssignSendAddress(BCI2000API, AddressRecorder.in_ip1, AddressRecorder.in_ip2, 
            AddressRecorder.in_ip3, AddressRecorder.in_ip4, AddressRecorder.in_port);
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
