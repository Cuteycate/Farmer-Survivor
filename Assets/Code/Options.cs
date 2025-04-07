using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component missing on the Options GameObject.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isShowing)
                Hide();
            else
                Show();
        }
    }

    public void Show()
    {
        if (GameManager.instance.isLive)
        {
            canvasGroup.alpha = 1f; 
            canvasGroup.interactable = true; 
            canvasGroup.blocksRaycasts = true;
            GameManager.instance.Stop();
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
            AudioManager.instance.EffectBgm(true);
            isShowing = true;
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
        isShowing = false;
    }
}
