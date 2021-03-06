﻿using UnityEngine;
using UnityEngine.UI;

public class SSVEPStimulusController : MonoBehaviour
{
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite blackSprite;

    private Image image;
    private Color color;
    private int frameCount;
    private bool flickering;
    private bool spriteFlag;

    // Start is called before the first frame update
    private void Start()
    {
        image = GetComponent<Image>();
        color = image.color;
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
            if (frameCount > 2)
            {
                frameCount = 1;
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

    public float GetAlpha()
    {
        return image.color.a;
    }

    public void SetAlpha(float alpha)
    {
        color.a = alpha;
        image.color = color;
    }
}
