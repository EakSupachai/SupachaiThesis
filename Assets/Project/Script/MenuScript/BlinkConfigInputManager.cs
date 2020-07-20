using System;
using UnityEngine;
using UnityEngine.UI;

public class BlinkConfigInputManager : MonoBehaviour
{
    void Start()
    {
        string objectName = gameObject.name;
        Text placeholder = transform.Find("Placeholder").GetComponent<Text>();
        InputField inputField = gameObject.GetComponent<InputField>();
        if (objectName == "NormalBlinkField")
        {
            placeholder.text = "" + EyeTrackerController.GetValidBlinkDuration();
            inputField.text = "" + EyeTrackerController.GetValidBlinkDuration();
            EyeTrackerController.SetValidBlinkDuration(EyeTrackerController.GetValidBlinkDuration());
        }
        else if (objectName == "NormalBlinkThresholdField")
        {
            placeholder.text = "" + (int)Math.Round(EyeTrackerController.GetValidOneEyeBlinkPercentage() * 100, 0);
            inputField.text = "" + (int)Math.Round(EyeTrackerController.GetValidOneEyeBlinkPercentage() * 100, 0);
            EyeTrackerController.SetValidOneEyeBlinkPercentage(EyeTrackerController.GetValidOneEyeBlinkPercentage());
        }
        else if (objectName == "HeavyBlinkField")
        {
            placeholder.text = "" + EyeTrackerController.GetValidHeavyBlinkDuration();
            inputField.text = "" + EyeTrackerController.GetValidHeavyBlinkDuration();
            EyeTrackerController.SetValidHeavyBlinkDuration(EyeTrackerController.GetValidHeavyBlinkDuration());
        }
    }

    public void SetValidBlinkDuration(string input)
    {
        int value = 0;
        bool valid = Int32.TryParse(input, out value);
        if (valid)
        {
            EyeTrackerController.SetValidBlinkDuration(value);
        }
    }

    public void SetValidOneEyeBlinkPercentage(string input)
    {
        int value = 0;
        bool valid = Int32.TryParse(input, out value);
        if (valid)
        {
            EyeTrackerController.SetValidOneEyeBlinkPercentage(value / 100f);
        }
    }

    public void SetValidHeavyBlinkDuration(string input)
    {
        int value = 0;
        bool valid = Int32.TryParse(input, out value);
        if (valid)
        {
            EyeTrackerController.SetValidHeavyBlinkDuration(value);
        }
    }
}
