using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPrefabFadeOutScript : MonoBehaviour
{
    [SerializeField] private float fadeOutTime = 0.2f;

    private AudioSource audioSource;
    private float beginFadeOutTime;
    private float originalVolumn;
    private float decreaseVolumePerFrame;
    private bool fadeOut;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolumn = audioSource.volume;
        float timePerFrame = 1 / 60f;
        int fadeOutFrameCount = (int) Mathf.Ceil(fadeOutTime / timePerFrame);
        decreaseVolumePerFrame = originalVolumn / fadeOutFrameCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.IsPause())
        {
            audioSource.Pause();
            return;
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }

        if (fadeOut)
        {
            float volumn = audioSource.volume - decreaseVolumePerFrame;
            if (volumn < 0f)
            {
                volumn = 0f;
            }
            audioSource.volume = volumn;
            if (volumn == 0f)
            {
                Destroy(this.gameObject);
            }
        }

        if (!audioSource.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }

    public void BeginFadeOut()
    {
        if (!fadeOut)
        {
            fadeOut = true;
            beginFadeOutTime = Time.time;
        }
    }
}
