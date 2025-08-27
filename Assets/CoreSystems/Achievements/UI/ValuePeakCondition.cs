using System;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Value Peak Condition", fileName = "ValuePeakCondition")]
	public class ValuePeakCondition : AchievementCondition
	{
		[Header("Peak Collection Settings")]
		[SerializeField] private string itemType = "coin";
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
			return $"{itemType} peak: {currentPeakQuantity} / {minPeakQuantity}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

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

			// Only update if this single collection event is higher than our current peak
			if (quantity > currentPeakQuantity)
			{
				currentPeakQuantity = quantity;
				SaveData();
				EvaluateCondition();
			}
		}

		protected override string Key => $"PeakCollectionCondition_{itemType}_{GetInstanceID()}";

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

		protected override void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				description = $"Collect {minPeakQuantity} {itemType}(s) in a single collection";
			}
		}
	}
}