using UnityEngine;
using UnityEngine.UI;

public class ShootingStimulusDropdownInputManager : MonoBehaviour
{

    private void Start()
    {
        Dropdown dropdown = GetComponent<Dropdown>();
        if (GameModeRecorder.shootingStimulusMode == 0)
        {
            dropdown.value = 2;
        }
        else if (GameModeRecorder.shootingStimulusMode == 1)
        {
            dropdown.value = 0;
        }
        else if (GameModeRecorder.shootingStimulusMode == 2)
        {
            dropdown.value = 1;
        }
    }

    public void RecordStimulusMode(int value)
    {
        if (value == 0)
        {
            GameModeRecorder.shootingStimulusMode = 1;
        }
        else if (value == 1)
        {
            GameModeRecorder.shootingStimulusMode = 2;
        }
        else if (value == 2)
        {
            GameModeRecorder.shootingStimulusMode = 0;
        }
    }
}
