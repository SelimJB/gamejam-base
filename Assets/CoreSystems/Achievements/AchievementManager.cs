using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoreSystems.Audio;

namespace CoreSystems.Achievements
{
	public class AchievementManager : MonoBehaviour
	{
		[Header("Achievement System")]
		[SerializeField] private AchievementCollection achievementCollection;
		[SerializeField] private bool playUnlockSounds = true;
		[SerializeField] private PersistenceMode persistenceMode = PersistenceMode.Disabled;
		[SerializeField] private bool printDebugLogs;

		[Header("UI")]
		[SerializeField] private GameObject achievementNotificationPrefab;
		[SerializeField] private Transform notificationParent;

		public static event Action<Achievement> OnAchievementUnlocked;
		public static event Action<Achievement> OnAchievementProgressChanged;

		private readonly Dictionary<string, bool> unlockedAchievements = new();
		private readonly HashSet<string> initializedAchievements = new();

		public List<Achievement> AllAchievements => achievementCollection.Achievements;
		public IEnumerable<Achievement> UnlockedAchievements => AllAchievements.Where(a => IsUnlocked(a.Id));
		public IEnumerable<Achievement> LockedAchievements => AllAchievements.Where(a => !IsUnlocked(a.Id));
		public IEnumerable<Achievement> VisibleAchievements => AllAchievements.Where(a => !a.IsHidden);
		public int TotalAchievements => AllAchievements.Count;
		public int UnlockedCount => unlockedAchievements.Count(kvp => kvp.Value);

		public enum PersistenceMode
		{
			[Tooltip("Nothing persists - fresh start every time")]
			Disabled,

			[Tooltip("Achievement unlocks persist, but progress resets")]
			UnlocksOnly,

			[Tooltip("Everything persists")]
			Full
		}

		private static AchievementManager instance;
		public static AchievementManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<AchievementManager>();
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
				InitializeSystem();
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			CheckAllAchievements();
		}

		private void InitializeSystem()
		{
			LoadUnlockedAchievements();
			InitializeAllAchievements();

			if (unlockedAchievements.Count > 0 && printDebugLogs)
				Debug.Log($"Achievement System initialized with {UnlockedCount}/{TotalAchievements} unlocked achievements.");
		}

		private void InitializeAllAchievements()
		{
			foreach (var achievement in AllAchievements)
			{
				if (achievement != null && !initializedAchievements.Contains(achievement.Id))
				{
					var persistentProgress = persistenceMode == PersistenceMode.Full;
					achievement.Initialize(persistentProgress);
					initializedAchievements.Add(achievement.Id);
				}
			}
		}

		private void OnDestroy()
		{
			foreach (var achievement in AllAchievements)
			{
				if (achievement != null)
				{
					achievement.Cleanup();
				}
			}
		}

		public bool IsUnlocked(string achievementId)
		{
			return unlockedAchievements.GetValueOrDefault(achievementId, false);
		}

		public bool UnlockAchievement(string achievementId)
		{
			var achievement = GetAchievement(achievementId);

			if (achievement == null)
			{
				Debug.LogWarning($"Achievement '{achievementId}' not found!");
				return false;
			}

			return UnlockAchievement(achievement);
		}

		public bool UnlockAchievement(Achievement achievement)
		{
			if (achievement == null || IsUnlocked(achievement.Id))
				return false;

			unlockedAchievements[achievement.Id] = true;
			SaveUnlockedAchievements();

			achievement.TriggerUnlock();
			OnAchievementUnlocked?.Invoke(achievement);

			if (playUnlockSounds)
			{
				var unlockSound = achievement.UnlockSound ?  achievement.UnlockSound : achievementCollection.DefaultUnlockSound;
				AudioManager.Instance?.Play(unlockSound);
			}

			if (printDebugLogs)
				Debug.Log($"Achievement Unlocked: {achievement.Title} ({achievement.Id}) - {achievement.Description}");

			return true;
		}

		public Achievement GetAchievement(string achievementId)
		{
			return AllAchievements.FirstOrDefault(a => a != null && a.Id == achievementId);
		}

		public float GetAchievementProgress(string achievementId)
		{
			var achievement = GetAchievement(achievementId);
			return achievement?.GetProgress() ?? 0f;
		}

		public void CheckAllAchievements()
		{
			foreach (var achievement in AllAchievements)
			{
				if (achievement != null && !IsUnlocked(achievement.Id))
				{
					CheckAchievement(achievement);
				}
			}
		}

		public void CheckAchievement(Achievement achievement)
		{
			if (achievement == null || IsUnlocked(achievement.Id))
				return;

			OnAchievementProgressChanged?.Invoke(achievement);

			if (achievement.IsCompleted())
				UnlockAchievement(achievement);
		}

		public AchievementStats GetStats()
		{
			return new AchievementStats
			{
				TotalAchievements = TotalAchievements,
				UnlockedAchievements = UnlockedCount,
				CompletionPercentage = TotalAchievements > 0 ? (float)UnlockedCount / TotalAchievements : 0f,
			};
		}

		private void LoadUnlockedAchievements()
		{
			if (persistenceMode == PersistenceMode.Disabled)
				return;

			foreach (var achievement in AllAchievements)
			{
				if (achievement != null)
				{
					var key = $"Achievement_{achievement.Id}_Unlocked";
					var isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
					unlockedAchievements[achievement.Id] = isUnlocked;
				}
			}
		}

		private void SaveUnlockedAchievements()
		{
			if (persistenceMode == PersistenceMode.Disabled)
				return;

			foreach (var kvp in unlockedAchievements)
			{
				var key = $"Achievement_{kvp.Key}_Unlocked";
				PlayerPrefs.SetInt(key, kvp.Value ? 1 : 0);
			}

			PlayerPrefs.Save();
		}

		[ContextMenu("Reset All Achievements")]
		[EditorButton]
		public void ResetAllAchievements()
		{
			foreach (var achievement in AllAchievements)
			{
				if (achievement != null)
				{
					var key = $"Achievement_{achievement.Id}_Unlocked";
					PlayerPrefs.DeleteKey(key);
				}
			}

			unlockedAchievements.Clear();
			SaveUnlockedAchievements();

			foreach (var achievement in AllAchievements)
			{
				if (achievement?.Conditions != null)
				{
					foreach (var condition in achievement.Conditions)
					{
						condition.ResetData();
						condition.SaveData();
					}
				}
			}

			Debug.Log("All achievements have been reset!");
		}
	}

	[Serializable]
	public struct AchievementStats
	{
		public int TotalAchievements;
		public int UnlockedAchievements;
		public float CompletionPercentage;
	}
}