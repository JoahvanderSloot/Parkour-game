using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixerGroup MixerGroup;

    public Sound[] Sounds;

    public Settings Settings;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound _s in Sounds)
        {
            _s.Source = gameObject.AddComponent<AudioSource>();
            _s.Source.clip = _s.Clip;
            _s.Source.loop = _s.Loop;

            _s.Source.outputAudioMixerGroup = MixerGroup;
        }
    }

    public void Play(string _sound)
    {
        Sound _s = Array.Find(Sounds, _item => _item.Name == _sound);
        if (_s == null)
        {
            Debug.LogWarning("Sound: " + _sound + " not found!");
            return;
        }

        _s.Source.volume = _s.m_Volume * (1f + UnityEngine.Random.Range(-_s.m_VolumeVariance / 2f, _s.m_VolumeVariance / 2f));
        _s.Source.pitch = _s.m_Pitch * (1f + UnityEngine.Random.Range(-_s.m_PitchVariance / 2f, _s.m_PitchVariance / 2f));

        if (!_s.Music)
        {
            if (Settings.Audio)
            {
                _s.Source.Play();
            }
        }
        else
        {
            if (Settings.Music)
            {
                _s.Source.Play();
            }
        }

    }

    public void Stop(string _sound)
    {
        Sound _s = Array.Find(Sounds, _item => _item.Name == _sound);
        if (_s == null)
        {
            Debug.LogWarning("Sound: " + _sound + " not found!");
            return;
        }

        _s.Source.Stop();
    }

    public void StopAllSounds()
    {
        foreach (Sound _s in Sounds)
        {
            _s.Source.Stop();
        }
    }

    public bool IsPlaying(string _sound)
    {
        Sound s = Array.Find(Sounds, _item => _item.Name == _sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + _sound + " not found!");
            return false;
        }

        return s.Source.isPlaying;
    }


    public void PauseAllSounds()
    {
        foreach (Sound _s in Sounds)
        {
            _s.Source.Pause();
        }
    }

    public void UnPauseAllSounds()
    {
        foreach (Sound _s in Sounds)
        {
            _s.Source.UnPause();
        }
    }
}