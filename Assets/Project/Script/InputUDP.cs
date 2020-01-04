using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Random = System.Random;

public class InputUDP : MonoBehaviour
{
    private static Thread readThread;    
    private static UdpClient client;
    private static string bufferedInput = ""; // this one has to be cleaned up from time to time
    private static object lockObject = new object();
    private static bool newInputReceived = false;
    private static bool inputAvailableStatus = true;
    private static bool inputLockStatus = true;
    private static int bufferSize = 5;
    private static int threshold = 4;
    private static Random random = new Random();

    // start from unity3d
    private void Start()
    {
        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();
    }

    // Unity Application Quit Function
    private void OnApplicationQuit()
    {
        CloseConnection();
    }

    // receive thread function
    private void ReceiveData()
    {
        client = new UdpClient(AddressRecorder.in_port);
        while (true)
        {
            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                inputAvailableStatus = true;

                // encode UTF8-coded bytes to text format
                lock (lockObject)
                {
                    string text = "";
                    if (inputLockStatus)
                    {
                        text = "0\n";
                    }
                    else
                    {
                        text = Encoding.UTF8.GetString(data);
                    }

                    // show received message
                    int strLength = 0;
                    strLength = text.Length;
                    text = text.Substring(strLength - 2, 1);

                    // update received messages
                    newInputReceived = true;
                    bufferedInput = bufferedInput + text;
                    strLength = bufferedInput.Length;
                    if (strLength > bufferSize)
                    {
                        bufferedInput = bufferedInput.Substring(strLength - bufferSize);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    public static string GetNewBufferedInput()
    {
        lock (lockObject)
        {
            if (newInputReceived)
            {
                newInputReceived = false;
                return bufferedInput;
            }
            else
            {
                return "NULL";
            }
        }
    }

    public static void CloseConnection()
    {
        if (readThread.IsAlive)
        {
            readThread.Abort();
        }
        client.Close();
    }

    public static bool GetInputAvailableStatus()
    {
        return inputAvailableStatus;
    }

    public static void ClearInput()
    {
        lock (lockObject)
        {
            bufferedInput = "00000";
        }
    }

    public static int GetThreshold()
    {
        return threshold;
    }

    public static void LockInput()
    {
        lock (lockObject)
        {
            bufferedInput = "00000";
            inputLockStatus = true;
        }
    }

    public static void UnlockInput()
    {
        lock (lockObject)
        {
            bufferedInput = "00000";
            inputLockStatus = false;
        }
    }
}