using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Random = System.Random;

public class InputUDP : MonoBehaviour
{
    //[SerializeField] private Text apiOutput;
    
    private static Thread readThread;    
    private static UdpClient client;
    private static string bufferedInput = ""; // this one has to be cleaned up from time to time
    private static object lockObject = new object();
    private static bool newInputReceived = false;
    private static bool inputStatus = false;
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
                //byte[] data = client.Receive(ref anyIP);
                inputStatus = true;

                // encode UTF8-coded bytes to text format
                //string text = Encoding.UTF8.GetString(data);
                int rand = random.Next(0, 100);
                string text = "";
                if (rand < 50)
                {
                    text = "0\n";
                }
                else
                {
                    text = "1\n";
                }

                // show received message
                int strLength = 0;
                strLength = text.Length;
                text = text.Substring(strLength - 2, 1);

                // update received messages
                lock (lockObject)
                {
                    newInputReceived = true;
                    bufferedInput = bufferedInput + text;
                    strLength = bufferedInput.Length;
                    if (strLength > bufferSize)
                    {
                        bufferedInput = bufferedInput.Substring(strLength - bufferSize);
                    }
                }
                //apiOutput.text = bufferedInput;
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    /*public static bool IsSsvepDetected()
    {
        int ssvepCounter = 0;
        lock (lockObject)
        {
            //Debug.Log(bufferedInput);
            for (int i = 0; i < bufferedInput.Length; i++)
            {
                if (bufferedInput[i].Equals('1'))
                {
                    ssvepCounter++;
                }
            }
        }
        if (ssvepCounter >= threshold)
        {
            return true;
        }
        return false;
    }*/

    public static bool IsNewInputReceived()
    {
        lock (lockObject)
        {
            bool temp = newInputReceived;
            newInputReceived = false;
            return temp;
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

    public static bool GetSsvepInputStatus()
    {
        return inputStatus;
    }

    public static string GetBufferedInput()
    {
        lock (lockObject)
        {
            return bufferedInput;
        }
    }
}