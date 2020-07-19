using UnityEngine;

public class BlinkStatus : MonoBehaviour
{
    public bool validOneEyedBlink;
    public bool oneEyedBlink;
    public bool twoEyedBlink;
    public bool heavyBlink;
    public bool left;
    public bool right;

    public BlinkStatus(bool validOneEyedBlink = false, bool oneEyedBlink = false, bool twoEyedBlink = false, bool left = false, bool right = false, bool heavyBlink = false)
    {
        this.validOneEyedBlink = validOneEyedBlink;
        this.oneEyedBlink = oneEyedBlink;
        this.twoEyedBlink = twoEyedBlink;
        this.left = left;
        this.right = right;
        this.heavyBlink = heavyBlink;
    }
}
