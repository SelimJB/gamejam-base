using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace CoreSystems.Achievements.UI
{
	public class AchievementMenu : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Transform achievementContainer;
		[SerializeField] private GameObject achievementUIPrefab;
		[SerializeField] private TextMeshProUGUI statsText;
		
		private readonly List<AchievementMenuSlot> achievementUIList = new();
		private AchievementManager achievementManager;

		private void Start()
		{
			achievementManager = AchievementManager.Instance;

			if (achievementManager == null)
			{
				Debug.LogError("AchievementManager not found! Make sure it's in the scene.");
				return;
			}

			AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
			AchievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;

			BuildAchievementList();
			UpdateStatsDisplay();
		}

		private void OnDestroy()
		{
			if (AchievementManager.Instance != null)
			{
				AchievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
				AchievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
			}
		}


		private void BuildAchievementList()
		{
			if (achievementManager == null || achievementUIPrefab == null || achievementContainer == null)
				return;


			ClearAchievementList();


			var achievements = achievementManager.AllAchievements;


			foreach (var achievement in achievements)
			{
				var achievementUIObj = Instantiate(achievementUIPrefab, achievementContainer);
				var achievementUI = achievementUIObj.GetComponent<AchievementMenuSlot>();

				if (achievementUI != null)
				{
					bool isUnlocked = achievementManager.IsUnlocked(achievement.Id);
					achievementUI.Initialize(achievement);
					achievementUIList.Add(achievementUI);
				}
			}

		}

		private void ClearAchievementList()
		{
			foreach (var achievementUI in achievementUIList)
			{
				if (achievementUI != null && achievementUI.gameObject != null)
				{
					Destroy(achievementUI.gameObject);
				}
			}

			achievementUIList.Clear();
		}

		private void UpdateStatsDisplay()
		{
			if (achievementManager == null || statsText == null)
				return;

			var stats = achievementManager.GetStats();
			statsText.text = $"Achievements: {stats.UnlockedAchievements}/{stats.TotalAchievements} " +
			                 $"({stats.CompletionPercentage:P0})";
		}
		
		private void UpdateAchievementUI(Achievement achievement)
		{
			var achievementUI = achievementUIList.FirstOrDefault(ui => ui.Achievement?.Id == achievement.Id);
			if (achievementUI != null)
			{
				var isUnlocked = achievementManager.IsUnlocked(achievement.Id);
				achievementUI.State = isUnlocked ? AchievementState.Unlocked : AchievementState.Locked;
			}
		}

		[ContextMenu("Refresh Achievement List")]
		public void RefreshList()
		{
			BuildAchievementList();
			UpdateStatsDisplay();
		}

		private void OnAchievementUnlocked(Achievement achievement)
		{
			UpdateAchievementUI(achievement);
			UpdateStatsDisplay();

			BuildAchievementList();
		}

		private void OnAchievementProgressChanged(Achievement achievement)
		{
			UpdateAchievementUI(achievement);

			if (achievement.GetProgress() > 0f)
			{
				var achievementUI = achievementUIList.FirstOrDefault(ui => ui.Achievement?.Id == achievement.Id);
				if (achievementUI == null)
				{
					BuildAchievementList();
				}
			}
		}
	}
}