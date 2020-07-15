using UnityEngine;
using UnityEngine.UI;

public class PrimaryBlinkSideDropdownInputManager : MonoBehaviour
{
    void Start()
    {
        Dropdown dropdown = GetComponent<Dropdown>();
        if (GameModeRecorder.primaryBlinkSide == 0)
        {
            dropdown.value = 0;
        }
        else if (GameModeRecorder.primaryBlinkSide == 1)
        {
            dropdown.value = 1;
        }
    }
    
    public void RecordPrimaryBlinkSideMode(int value)
    {
        if (value == 0)
        {
            GameModeRecorder.primaryBlinkSide = 0;
        }
        else if (value == 1)
        {
            GameModeRecorder.primaryBlinkSide = 1;
        }
    }
}
