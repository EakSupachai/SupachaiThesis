using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private List<GameObject> hoverMarkers = new List<GameObject>();
    [SerializeField] private AudioClip hoverAudio;
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject clickAudioPrefab;
    [SerializeField] private GameObject errorAudioPrefab;

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
        else if (command == "game start")
        {
            if (AddressRecorder.in_recordValid && AddressRecorder.out_recordValid)
            {
                Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
                GameModeRecorder.classifyMode = true;
                GameModeRecorder.testMode = false;
                GameModeRecorder.calibrationMode = false;
                SaveStatInputRecorder.ResetValue();
                StartCoroutine(StallBeforeLoadingScene(1));
            }
            else
            {
                Instantiate(errorAudioPrefab, Vector3.zero, Quaternion.identity);
            }
        }
        else if (command == "test start")
        {
            if (AddressRecorder.in_recordValid && AddressRecorder.out_recordValid)
            {
                Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
                GameModeRecorder.classifyMode = true;
                GameModeRecorder.testMode = true;
                GameModeRecorder.calibrationMode = false;
                SaveStatInputRecorder.ResetValue();
                StartCoroutine(StallBeforeLoadingScene(1));
            }
            else
            {
                Instantiate(errorAudioPrefab, Vector3.zero, Quaternion.identity);
            }
        }
        else if (command == "calibration start")
        {
            if (AddressRecorder.in_recordValid && AddressRecorder.out_recordValid)
            {
                Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
                GameModeRecorder.classifyMode = false;
                GameModeRecorder.testMode = false;
                GameModeRecorder.calibrationMode = true;
                StartCoroutine(StallBeforeLoadingScene(1));
            }
            else
            {
                Instantiate(errorAudioPrefab, Vector3.zero, Quaternion.identity);
            }
        }
        else if (command == "retry")
        {
            InputUDP.CloseConnection();
            OutputUDP.CloseConnection();
            EyeTrackerController.CleanUp();
            SaveStatInputRecorder.ResetValue();
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
        else if (command == "show stat" && gameController != null)
        {
            if (gameController.SwitchShowStat())
            {
                Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            }
            else
            {
                Instantiate(errorAudioPrefab, Vector3.zero, Quaternion.identity);
            }
        }
        else if (command == "show save stat panel" && gameController != null)
        {
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            gameController.SwitchSaveStat();
        }
        else if (command == "save stat" && gameController != null)
        {
            Instantiate(clickAudioPrefab, Vector3.zero, Quaternion.identity);
            gameController.SaveStat();
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
        // loading back to the main menu
        if (scene == 0)
        {
            InputUDP.CloseConnection();
            OutputUDP.CloseConnection();
            EyeTrackerController.CleanUp();
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
