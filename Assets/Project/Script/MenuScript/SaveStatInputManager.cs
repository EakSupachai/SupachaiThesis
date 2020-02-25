using System;
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
        else if (objectName == "InsideHUDInputField")
        {
            if (Int32.TryParse(inputField.text, out parsedInt))
            {
                if (parsedInt >= 0)
                {
                    SaveStatInputRecorder.blinkInsideFalseTrigger = parsedInt;
                    SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid = true;
                }
                else
                {
                    SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid = false;
                }
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
                if (parsedInt >= 0)
                {
                    SaveStatInputRecorder.blinkOutsideFalseTrigger = parsedInt;
                    SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid = true;
                }
                else
                {
                    SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid = false;
                }
            }
            else
            {
                //error
                SaveStatInputRecorder.blinkOutsideFalseTrigger_recordValid = false;
            }
        }
        if (!SaveStatInputRecorder.fileName_recordValid || !SaveStatInputRecorder.blinkInsideFalseTrigger_recordValid
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
