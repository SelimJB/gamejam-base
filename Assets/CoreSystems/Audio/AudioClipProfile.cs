using UnityEngine;

namespace CoreSystems.Audio.Editor
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Audio/Create AudioClipProfile", fileName = "NewAudioClipProfile", order = -1000)]
	public class AudioClipProfile : ScriptableObject
	{
		[SerializeField] private AudioClip clip;
		[SerializeField, Range(0f, 1f)] private float volume = 1f;
		[SerializeField] private bool loop;
		[SerializeField] private bool randomizePitch;
		[SerializeField] private float pitchMin = 0.95f;
		[SerializeField] private float pitchMax = 1.05f;
		[SerializeField] private float delayBetweenPlays = 0f;

		public AudioClip Clip => clip;
		public float Volume => volume;
		public bool RandomizePitch => randomizePitch;
		public float PitchMin => pitchMin;
		public float PitchMax => pitchMax;
		public float Pitch => RandomizePitch ? Random.Range(pitchMin, pitchMax) : 1f;
		public string Name => name;
		public bool Loop => loop;
		public float DelayBetweenPlays => delayBetweenPlays;

		public void SetClip(AudioClip c) => clip = c;
	}
}