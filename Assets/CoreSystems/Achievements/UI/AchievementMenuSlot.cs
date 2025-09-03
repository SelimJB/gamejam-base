using Coffee.UISoftMask;
using GameJam.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystems.Achievements.UI
{
	public class AchievementMenuSlot : MonoBehaviour
	{
		[SerializeField] private Image icon;
		[SerializeField] private Image iconBackground;
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI description;
		[SerializeField] private TextMeshProUGUI flavorText;
		[SerializeField] private SoftMask softMask;

		[SerializeField] private Image achievementPanel;
		[SerializeField] private GameObject hiddenIconContainer;
		[SerializeField] private string hiddenTitle = "???";
		[SerializeField] private string hiddenDescription = "???";

		[SerializeField] private UIImageGrayscale iconGrayscale;
		[SerializeField] private UIImageGrayscale iconBackgroundGrayscale;

		[SerializeField] private float lockedGrayscaleAmount = 0.86f;
		[SerializeField] private Image lockIcon;
		[SerializeField] private Image openLockIcon;
		[SerializeField] private Color unlockedPanelColor = new(0.976f, 0.976f, 0.976f, 1f);
		[SerializeField] private Color lockedPanelColor = new(0.941f, 0.941f, 0.941f, 1f);
		[SerializeField] private Color unlockedTextColor = new(0.125f, 0.125f, 0.125f, 1f);
		[SerializeField] private Color lockedTextColor = new(0.301f, 0.301f, 0.301f, 1f);

		private Achievement achievement;

		public Achievement Achievement => achievement;

		public void Initialize(Achievement achievement)
		{
			this.achievement = achievement;

			icon.sprite = achievement.Icon;
			iconBackground.color = achievement.BackgroundColor;
			icon.color = achievement.Color;

			ChangeState();
			InitializeSoftMask(achievement);
		}

		private void InitializeSoftMask(Achievement achievement)
		{
			softMask.enabled = achievement.DisplaySoftMask;
			if (!achievement.DisplaySoftMask)
				softMask.GetComponent<Image>().enabled = false;
			var padding = achievement.IconPadding;
			softMask.rectTransform.offsetMin = new Vector2(padding, padding);
			softMask.rectTransform.offsetMax = new Vector2(-padding, -padding);
		}

		public void ChangeState()
		{
			switch (achievement.State)
			{
				case AchievementState.Hidden:
					hiddenIconContainer.SetActive(true);
					icon.gameObject.SetActive(false);
					title.text = hiddenTitle;
					description.text = hiddenDescription;
					title.color = lockedTextColor;
					description.color = lockedTextColor;
					achievementPanel.color = lockedPanelColor;
					lockIcon.gameObject.SetActive(true);
					openLockIcon.gameObject.SetActive(false);
					break;
				case AchievementState.Unlocked:
					hiddenIconContainer.SetActive(false);
					icon.gameObject.SetActive(true);
					iconBackground.gameObject.SetActive(true);
					iconGrayscale.GrayscaleAmount = 0f;
					iconBackgroundGrayscale.GrayscaleAmount = 0f;
					title.text = achievement.Title;
					description.text = achievement.GetDescription();
					title.color = unlockedTextColor;
					description.color = unlockedTextColor;
					achievementPanel.color = unlockedPanelColor;
					lockIcon.gameObject.SetActive(false);
					openLockIcon.gameObject.SetActive(true);
					flavorText.color = unlockedTextColor;
					flavorText.text = achievement.FlavorText ?? "";
					break;
				case AchievementState.Locked:
					hiddenIconContainer.SetActive(false);
					icon.gameObject.SetActive(true);
					iconBackground.gameObject.SetActive(true);
					iconGrayscale.GrayscaleAmount = lockedGrayscaleAmount;
					iconBackgroundGrayscale.GrayscaleAmount = lockedGrayscaleAmount;
					title.text = achievement.Title;
					description.text = achievement.GetDescription();
					title.color = lockedTextColor;
					description.color = lockedTextColor;
					achievementPanel.color = lockedPanelColor;
					lockIcon.gameObject.SetActive(true);
					openLockIcon.gameObject.SetActive(false);
					break;
			}
		}
	}
}