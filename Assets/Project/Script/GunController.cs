using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class GunController : MonoBehaviour
{
    [HideInInspector] public float kickUpTime;
    [HideInInspector] public float lowerTime;
    public float aimSpeed;
    public Vector3 hipRecoil;
    public Vector3 aimRecoil;
    public Vector3 aimPosition;

    public abstract bool Shoot();
    public abstract void SwitchCrosshair();
    public abstract void SwitchToDefaultCrosshair();
    public abstract void HideCrosshair();
    public abstract void SwitchMode(string mode);
}
