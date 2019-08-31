using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSVEPStimulusController : MonoBehaviour
{
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite blackSprite;

    private Image image;
    private int frameCount;
    private bool flickering;
    private bool spriteFlag;

    // Start is called before the first frame update
    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = null;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameController.IsPause())
        {
            return;
        }
        if (flickering)
        {
            if (frameCount > 4)
            {
                frameCount = 0;
                image.sprite = spriteFlag ? whiteSprite : blackSprite;
                spriteFlag = !spriteFlag;
            }
            frameCount++;
        }
    }

    public void StartFlickering()
    {
        frameCount = 1;
        flickering = true;
        spriteFlag = false;
        image.sprite = whiteSprite;
        gameObject.SetActive(true);
    }

    public void StopFlickering()
    {
        gameObject.SetActive(false);
        flickering = false;
        image.sprite = null;
    }

    public bool IsFlickering()
    {
        return flickering;
    }
}
