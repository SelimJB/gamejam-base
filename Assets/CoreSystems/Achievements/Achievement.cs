using System;
using CoreSystems.Audio;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Achievement", fileName = "NewAchievement", order = -900)]
	public class Achievement : ScriptableObject
	{
		[SerializeField] private string title;
		[SerializeField, TextArea(2, 4)] private string flavorText;
		[SerializeField] private bool overrideDescription;
		[FormerlySerializedAs("description")] [SerializeField, TextArea(2, 4)] private string customDescription;
		[SerializeField] private Sprite icon;
		[SerializeField] private Color color;
		[SerializeField] private Color backgroundColor;
		[SerializeField] private bool isHidden;
		[SerializeField] private AchievementCondition[] conditions;
		[SerializeField, PreviewAudioClip] private AudioClip unlockSound;

		public string Id { get; private set; }
		public string Title => title;
		public Sprite Icon => icon;
		public string FlavorText => flavorText;
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

		public string GetDescription(string bullet = "- ")
		{
			if (overrideDescription)
				return customDescription;

			if (conditions == null || conditions.Length == 0)
				return "No conditions set.";

			if (conditions.Length == 1)
				return conditions[0].Description;

			var res = new System.Text.StringBuilder();

			foreach (var condition in conditions)
			{
				if (condition != null)
				{
					res.AppendLine($"{bullet}{condition.Description}");
				}
			}

			return res.ToString();
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