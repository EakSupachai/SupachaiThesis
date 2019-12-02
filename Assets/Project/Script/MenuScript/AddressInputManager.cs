using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddressInputManager : MonoBehaviour
{
    [SerializeField] private GameObject invalidText;

    private InputField inputField;
    private Text placeholder;
    private string objectName;
    private string placeholderText;
    private char[] delimiter = new char[2] { '.', ':' };

    private void Start()
    {
        invalidText.SetActive(false);
        objectName = gameObject.name;
        inputField = gameObject.GetComponent<InputField>();
        placeholder = transform.Find("Placeholder").GetComponent<Text>();
        if (objectName == "InputAddressField")
        {
            placeholder.text = AddressRecorder.in_ip1 + "." + AddressRecorder.in_ip2 + "." 
                + AddressRecorder.in_ip3 + "." + AddressRecorder.in_ip4 + ":" + AddressRecorder.in_port;
        }
        else
        {
            placeholder.text = AddressRecorder.out_ip1 + "." + AddressRecorder.out_ip2 + "."
                + AddressRecorder.out_ip3 + "." + AddressRecorder.out_ip4 + ":" + AddressRecorder.out_port;
        }
        placeholderText = placeholder.text;
        inputField.text = placeholderText;
    }

    public void RecordAddress()
    {
        if (inputField.text == "")
        {
            inputField.text = placeholderText;
        }
        char[] delimiter = { '.', ':' };
        int[] ip = { 0, 0, 0, 0, 0 };
        string[] strList = inputField.text.Split(delimiter);
        if (strList.Length != 5)
        {
            if (objectName == "InputAddressField")
            {
                AddressRecorder.in_recordValid = false;
            }
            else
            {
                AddressRecorder.out_recordValid = false;
            }
            invalidText.SetActive(true);
            return;
        }
        int parsedInt = 0;
        bool errorFound = false;
        for (int i = 0; i < strList.Length; i++)
        {
            strList[i] = strList[i].Trim();
            if (Int32.TryParse(strList[i], out parsedInt))
            {
                if (i == strList.Length - 1)
                {
                    if (parsedInt >= 1 && parsedInt <= 65535)
                    {
                        ip[i] = parsedInt;
                    }
                    else
                    {
                        errorFound = true;
                        break;
                    }
                }
                else
                {
                    if (parsedInt >= 0 && parsedInt <= 255)
                    {
                        ip[i] = parsedInt;
                    }
                    else
                    {
                        errorFound = true;
                        break;
                    }
                }
            }
            else
            {
                errorFound = true;
                break;
            }
        }
        if (errorFound)
        {
            if (objectName == "InputAddressField")
            {
                AddressRecorder.in_recordValid = false;
            }
            else
            {
                AddressRecorder.out_recordValid = false;
            }
            invalidText.SetActive(true);
            return;
        }
        if (objectName == "InputAddressField")
        {
            AddressRecorder.in_ip1 = ip[0];
            AddressRecorder.in_ip2 = ip[1];
            AddressRecorder.in_ip3 = ip[2];
            AddressRecorder.in_ip4 = ip[3];
            AddressRecorder.in_port = ip[4];
            AddressRecorder.in_recordValid = true;
        }
        else
        {
            AddressRecorder.out_ip1 = ip[0];
            AddressRecorder.out_ip2 = ip[1];
            AddressRecorder.out_ip3 = ip[2];
            AddressRecorder.out_ip4 = ip[3];
            AddressRecorder.out_port = ip[4];
            AddressRecorder.out_recordValid = true;
        }
        invalidText.SetActive(false);
    }
}
