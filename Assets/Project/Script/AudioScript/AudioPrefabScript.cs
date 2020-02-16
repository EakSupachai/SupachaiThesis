using UnityEngine;

public class AudioPrefabScript : MonoBehaviour
{
    private AudioSource audioSource;
    private float addedPitch = 0.08f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        
        if (GameController.IsInWaveCompletedState() && !GameController.IsGameOver())
        {
            audioSource.pitch = Time.timeScale + addedPitch;
        }
        else
        {
            if (GameController.IsGameOver())
            {
                audioSource.pitch = GameController.defaultTimeScale;
            }
            else
            {
                audioSource.pitch = Time.timeScale == GameController.defaultTimeScale ? Time.timeScale : Time.timeScale + addedPitch;
            }
        }

        if (!audioSource.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetAddedPitch(float pitch)
    {
        addedPitch = pitch;
    }
}
