using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class InputUDP : MonoBehaviour
{
    [SerializeField] private Text apiOutput;

    // read Thread
    Thread readThread;

    // udpclient object
    UdpClient client;

    // port number
    private int port = 20321;

    // UDP packet store
    private string bufferedInput = ""; // this one has to be cleaned up from time to time
    private object lockObject = new object();
    private int bufferSize = 5;
    private bool newInputReceived = false;

    // start from unity3d
    void Start()
    {
        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();
    }

    // Unity Update Function
    void Update()
    {
        // check button "q" to abort the read-thread
        if (Input.GetKeyDown("q"))
            stopThread();
    }

    // Unity Application Quit Function
    void OnApplicationQuit()
    {
        stopThread();
    }

    // Stop reading UDP messages
    private void stopThread()
    {
        if (readThread.IsAlive)
        {
            readThread.Abort();
        }
        client.Close();
    }

    // receive thread function
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // encode UTF8-coded bytes to text format
                string text = Encoding.UTF8.GetString(data);

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
                apiOutput.text = bufferedInput;
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    // return the latest message
    public bool getLatestPacket()
    {
        bool ssvepReceived = false;
        int ssvepCounter = -1;
        lock(lockObject)
        {
            if (newInputReceived)
            {
                newInputReceived = false;
                for(int i = 0; i < bufferedInput.Length; i++)
                {
                    if (bufferedInput[i].Equals('1'))
                    {
                        ssvepCounter++;
                    }
                }
            }
        }
        if (ssvepCounter >= (bufferSize - 1))
        {
            ssvepReceived = true;
        }
        return ssvepReceived;
    }
}