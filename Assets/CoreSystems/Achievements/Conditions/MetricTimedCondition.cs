using System.Collections.Generic;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Condition: Metric Timed", fileName = "Condition_MetricTimed", order = 11)]
	public class MetricTimedCondition : AchievementCondition
	{
		[SerializeField] private MetricType metric;
		[SerializeField] private int targetQuantity = 50;
		[SerializeField] private float timeLimit = 30f;

		private readonly List<(float timestamp, int quantity)> recentEvents = new();
		private int currentCumulativeQuantity;

		public override bool IsConditionMet()
		{
			CleanupOldEvents();
			return currentCumulativeQuantity >= targetQuantity;
		}

		public override float GetProgress()
		{
			CleanupOldEvents();
			return Mathf.Clamp01((float)currentCumulativeQuantity / targetQuantity);
		}

		public override string GetProgressDescription()
		{
			return $"{metric}: {currentCumulativeQuantity}/{targetQuantity} (in {timeLimit}s)";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(false);
			GameEvents.OnMetricIncreased += OnMetricIncreased;
		}

		public override void Cleanup()
		{
			base.Cleanup();
			GameEvents.OnMetricIncreased -= OnMetricIncreased;
		}

		private void OnMetricIncreased(MetricType _metric, int quantity)
		{
			if (metric != _metric) return;

			var currentTime = Time.time;
			recentEvents.Add((currentTime, quantity));

			CleanupOldEvents();
			RecalculateCumulative();

			EvaluateCondition();
		}

		private void CleanupOldEvents()
		{
			var currentTime = Time.time;
			var cutoffTime = currentTime - timeLimit;

			recentEvents.RemoveAll(evt => evt.timestamp < cutoffTime);
		}

		private void RecalculateCumulative()
		{
			currentCumulativeQuantity = 0;
			foreach (var evt in recentEvents)
			{
				currentCumulativeQuantity += evt.quantity;
			}
		}
		
		protected override void LoadConditionData() { }

		protected override void SaveConditionData() { }

		public override void ResetData()
		{
			recentEvents.Clear();
			currentCumulativeQuantity = 0;
		}
		
		public override string Key => $"TimedCumulativeCondition_{metric}_{GetInstanceID()}";
		public override string DefaultDescription => "Collect {1} {0} within {2} seconds";
		protected override object[] GetDescriptionFormatArgs() => new object[] { targetQuantity > 1 ? $"{metric}s" : metric, targetQuantity, timeLimit };
	}
}