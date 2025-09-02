using System;

namespace CoreSystems
{
	public static class GameEvents
	{
		public static event Action<int> OnScoreIncreased;
		public static event Action<float> OnTimeElapsed;
		public static event Action<float> OnSurvivalTime;
		public static event Action<string, int> OnMetricIncreased;
		public static event Action<string> OnSpecialItemCollected;
		public static event Action OnPlayerDeath;
		public static event Action<int> OnLevelUp;
		public static event Action<string> OnUpgradeUnlocked;
		public static event Action<string, float> OnStatChanged;
		public static event Action<string> OnMilestoneReached;

		// Don't hesitate to create nested namespaces for better organization.
		// public static class GAMENAMESPACE
		// {
		// 	public static event Action<int> OnDamageDealt;
		// }

		public static void ClearAllEvents()
		{
			OnScoreIncreased = null;
			OnTimeElapsed = null;
			OnSurvivalTime = null;
			OnMetricIncreased = null;
			OnSpecialItemCollected = null;
			OnPlayerDeath = null;
			OnLevelUp = null;
			OnUpgradeUnlocked = null;
			OnStatChanged = null;
			OnMilestoneReached = null;
		}

		public static void TriggerScoreChanged(int newScore)
		{
		}

		public static void TriggerScoreIncreased(int increment)
		{
			OnScoreIncreased?.Invoke(increment);
		}

		public static void TriggerTimeElapsed(float time)
		{
			OnTimeElapsed?.Invoke(time);
		}

// TODO CLEAN ET CHANGER LE RESTE
		public static void TriggerSurvivalTime(float survivalTime)
		{
			OnSurvivalTime?.Invoke(survivalTime);
		}

		public static void ReportMetricIncrease(string itemType, int quantity = 1)
		{
			OnMetricIncreased?.Invoke(itemType, quantity);
		}

		public static void TriggerSpecialItemCollected(string itemId)
		{
			OnSpecialItemCollected?.Invoke(itemId);
		}

		public static void TriggerPlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}

		public static void TriggerLevelUp(int newLevel)
		{
			OnLevelUp?.Invoke(newLevel);
		}

		public static void TriggerUpgradeUnlocked(string upgradeId)
		{
			OnUpgradeUnlocked?.Invoke(upgradeId);
		}

		public static void TriggerStatChanged(string statName, float newValue)
		{
			OnStatChanged?.Invoke(statName, newValue);
		}

		public static void TriggerMilestone(string signal)
		{
			OnMilestoneReached?.Invoke(signal);
		}
	}
}