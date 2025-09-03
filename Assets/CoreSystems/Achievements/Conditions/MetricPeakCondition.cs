using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Condition: Metric Peak", fileName = "Condition_MetricPeak", order = 11)]
	public class MetricPeakCondition : AchievementCondition
	{
		[Header("Peak Collection Settings")]
		[SerializeField] private MetricType metric;
		[SerializeField] private float minPeakQuantity = 100;

		private float currentPeakQuantity;

		public override bool IsConditionMet()
		{
			return currentPeakQuantity >= minPeakQuantity;
		}

		public override float GetProgress()
		{
			return Mathf.Clamp01(currentPeakQuantity / minPeakQuantity);
		}

		public override string GetProgressDescription()
		{
			return $"{metric} peak: {currentPeakQuantity} / {minPeakQuantity}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			GameEvents.OnMetricIncreased += OnMetricIncreased;
		}

		public override void Cleanup()
		{
			base.Cleanup();

			GameEvents.OnMetricIncreased -= OnMetricIncreased;
		}

		private void OnMetricIncreased(MetricType _metric, int quantity)
		{
			if (_metric != metric) return;

			// Only update if this single collection event is higher than our current peak
			if (quantity > currentPeakQuantity)
			{
				currentPeakQuantity = quantity;
				SaveData();
				EvaluateCondition();
			}
		}


		protected override void LoadConditionData()
		{
			currentPeakQuantity = PlayerPrefs.GetFloat(Key, 0);
		}

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetFloat(Key, currentPeakQuantity);
			PlayerPrefs.Save();
		}

		public override void ResetData()
		{
			currentPeakQuantity = 0f;
		}

		public override string Key => $"PeakCollectionCondition_{metric}_{GetInstanceID()}";
		public override string DefaultDescription => "Collect a peak of {1} {0}(s) in a single event";
		protected override object[] GetDescriptionFormatArgs() => new object[] { metric, minPeakQuantity };
	}
}