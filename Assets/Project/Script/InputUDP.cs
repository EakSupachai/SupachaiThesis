using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Random = System.Random;

public class InputUDP : MonoBehaviour
{
    private static Thread readThread;    
    private static UdpClient client;
    private static string bufferedInput = ""; // this one has to be cleaned up from time to time
    private static List<float> bufferedDistance1 = new List<float>();
    private static List<float> bufferedDistanceDiff1 = new List<float>();
    private static List<float> bufferedDistance2 = new List<float>();
    private static List<float> bufferedDistanceDiff2 = new List<float>();
    private static object lockObject = new object();
    private static bool newInputReceived = false;
    private static bool inputAvailableStatus = false;
    private static bool inputLockStatus = true;
    private static int bufferSize = 5;
    private static int threshold = 4;
    private static Random random = new Random();

    // start from unity3d
    private void Start()
    {
        for (int i = 0; i < bufferSize; i++)
        {
            bufferedDistance1.Add(0f);
            bufferedDistance2.Add(0f);
        }
        for (int i = 0; i < bufferSize-1; i++)
        {
            bufferedDistanceDiff1.Add(0f);
            bufferedDistanceDiff2.Add(0f);
        }
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
                    string text2 = "";
                    string text3 = "";
                    if (inputLockStatus)
                    {
                        text = "0\n";
                        text2 = "0\n";
                        text3 = "0\n";
                    }
                    else
                    {
                        text = Encoding.UTF8.GetString(data);
                        text2 = Encoding.UTF8.GetString(data);
                        text3 = Encoding.UTF8.GetString(data);
                    }

                    // show received message
                    int strLength = 0;
                    int startIndex = 0;
                    float d_temp1 = 0f;
                    float d_temp2 = 0f;
                    strLength = text.Length;
                    text = text.Substring(strLength - 2, 1);

                    startIndex = text2.IndexOf(' ') + 1;
                    text2 = text2.Substring(startIndex, text2.Length - startIndex - 1);
                    d_temp1 = float.Parse(text2);
                    startIndex = text3.IndexOf(' ') + 1;
                    text3 = text3.Substring(startIndex, text3.Length - startIndex - 1);
                    d_temp2 = float.Parse(text3);

                    // update received messages
                    newInputReceived = true;
                    bufferedInput = bufferedInput + text;
                    strLength = bufferedInput.Length;
                    if (strLength > bufferSize)
                    {
                        bufferedInput = bufferedInput.Substring(strLength - bufferSize);
                    }

                    for (int i = 1; i < bufferSize; i++)
                    {
                        bufferedDistance1[i - 1] = bufferedDistance1[i];
                        bufferedDistance2[i - 1] = bufferedDistance2[i];
                    }
                    bufferedDistance1[bufferSize - 1] = d_temp1;
                    bufferedDistance2[bufferSize - 1] = d_temp2;
                    for (int i = 0; i < bufferSize-1; i++)
                    {
                        bufferedDistanceDiff1[i] = bufferedDistance1[i + 1] - bufferedDistance1[i];
                        bufferedDistanceDiff2[i] = bufferedDistance2[i + 1] - bufferedDistance2[i];
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    //not full algorithm version
    /*private void ReceiveData()
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
    }*/

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

    public static string GetNewBufferedDistance()
    {
        lock (lockObject)
        {
            return bufferedDistance1[bufferSize - 1] + " " + bufferedDistance2[bufferSize - 1];
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