using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystems.Achievements.UI
{
	public enum AchievementState
	{
		Hidden,
		Unlocked,
		Locked,
	}

	public class AchievementMenuSlot : MonoBehaviour
	{
		[SerializeField] private Image icon;
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI description;

		private Achievement achievement;

		public Achievement Achievement => achievement;
		public AchievementState State { get; set; } = AchievementState.Locked;

		public void Initialize(Achievement achievement)
		{
			this.achievement = achievement;

			icon.sprite = achievement.Icon;
			title.text = achievement.Title;
			description.text = achievement.Description;
		}
	}
}