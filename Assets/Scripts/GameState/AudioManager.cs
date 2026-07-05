using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[Header("SFX")]
	[SerializeField] private AudioSource sfxSource;
	[SerializeField] private float sfxVolume = 1f;

	private readonly List<AudioSource> loopSfxSources = new List<AudioSource>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		if (sfxSource == null)
		{
			sfxSource = GetComponent<AudioSource>();
		}
	}

	public void PlaySfx(AudioClip clip, float volume = 1f)
	{
		if (clip == null)
		{
			return;
		}

		if (sfxSource == null)
		{
			return;
		}

		sfxSource.PlayOneShot(clip, volume * sfxVolume);
	}

	public AudioSource PlayLoopSfx(AudioClip clip, float volume = 1f)
	{
		if (clip == null)
		{
			return null;
		}

		GameObject soundObject = new GameObject($"LoopSfx_{clip.name}");
		soundObject.transform.SetParent(transform);

		AudioSource audioSource = soundObject.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.loop = true;
		audioSource.volume = volume * sfxVolume;
		audioSource.spatialBlend = 0f; // 위치와 상관없는 2D 사운드
		audioSource.playOnAwake = false;

		audioSource.Play();

		loopSfxSources.Add(audioSource);

		return audioSource;
	}

	public void StopLoopSfx(AudioSource audioSource)
	{
		if (audioSource == null)
		{
			return;
		}

		loopSfxSources.Remove(audioSource);

		Destroy(audioSource.gameObject);
	}

	public void StopAllSounds()
	{
		if (sfxSource != null)
		{
			sfxSource.Stop();
		}

		for (int i = loopSfxSources.Count - 1; i >= 0; i--)
		{
			AudioSource source = loopSfxSources[i];

			if (source == null)
			{
				loopSfxSources.RemoveAt(i);
				continue;
			}

			Destroy(source.gameObject);
		}

		loopSfxSources.Clear();
	}
}