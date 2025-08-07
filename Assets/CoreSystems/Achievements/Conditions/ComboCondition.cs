using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Combo Condition", fileName = "ComboCondition")]
	public class ComboCondition : AchievementCondition
	{
		[Header("Combo Settings")]
		[SerializeField] private string actionType = "enemy_kill";
		[SerializeField] private int targetComboCount = 5;
		[SerializeField] private float comboTimeWindow = 3f;
		[SerializeField] private bool resetOnMiss = true;

		private int currentCombo;
		private int bestCombo;
		private float lastActionTime;

		public override bool IsConditionMet()
		{
			return bestCombo >= targetComboCount;
		}

		public override float GetProgress()
		{
			return Mathf.Clamp01((float)bestCombo / targetComboCount);
		}

		public override string GetProgressDescription()
		{
			return $"Best {actionType} combo: {bestCombo} / {targetComboCount}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			switch (actionType.ToLower())
			{
				case "item_collect":
					GameEvents.OnItemCollected += OnItemCollected;
					break;
				case "special_action":
					GameEvents.OnSpecialAction += OnSpecialAction;
					break;
			}

			GameEvents.OnPlayerDeath += OnPlayerDeath;
		}

		public override void Cleanup()
		{
			base.Cleanup();

			GameEvents.OnItemCollected -= OnItemCollected;
			GameEvents.OnSpecialAction -= OnSpecialAction;
			GameEvents.OnPlayerDeath -= OnPlayerDeath;
		}

		private void OnEnemyDefeated(string enemyType)
		{
			if (actionType.ToLower() == "enemy_kill")
			{
				ProcessComboAction();
			}
		}

		private void OnItemCollected(string itemType, int quantity)
		{
			if (actionType.ToLower() == "item_collect")
			{
				ProcessComboAction();
			}
		}

		private void OnSpecialAction(string actionId)
		{
			if (actionType.ToLower() == "special_action")
			{
				ProcessComboAction();
			}
		}

		private void OnPlayerDeath()
		{
			if (resetOnMiss)
			{
				ResetCombo();
			}
		}

		private void ProcessComboAction()
		{
			var currentTime = Time.time;

			if (currentCombo > 0 && currentTime - lastActionTime > comboTimeWindow)
			{
				if (resetOnMiss)
				{
					ResetCombo();
				}
			}

			currentCombo++;
			lastActionTime = currentTime;

			if (currentCombo > bestCombo)
			{
				bestCombo = currentCombo;
				SaveData();
			}

			EvaluateCondition();
		}

		private void ResetCombo()
		{
			currentCombo = 0;
		}

		protected override string Key => $"ComboCondition_{actionType}_{GetInstanceID()}";

		protected override void LoadConditionData()
		{
			bestCombo = PlayerPrefs.GetInt(Key, 0);
		}

		public override void ResetData()
		{
			bestCombo = 0;
		}

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetInt(Key, bestCombo);
			PlayerPrefs.Save();
		}

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				description = $"Achieve a {targetComboCount}x {actionType} combo";
			}
		}
	}
}