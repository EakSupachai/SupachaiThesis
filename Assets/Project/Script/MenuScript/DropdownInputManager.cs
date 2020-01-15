using UnityEngine;

public class DropdownInputManager : MonoBehaviour
{
    public void RecordStimulusMode(int value)
    {
        if (value == 0)
        {
            GameModeRecorder.useCrosshairStimulus = true;
        }
        else
        {
            GameModeRecorder.useCrosshairStimulus = false;
        }
    }
}
