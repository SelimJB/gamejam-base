using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace CoreSystems.Achievements.UI
{
	public class AchievementMenu : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Transform achievementContainer;
		[SerializeField] private GameObject achievementUIPrefab;
		[SerializeField] private TextMeshProUGUI statsText;
		[SerializeField] private TextMeshProUGUI percentStatsText;
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private Scrollbar scrollbar;
		[SerializeField] private Button backgroundPanel;

		[Header("Animation Settings")]
		[SerializeField] private float fadeDuration = 0.3f;
		[SerializeField] private Ease showEase = Ease.OutBack;
		[SerializeField] private Ease hideEase = Ease.InBack;
		[SerializeField] private Vector3 showFromScale = Vector3.zero;
		[SerializeField] private Vector3 hideToScale = Vector3.zero;

		private readonly List<AchievementMenuSlot> achievementUIList = new();
		private AchievementManager achievementManager;
		private Tween currentFadeTween;

		private void Start()
		{
			ShowInstant(false);

			achievementManager = AchievementManager.Instance;

			if (achievementManager == null)
			{
				Debug.LogError("AchievementManager not found! Make sure it's in the scene.");
				return;
			}

			AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
			AchievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;
			backgroundPanel.onClick.AddListener(Hide);

			BuildAchievementList();
			UpdateStatsDisplay();
			scrollbar.value = 1f;
		}

		private void OnDestroy()
		{
			currentFadeTween?.Kill();

			if (AchievementManager.Instance != null)
			{
				AchievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
				AchievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
			}
			
			backgroundPanel.onClick.RemoveListener(Hide);
		}

		public void Show(bool show = true)
		{
			Show(show, fadeDuration);
		}

		public void Show(bool show, float duration)
		{
			currentFadeTween?.Kill();

			if (show)
			{
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
				canvasGroup.alpha = 1f;

				transform.localScale = showFromScale;
				currentFadeTween = transform.DOScale(Vector3.one, duration)
					.SetEase(showEase)
					.OnComplete(() => currentFadeTween = null);
			}
			else
			{
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;

				currentFadeTween = transform.DOScale(hideToScale, duration)
					.SetEase(hideEase)
					.OnComplete(() =>
					{
						canvasGroup.alpha = 0f;
						currentFadeTween = null;
					});
			}
		}

		public void ToggleVisibility()
		{
			var isVisible = transform.localScale.x > 0.5f;
			Show(!isVisible);
		}

		public void Hide()
		{
			Show(false);	
		}
		
		public void ShowInstant(bool show)
		{
			currentFadeTween?.Kill();
			canvasGroup.alpha = show ? 1f : 0f;
			canvasGroup.interactable = show;
			canvasGroup.blocksRaycasts = show;
			transform.localScale = show ? Vector3.one : hideToScale;
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
			statsText.text = $"Achievements: {stats.UnlockedAchievements}/{stats.TotalAchievements}";

			if (percentStatsText != null)
				percentStatsText.text = $"{stats.CompletionPercentage:P0}";
		}

		private void UpdateAchievementUI(Achievement achievement)
		{
			var achievementUI = achievementUIList.FirstOrDefault(ui => ui.Achievement?.Id == achievement.Id);
			if (achievementUI != null)
				achievementUI.ChangeState();
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