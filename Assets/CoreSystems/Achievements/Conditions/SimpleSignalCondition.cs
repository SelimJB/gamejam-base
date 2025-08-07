using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Simple Signal Condition", fileName = "SimpleSignalCondition")]
	public class SimpleSignalCondition : AchievementCondition
	{
		[SerializeField] private string expectedSignal;

		private bool isConditionMet;

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			GameEvents.OnSignalReceived += OnOnSignalReceivedSignalReceived;
		}

		private void OnOnSignalReceivedSignalReceived(string receivedSignal)
		{
			Debug.Log($"Signal received: {receivedSignal}, waiting for: {expectedSignal}");
			if (receivedSignal == expectedSignal)
				isConditionMet = true;

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