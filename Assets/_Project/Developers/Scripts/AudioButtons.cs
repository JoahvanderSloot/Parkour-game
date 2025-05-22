using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioButtons : MonoBehaviour
{
    [SerializeField] private Button audioButton;
    [SerializeField] private Button musicButton;

    [SerializeField] private List<Sprite> images;

    [SerializeField] bool isAudioButton;

    [SerializeField] Settings gameSettings;

    private void Start()
    {
        if (isAudioButton)
        {
            UpdateButtonSprite(audioButton, gameSettings.Audio);
        }
        else
        {
            UpdateButtonSprite(musicButton, gameSettings.Music);
        }
    }

    public void AudioButton()
    {
        if (audioButton != null)
        {
            gameSettings.Audio = !gameSettings.Audio;
            UpdateButtonSprite(audioButton, gameSettings.Audio);

            //if(gameSettings.Audio)
            //{
            //    AudioManager.Instance.Play("Click");
            //}
            //else
            //{
            //    AudioManager.Instance.Stop("Click");
            //}
        }
    }

    public void MusicButton()
    {
        if (musicButton != null)
        {
            gameSettings.Music = !gameSettings.Music;
            UpdateButtonSprite(musicButton, gameSettings.Music);

            //AudioManager.Instance.Play("Click");
        }
    }

    private void UpdateButtonSprite(Button _button, bool _isActive)
    {
        if (_button != null && images != null && images.Count >= 2)
        {
            _button.image.sprite = _isActive ? images[1] : images[0];
        }
    }

    public void PointerEnter()
    {
        transform.localScale = new Vector2(1.1f, 1.1f);
        //AudioManager.Instance.Play("Hover");
    }

    public void PointerExit()
    {
        transform.localScale = Vector2.one;
    }
}
