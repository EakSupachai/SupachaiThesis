using UnityEngine;
using UnityEngine.UI;

public class SecondaryBlinkSideDropdownInputManager : MonoBehaviour
{
    void Start()
    {
        Dropdown dropdown = GetComponent<Dropdown>();
        if (GameModeRecorder.secondaryBlinkSide == 0)
        {
            dropdown.value = 0;
        }
        else if (GameModeRecorder.secondaryBlinkSide == 1)
        {
            dropdown.value = 1;
        }
    }

    public void RecordSecondaryBlinkSideMode(int value)
    {
        if (value == 0)
        {
            GameModeRecorder.secondaryBlinkSide = 0;
        }
        else if (value == 1)
        {
            GameModeRecorder.secondaryBlinkSide = 1;
        }
    }
}
