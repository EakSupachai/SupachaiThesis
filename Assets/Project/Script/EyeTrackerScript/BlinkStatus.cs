using UnityEngine;

public class BlinkStatus : MonoBehaviour
{
    public bool blinked;
    public bool left;
    public bool right;

    public BlinkStatus(bool blinked = false, bool left = false, bool right = false)
    {
        this.blinked = blinked;
        this.left = left;
        this.right = right;
    }
}
