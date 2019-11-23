using UnityEngine;

public class BlinkStatus : MonoBehaviour
{
    public bool oneEyedBlink;
    public bool twoEyedBlink;
    public bool left;
    public bool right;

    public BlinkStatus(bool oneEyedBlink = false, bool twoEyedBlink = false, bool left = false, bool right = false)
    {
        this.oneEyedBlink = oneEyedBlink;
        this.twoEyedBlink = twoEyedBlink;
        this.left = left;
        this.right = right;
    }
}
