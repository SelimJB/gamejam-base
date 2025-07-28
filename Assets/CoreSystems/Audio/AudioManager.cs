using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace CoreSystems.Audio
{
	public class AudioManager : MonoBehaviour
	{
		[SerializeField] private AudioMixer audioMixer;
		[SerializeField] private AudioLibrary audioLibrary;
		[SerializeField] private AudioSource musicSource;
		[SerializeField] private List<AudioSource> sfxSources = new();
		[SerializeField] private bool isMasterMuted;

		[Header("Music")]
		[SerializeField] private AudioClip music;
		[SerializeField] private bool playMusicOnStart = true;

		[Header("SFX")]
		[SerializeField] private AudioSource sfxAudioSourcePrefab;
		[SerializeField] private int sfxMaxSources = 20;

		private static AudioManager instance;

		public AudioLibrary AudioLibrary => audioLibrary;
		public static AudioManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<AudioManager>();
				}

				return instance;
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
		}


		private void Start()
		{
			musicSource.clip = music;
			if (playMusicOnStart)
				musicSource.Play();
		}

		public void PlayMusic(AudioClip newClip = null, bool loop = true)
		{
			if (newClip != null)
			{
				musicSource.clip = newClip;
			}

			if (musicSource.clip == null)
			{
				Debug.LogWarning("No music clip assigned to play.");
				return;
			}

			musicSource.loop = loop;
			musicSource.Play();
		}

		public void PauseMusic()
		{
			if (musicSource.isPlaying)
				musicSource.Pause();
		}

		public void ResumeMusic()
		{
			if (!musicSource.isPlaying)
				musicSource.UnPause();
		}

		public void StopMusic()
		{
			musicSource.Stop();
		}

		public void Play(AudioClip clip, bool useClipProfile = true)
		{
			var volume = 1f;
			var pitch = 1f;
			var loop = false;

			if (useClipProfile)
			{
				var clipProfile = audioLibrary.GetClipProfile(clip.name);
				if (clipProfile != null)
				{
					volume = clipProfile.Volume;
					pitch = clipProfile.RandomizePitch ? Random.Range(clipProfile.PitchMin, clipProfile.PitchMax) : 1f;
					loop = clipProfile.Loop;
				}
			}

			PlayRaw(clip, volume, pitch, loop);
		}

		public void Play(string clipName, bool useClipProfile = true)
		{
			var clip = audioLibrary.GetClip(clipName);
			if (clip == null)
			{
				Debug.LogWarning($"Audio clip '{clipName}' not found in AudioLibrary.");
				return;
			}

			Play(clip, useClipProfile);
		}

		public void PlayRaw(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
		{
			if (clip == null)
			{
				Debug.LogWarning("Attempted to play a null AudioClip.");
				return;
			}

			if (!TryGetAvailableAudioSource(out var source))
			{
				Debug.LogWarning($"No available audio source to play {clip.name}");
				return;
			}

			ReinitializeAudioSource(source);
			source.clip = clip;
			source.volume = volume;
			source.pitch = pitch;
			source.loop = loop;

			source.Play();
		}

		private bool TryGetAvailableAudioSource(out AudioSource audioSource)
		{
			// Should be good enough for most cases, but could be optimized further 
			foreach (var source in sfxSources)
			{
				if (source.isPlaying) continue;

				audioSource = source;
				return true;
			}

			if (sfxSources.Count < sfxMaxSources)
			{
				var newSource = Instantiate(sfxAudioSourcePrefab, transform);
				sfxSources.Add(newSource);
				audioSource = newSource;
				return true;
			}

			audioSource = null;
			return false;
		}

		private void ReinitializeAudioSource(AudioSource source)
		{
			source.clip = null;
			source.volume = 1f;
			source.pitch = 1f;
			source.loop = false;
		}
	}
}