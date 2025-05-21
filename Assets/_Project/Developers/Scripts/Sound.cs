using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {

	public string Name;

	public AudioClip Clip;

	[Range(0f, 1.5f)]
	public float m_Volume = .75f;
	[Range(0f, 1f)]
	public float m_VolumeVariance = .1f;

	[Range(.1f, 3f)]
	public float m_Pitch = 1f;
	[Range(0f, 1f)]
	public float m_PitchVariance = .1f;

	public bool Loop = false;

    public bool Music = false;

    public AudioMixerGroup MixerGroup;

	[HideInInspector]
	public AudioSource Source;

}
