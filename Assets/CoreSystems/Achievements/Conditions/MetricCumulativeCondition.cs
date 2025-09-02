using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Condition: Metric Cumulative", fileName = "Condition_MetricCumulative", order = 10)]
	public class MetricCumulativeCondition : AchievementCondition
	{
		[Header("Collection Settings")]
		[SerializeField] private MetricType metric;
		[SerializeField] private int targetQuantity = 100;

		private int currentQuantity;

		public override bool IsConditionMet()
		{
			return currentQuantity >= targetQuantity;
		}

		public override float GetProgress()
		{
			return Mathf.Clamp01((float)currentQuantity / targetQuantity);
		}

		public override string GetProgressDescription()
		{
			return $"{metric}: {currentQuantity} / {targetQuantity}";
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

			currentQuantity += quantity;

			SaveData();

			EvaluateCondition();
		}

		protected override string Key => $"CollectionCondition_{metric}_{GetInstanceID()}";

		protected override void LoadConditionData()
		{
			currentQuantity = PlayerPrefs.GetInt(Key, 0);
		}

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetInt(Key, currentQuantity);
			PlayerPrefs.Save();
		}

		public override void ResetData()
		{
			currentQuantity = 0;
		}

		protected override void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				description = $"Collect {targetQuantity} {metric}(s)";
			}
		}
	}
}