using UnityEngine;

public class AudioPrefabScript : MonoBehaviour
{
    private AudioSource audioSource;

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
        
        if (!GameController.IsSlowMotionAllowed() && !GameController.IsGameOver())
        {
            audioSource.pitch = Time.timeScale + 0.15f;
        }
        else
        {
            if (GameController.IsGameOver())
            {
                audioSource.pitch = GameController.defaultTimeScale;
            }
            else
            {
                audioSource.pitch = Time.timeScale;
            }
        }
        //audioSource.pitch = Time.timeScale != GameController.defaultTimeScale ? Time.timeScale + 0.1f : Time.timeScale;
        if (!audioSource.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
}
