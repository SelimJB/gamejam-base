using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Value Timed Condition", fileName = "ValueTimedCondition")]
	public class ValueTimedCondition : AchievementCondition
	{
		[Header("Timed Cumulative Settings")]
		[SerializeField] private string itemType = "coin";
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
			CleanupOldEvents();
			return $"{itemType}: {currentCumulativeQuantity}/{targetQuantity} (in {timeLimit}s)";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(false);
			GameEvents.OnItemCollected += OnItemCollected;
		}

		public override void Cleanup()
		{
			base.Cleanup();
			GameEvents.OnItemCollected -= OnItemCollected;
		}

		private void OnItemCollected(string collectedItemType, int quantity)
		{
			if (!collectedItemType.Equals(itemType, StringComparison.OrdinalIgnoreCase)) return;

			float currentTime = Time.time;
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

		protected override string Key => $"TimedCumulativeCondition_{itemType}_{GetInstanceID()}";

		protected override void LoadConditionData() { }

		protected override void SaveConditionData() { }

		public override void ResetData()
		{
			recentEvents.Clear();
			currentCumulativeQuantity = 0;
		}

		protected override void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				description = $"Collect {targetQuantity} {itemType}(s) within {timeLimit} seconds";
			}
		}
	}
}