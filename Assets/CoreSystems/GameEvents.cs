using System;

namespace CoreSystems
{
	public static class GameEvents
	{
		public static event Action<int> OnScoreChanged;
		public static event Action<int> OnScoreIncreased;
		public static event Action<float> OnTimeElapsed;
		public static event Action<float> OnSurvivalTime;
		public static event Action<string, int> OnItemCollected;
		public static event Action<string> OnSpecialItemCollected;
		public static event Action OnPlayerDeath;
		public static event Action OnLevelCompleted;
		public static event Action<int> OnLevelUp;
		public static event Action<string> OnUpgradeUnlocked;
		public static event Action<string> OnSpecialAction;
		public static event Action<string, float> OnStatChanged;
		public static event Action<string> OnSignalReceived;

		// Don't hesitate to create nested namespaces for better organization.
		// public static class GAMENAMESPACE
		// {
		// 	public static event Action<int> OnDamageDealt;
		// }

		public static void ClearAllEvents()
		{
			OnScoreChanged = null;
			OnScoreIncreased = null;
			OnTimeElapsed = null;
			OnSurvivalTime = null;
			OnItemCollected = null;
			OnSpecialItemCollected = null;
			OnPlayerDeath = null;
			OnLevelCompleted = null;
			OnLevelUp = null;
			OnUpgradeUnlocked = null;
			OnSpecialAction = null;
			OnStatChanged = null;
			OnSignalReceived = null;
		}

		public static void TriggerScoreChanged(int newScore)
		{
			OnScoreChanged?.Invoke(newScore);
		}

		public static void TriggerScoreIncreased(int increment)
		{
			OnScoreIncreased?.Invoke(increment);
		}

		public static void TriggerTimeElapsed(float time)
		{
			OnTimeElapsed?.Invoke(time);
		}

		public static void TriggerSurvivalTime(float survivalTime)
		{
			OnSurvivalTime?.Invoke(survivalTime);
		}

		public static void TriggerItemCollected(string itemType, int quantity = 1)
		{
			OnItemCollected?.Invoke(itemType, quantity);
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

		public static void TriggerSpecialAction(string actionId)
		{
			OnSpecialAction?.Invoke(actionId);
		}

		public static void TriggerStatChanged(string statName, float newValue)
		{
			OnStatChanged?.Invoke(statName, newValue);
		}

		public static void TriggerSignal(string signal)
		{
			OnSignalReceived?.Invoke(signal);
		}
	}
}