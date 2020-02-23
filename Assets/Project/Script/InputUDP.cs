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
    private static float bufferedGrad0;
    private static float bufferedGrad1;
    private static float bufferedGrad2;
    private static List<float> bufferedNorm0 = new List<float>();
    private static List<float> bufferedNorm1 = new List<float>();
    private static List<float> bufferedNorm2 = new List<float>();
    private static object lockObject = new object();
    private static bool newInputReceived = false;
    private static bool inputAvailableStatus = false;
    private static bool inputLockStatus = true;
    private static int bufferSize = 4;
    private static int threshold = 3;
    private static int defaultThreshold = 3;
    private static int loweredThreshold = 2;
    private static Random random = new Random();

    // start from unity3d
    private void Start()
    {
        for (int i = 0; i < bufferSize; i++)
        {
            bufferedNorm0.Add(0f);
            bufferedNorm1.Add(0f);
            bufferedNorm2.Add(0f);
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
                    string text1 = "";
                    string text2 = "";
                    string text3 = "";
                    string text4 = "";
                    if (inputLockStatus)
                    {
                        text1 = "0\n";
                        text2 = "0\n";
                        text3 = "0\n";
                        text4 = "0\n";
                    }
                    else
                    {
                        text1 = Encoding.UTF8.GetString(data);
                        text2 = Encoding.UTF8.GetString(data);
                        text3 = Encoding.UTF8.GetString(data);
                        text4 = Encoding.UTF8.GetString(data);
                    }

                    // show received message
                    int strLength = 0;
                    int startIndex = 0;
                    float d_temp0 = 0f;
                    float d_temp1 = 0f;
                    float d_temp2 = 0f;
                    strLength = text1.Length;
                    text1 = text1.Substring(strLength - 2, 1);

                    startIndex = text2.IndexOf(' ') + 1;
                    text2 = text2.Substring(startIndex, text2.Length - startIndex - 1);
                    d_temp0 = float.Parse(text2);
                    startIndex = text3.IndexOf(' ') + 1;
                    text3 = text3.Substring(startIndex, text3.Length - startIndex - 1);
                    d_temp1 = float.Parse(text3);
                    startIndex = text4.IndexOf(' ') + 1;
                    text4 = text4.Substring(startIndex, text4.Length - startIndex - 1);
                    d_temp2 = float.Parse(text4);

                    // update received messages
                    newInputReceived = true;
                    bufferedInput = bufferedInput + text1;
                    strLength = bufferedInput.Length;
                    if (strLength > bufferSize)
                    {
                        bufferedInput = bufferedInput.Substring(strLength - bufferSize);
                    }

                    for (int i = 1; i < bufferSize; i++)
                    {
                        bufferedNorm0[i - 1] = bufferedNorm0[i];
                        bufferedNorm1[i - 1] = bufferedNorm1[i];
                        bufferedNorm2[i - 1] = bufferedNorm2[i];
                    }
                    float m_norm = d_temp0 + d_temp1 + d_temp2;
                    bufferedNorm0[bufferSize - 1] = d_temp0 / m_norm;
                    bufferedNorm1[bufferSize - 1] = d_temp1 / m_norm;
                    bufferedNorm2[bufferSize - 1] = d_temp2 / m_norm;
                    bufferedGrad0 = 0f;
                    bufferedGrad1 = 0f;
                    bufferedGrad2 = 0f;
                    for (int i = 0; i < bufferSize-1; i++)
                    {
                        bufferedGrad0 += (bufferedNorm0[i + 1] - bufferedNorm0[i]);
                        bufferedGrad1 += (bufferedNorm1[i + 1] - bufferedNorm1[i]);
                        bufferedGrad2 += (bufferedNorm2[i + 1] - bufferedNorm2[i]);
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

    /*public static string GetNewBufferedInput()
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
    }*/

    public static InputObject GetNewBufferedInput()
    {
        lock (lockObject)
        {
            if (newInputReceived)
            {
                InputObject io = new InputObject(bufferedInput, bufferedGrad0, bufferedGrad1, bufferedGrad2);
                newInputReceived = false;
                return io;
            }
            else
            {
                InputObject io = new InputObject("NULL", 0f, 0f, 0f);
                return io;
            }
        }
    }

    /*public static string GetNewBufferedDistance()
    {
        lock (lockObject)
        {
            return bufferedDistance1[bufferSize - 1] + " " + bufferedDistance2[bufferSize - 1];
        }
    }*/

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
            bufferedInput = "0000";
        }
    }

    public static void LowerThreshold()
    {
        threshold = loweredThreshold;
    }

    public static void ResetThreshold()
    {
        threshold = defaultThreshold;
    }

    public static int GetThreshold()
    {
        return threshold;
    }

    public static void LockInput()
    {
        lock (lockObject)
        {
            bufferedInput = "0000";
            inputLockStatus = true;
        }
    }

    public static void UnlockInput()
    {
        lock (lockObject)
        {
            bufferedInput = "0000";
            inputLockStatus = false;
        }
    }
}