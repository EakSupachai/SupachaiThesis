using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveStatInputManager : MonoBehaviour
{
    [SerializeField] private Button saveButton;

    private InputField inputField;
    private Text placeholder;
    private string objectName;
    private string placeholderText;

    private void Start()
    {
        inputField = gameObject.GetComponent<InputField>();
        placeholder = transform.Find("Placeholder").GetComponent<Text>();
        objectName = gameObject.name;
        if (objectName == "FileNameInputField")
        {
            inputField.text = SaveStatInputRecorder.fileName;
        }
        else if (objectName == "ShootingInputField")
        {
            inputField.text = "" + SaveStatInputRecorder.shootFalseTrigger;
        }
        else if (objectName == "CoreInputField")
        {
            inputField.text = "" + SaveStatInputRecorder.coreFalseTrigger;
        }
        else if (objectName == "InsideHUDInputField")
        {
            inputField.text = "" + SaveStatInputRecorder.blinkInsideFalseTrigger;
        }
        else if (objectName == "OutsideHUDInputField")
        {
            inputField.text = "" + SaveStatInputRecorder.blinkOutsideFalseTrigger;
        }
        RecordSaveStatInput();
    }
    
    public void RecordSaveStatInput()
    {
        int parsedInt = 0;
        if (objectName == "FileNameInputField")
        {
            SaveStatInputRecorder.fileName = inputField.text;
            SaveStatInputRecorder.fileName_recordValid = true;
            if (inputField.text == "")
            {
                //error
                SaveStatInputRecorder.fileName_recordValid = false;
            }
        }
        else if (objectName == "ShootingInputField")
        {
            if (Int32.TryParse(inputField.text, out parsedInt))
            {
                if (parsedInt >= 0)
                {
                    SaveStatInputRecorder.shootFalseTrigger = parsedInt;
                    SaveStatInputRecorder.shootFalseTrigger_recordValid = true;
                }
                else
                {
                    SaveStatInputRecorder.shootFalseTrigger_recordValid = false;
                }
            }
            else
            {
                //error
                SaveStatInputRecorder.shootFalseTrigger_recordValid = false;
            }
        }
        else if (objectName == "CoreInputField")
        {
            if (Int32.TryParse(inputField.text, out parsedInt))
            {
                SaveStatInputRecorder.coreFalseTrigger = parsedInt;
                SaveStatInputRecorder.coreFalseTrigger_recordValid = true;
            }
            else
            {
                //error
                SaveStatInputRecorder.coreFalseTrigger_recordValid = false;
            }
        }
        else if (objectName == "InsideHUDInputField")
        {
            if (Int32.TryParse(inputField.text, out parsedInt))
            {
                SaveStatInputRecorder.blinkInsideFalseTrigger = parsedInt;
                SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid = true;
            }
            else
            {
                //error
                SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid = false;
            }
        }
        else if (objectName == "OutsideHUDInputField")
        {
            if (Int32.TryParse(inputField.text, out parsedInt))
            {
                SaveStatInputRecorder.blinkOutsideFalseTrigger = parsedInt;
                SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid = true;
            }
            else
            {
                //error
                SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid = false;
            }
        }
        if (!SaveStatInputRecorder.fileName_recordValid || !SaveStatInputRecorder.shootFalseTrigger_recordValid
            || !SaveStatInputRecorder.coreFalseTrigger_recordValid || !SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid
            || !SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid)
        {
            saveButton.interactable = false;
        }
        else
        {
            saveButton.interactable = true;
        }
    }
}
