using System.Collections.Generic;
using CoreSystems.Audio;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Achievement Collection", fileName = "AchievementCollection", order = -900)]
	public class AchievementCollection : ScriptableObject
	{
		[SerializeField] private List<Achievement> achievements;
		[SerializeField, PreviewAudioClip] private AudioClip defaultUnlockSound; 

		public List<Achievement> Achievements => achievements;
		public AudioClip DefaultUnlockSound => defaultUnlockSound;
	}
}