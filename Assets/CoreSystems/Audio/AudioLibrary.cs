using System.Collections.Generic;
using System.Linq;
using CoreSystems.Audio.Editor;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Audio/Audio Library", order = -1000)]
public class AudioLibrary : ScriptableObject
{
	[SerializeField] private List<AudioClip> clips;
	[SerializeField] private List<AudioClipProfile> clipEntries;

	private Dictionary<string, AudioClip> lookup;
	private Dictionary<string, AudioClipProfile> profileLookup;

	public AudioClip GetClip(string id) => lookup[id];
	public AudioClipProfile GetClipProfile(string id) => profileLookup.GetValueOrDefault(id);

	private void OnEnable()
	{
		BuildLookup();
	}

	private void BuildLookup()
	{
		lookup = clips.Where(c => c != null).ToDictionary(c => c.name, c => c);
		profileLookup = clipEntries.ToDictionary(c => c.Clip.name, c => c);
	}

	private void OnValidate()
	{
		BuildLookup();
	}
}