using UnityEngine;

namespace CoreSystems.Achievements.UI
{
	public class AchievementNotificationController : MonoBehaviour
	{
		[SerializeField] private AchievementNotification prefab;
		[SerializeField] private Canvas canvas;
		[SerializeField] private AchievementMenu achievementMenu;
		[SerializeField] private bool openMenuOnNotificationClick = true;

		private AchievementManager achievementManager;

		private void Start()
		{
			achievementManager = AchievementManager.Instance;

			if (achievementManager == null)
			{
				Debug.LogError("AchievementManager not found! Make sure it's in the scene.");
				return;
			}

			if (openMenuOnNotificationClick && achievementMenu == null)
			{
				achievementMenu = FindObjectOfType<AchievementMenu>();
				if (achievementMenu == null)
				{
					Debug.LogWarning("AchievementMenu not found in the scene! Notifications will not open the menu.");
					openMenuOnNotificationClick = false;
				}

				return;
			}

			AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
		}

		private void OnAchievementUnlocked(Achievement obj)
		{
			if (prefab == null)
			{
				Debug.LogError("AchievementNotification prefab is not assigned!");
				return;
			}

			if (canvas == null)
			{
				Debug.LogError("Canvas is not assigned for AchievementNotificationController!");
				return;
			}

			var notification = Instantiate(prefab, canvas.transform);
			notification.Initialize(obj);
			notification.ShowNotification();

			if (achievementMenu != null && openMenuOnNotificationClick)
			{
				notification.onPointerClick += () => { achievementMenu.Show(); };
			}
		}
	}
}