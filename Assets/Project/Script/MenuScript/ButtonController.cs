using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bci2000Api;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private List<GameObject> hoverMarkers = new List<GameObject>();
    [SerializeField] private AudioClip hoverAudio;
    [SerializeField] private GameObject clickAudioPrefab;

    private static bool resumePressed;

    private AudioSource audioSource;

    // Start is called before the first frame update
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (GameObject marker in hoverMarkers)
        {
            marker.SetActive(false);
        }
    }

    public void OnPointerEnter()
    {
        audioSource.clip = hoverAudio;
        audioSource.Play();
        foreach (GameObject marker in hoverMarkers)
        {
            marker.SetActive(true);
        }
    }

    public void OnPointerExit()
    {
        foreach (GameObject marker in hoverMarkers)
        {
            marker.SetActive(false);
        }
    }

    public void OnPointerClick(string command)
    {
        if (command == "resume")
        {
            resumePressed = true;
            OnPointerExit();
        }
        else if (command == "retry" || command == "start")
        {
            if (command == "retry")
            {
                InputUDP.Stop();
                EyeTrackerController.CleanUp();
                if (Interop.UseIsSendConnectionOpen(GameController.BCI2000API))
                {
                    Interop.UseCloseSendConnection(GameController.BCI2000API);
                }
            }
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            StartCoroutine(StallBeforeLoadingScene(1));
        }
        else if (command == "back")
        {
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            StartCoroutine(StallBeforeLoadingScene(0));
        }
        else if (command == "quit")
        {
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            StartCoroutine(StallBeforeQuit());
        }
        else
        {
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    public static bool IsResumePressed()
    {
        bool pressed = resumePressed;
        resumePressed = false;
        return pressed;
    }

    private IEnumerator StallBeforeLoadingScene(int scene)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (scene == 0)
        {
            InputUDP.Stop();
            EyeTrackerController.CleanUp();
        }
        if (Interop.UseIsSendConnectionOpen(GameController.BCI2000API))
        {
            Interop.UseCloseSendConnection(GameController.BCI2000API);
        }
        yield return new WaitForSecondsRealtime(0.75f);
        SceneManager.LoadScene(scene);
    }

    private IEnumerator StallBeforeQuit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        yield return new WaitForSecondsRealtime(0.75f);
        Application.Quit();
    }
}
