using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Condition: Milestone", fileName = "Condition_Milestone", order = 12)]
	public class MilestoneCondition : AchievementCondition
	{
		[SerializeField] private string expectedSignal;

		private bool isConditionMet;

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			GameEvents.OnMilestoneReached += OnOnMilestoneReachedMilestoneReached;
		}

		private void OnOnMilestoneReachedMilestoneReached(string receivedSignal)
		{
			if (receivedSignal == expectedSignal)
			{
				isConditionMet = true;
				SaveData();
			}

			EvaluateCondition();
		}

		public override bool IsConditionMet()
		{
			return isConditionMet;
		}

		public override float GetProgress()
		{
			return isConditionMet ? 1f : 0f;
		}

		protected override string Key => $"SignalCondition_{expectedSignal}_{GetInstanceID()}";

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetInt(Key, isConditionMet ? 1 : 0);
			PlayerPrefs.Save();
		}

		protected override void LoadConditionData()
		{
			isConditionMet = PlayerPrefs.GetInt(Key, 0) == 1;
		}

		public override void ResetData()
		{
			isConditionMet = false;
		}
	}
}