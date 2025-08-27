using System;
using CoreSystems.Audio;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Achievement", fileName = "NewAchievement", order = -900)]
	public class Achievement : ScriptableObject
	{
		[Header("Basic Info")]
		[SerializeField] private string title;
		[SerializeField, TextArea(2, 4)] private string description;
		[SerializeField] private Sprite icon;
		[SerializeField] private Color color;
		[SerializeField] private Color backgroundColor;
		[SerializeField] private bool isHidden;

		[Header("Conditions")]
		[SerializeField] private AchievementCondition[] conditions;

		[Header("Audio")]
		[SerializeField, PreviewAudioClip] private AudioClip unlockSound;

		public string Id { get; private set; }
		public string Title => title;
		public string Description => description;
		public Sprite Icon => icon;
		public bool IsHidden => isHidden;
		public AchievementCondition[] Conditions => conditions;
		public AudioClip UnlockSound => unlockSound;
		public Color Color => color;
		public Color BackgroundColor => backgroundColor;
		public AchievementState State
		{
			get
			{
				if (isUnlockedFromPersistence || IsCompleted())
					return AchievementState.Unlocked;

				if (isHidden)
					return AchievementState.Hidden;

				return AchievementState.Locked;
			}
		}

		private bool isUnlockedFromPersistence;

		public void MarkAsUnlockedFromPersistence()
		{
			isUnlockedFromPersistence = true;
		}

		public void ResetPersistenceFlag()
		{
			isUnlockedFromPersistence = false;
		}

		public event Action<Achievement> OnUnlocked;

		public bool IsCompleted()
		{
			if (conditions == null || conditions.Length == 0)
				return false;

			foreach (var condition in conditions)
			{
				if (condition == null || !condition.IsConditionMet())
					return false;
			}

			return true;
		}

		public float GetProgress()
		{
			if (conditions == null || conditions.Length == 0)
				return 0f;

			var totalProgress = 0f;

			foreach (var condition in conditions)
			{
				if (condition != null)
					totalProgress += condition.GetProgress();
			}

			return totalProgress / conditions.Length;
		}

		public void Initialize(bool persistentProgress)
		{
			Id = $"{name}_{GetInstanceID()}";

			if (conditions == null) return;

			foreach (var condition in conditions)
			{
				condition?.Initialize(persistentProgress);

				if (condition?.OnConditionMet != null)
					condition.OnConditionMet = null;

				condition.OnConditionMet += OnConditionMetHandler;
			}
		}

		private void OnConditionMetHandler()
		{
			if (IsCompleted())
				AchievementManager.Instance.CheckAchievement(this);
		}

		internal void TriggerUnlock()
		{
			OnUnlocked?.Invoke(this);
		}

		public void Cleanup()
		{
			if (conditions != null)
			{
				foreach (var condition in conditions)
				{
					if (condition != null)
					{
						condition.OnConditionMet -= OnConditionMetHandler;
						condition.Cleanup();
					}
				}
			}

			OnUnlocked = null;
		}
	}

	public enum AchievementState
	{
		Hidden,
		Unlocked,
		Locked
	}
}