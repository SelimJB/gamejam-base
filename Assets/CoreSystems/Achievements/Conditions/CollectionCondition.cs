using System;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Collection Condition", fileName = "CollectionCondition")]
	public class CollectionCondition : AchievementCondition
	{
		[Header("Collection Settings")]
		[SerializeField] private string itemType = "coin"; // Replace with a more specific type or enum when using in a real project
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
			return $"{itemType}: {currentQuantity} / {targetQuantity}";
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

			currentQuantity += quantity;

			SaveData();

			EvaluateCondition();
		}

		protected override string Key => $"CollectionCondition_{itemType}_{GetInstanceID()}";

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
				description = $"Collect {targetQuantity} {itemType}(s)";
			}
		}
	}
}