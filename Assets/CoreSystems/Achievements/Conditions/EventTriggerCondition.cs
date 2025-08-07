using System;
using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Game Event Condition", fileName = "GameEventCondition")]
	public class EventTriggerCondition : AchievementCondition
	{
		[Header("Event Settings")]
		[SerializeField] private GameEventType eventType = GameEventType.PlayerDeath;
		[SerializeField] private int requiredTriggers = 1;
		[SerializeField] private bool resetOnRestart = true;

		[Header("Optional Filters (for events with parameters)")]
		[SerializeField] private string stringParameter = "";
		[SerializeField] private int intParameter;
		[SerializeField] private bool useStringFilter;
		[SerializeField] private bool useIntFilter;

		private int currentTriggers;

		public enum GameEventType
		{
			PlayerDeath,
			LevelCompleted,
			ScoreChanged,
			ScoreIncreased,
			TimeElapsed,
			SurvivalTime,
			ItemCollected,
			SpecialItemCollected,
			LevelUp,
			UpgradeUnlocked,
			SpecialAction,
			StatChanged
		}

		public override bool IsConditionMet()
		{
			return currentTriggers >= requiredTriggers;
		}

		public override float GetProgress()
		{
			return Mathf.Clamp01((float)currentTriggers / requiredTriggers);
		}

		public override string GetProgressDescription()
		{
			return $"Event '{eventType}' triggered: {currentTriggers} / {requiredTriggers}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			if (resetOnRestart && !persistProgress)
			{
				currentTriggers = 0;
			}

			SubscribeToEvent();
		}

		public override void Cleanup()
		{
			base.Cleanup();
			UnsubscribeFromEvent();
		}

		private void SubscribeToEvent()
		{
			switch (eventType)
			{
				case GameEventType.PlayerDeath:
					GameEvents.OnPlayerDeath += OnSimpleEvent;
					break;
				case GameEventType.LevelCompleted:
					GameEvents.OnLevelCompleted += OnSimpleEvent;
					break;
				case GameEventType.ScoreChanged:
					GameEvents.OnScoreChanged += OnIntEvent;
					break;
				case GameEventType.ScoreIncreased:
					GameEvents.OnScoreIncreased += OnIntEvent;
					break;
				case GameEventType.TimeElapsed:
					GameEvents.OnTimeElapsed += OnFloatEvent;
					break;
				case GameEventType.SurvivalTime:
					GameEvents.OnSurvivalTime += OnFloatEvent;
					break;
				case GameEventType.ItemCollected:
					GameEvents.OnItemCollected += OnItemCollectedEvent;
					break;
				case GameEventType.SpecialItemCollected:
					GameEvents.OnSpecialItemCollected += OnStringEvent;
					break;
				case GameEventType.LevelUp:
					GameEvents.OnLevelUp += OnIntEvent;
					break;
				case GameEventType.UpgradeUnlocked:
					GameEvents.OnUpgradeUnlocked += OnStringEvent;
					break;
				case GameEventType.SpecialAction:
					GameEvents.OnSpecialAction += OnStringEvent;
					break;
				case GameEventType.StatChanged:
					GameEvents.OnStatChanged += OnStatChangedEvent;
					break;
			}
		}

		private void UnsubscribeFromEvent()
		{
			switch (eventType)
			{
				case GameEventType.PlayerDeath:
					GameEvents.OnPlayerDeath -= OnSimpleEvent;
					break;
				case GameEventType.LevelCompleted:
					GameEvents.OnLevelCompleted -= OnSimpleEvent;
					break;
				case GameEventType.ScoreChanged:
					GameEvents.OnScoreChanged -= OnIntEvent;
					break;
				case GameEventType.ScoreIncreased:
					GameEvents.OnScoreIncreased -= OnIntEvent;
					break;
				case GameEventType.TimeElapsed:
					GameEvents.OnTimeElapsed -= OnFloatEvent;
					break;
				case GameEventType.SurvivalTime:
					GameEvents.OnSurvivalTime -= OnFloatEvent;
					break;
				case GameEventType.ItemCollected:
					GameEvents.OnItemCollected -= OnItemCollectedEvent;
					break;
				case GameEventType.SpecialItemCollected:
					GameEvents.OnSpecialItemCollected -= OnStringEvent;
					break;
				case GameEventType.LevelUp:
					GameEvents.OnLevelUp -= OnIntEvent;
					break;
				case GameEventType.UpgradeUnlocked:
					GameEvents.OnUpgradeUnlocked -= OnStringEvent;
					break;
				case GameEventType.SpecialAction:
					GameEvents.OnSpecialAction -= OnStringEvent;
					break;
				case GameEventType.StatChanged:
					GameEvents.OnStatChanged -= OnStatChangedEvent;
					break;
			}
		}

		private void OnSimpleEvent()
		{
			TriggerEvent();
		}

		private void OnIntEvent(int value)
		{
			if (useIntFilter && value != intParameter)
				return;

			TriggerEvent();
		}

		private void OnFloatEvent(float value)
		{
			if (useIntFilter && value < intParameter)
				return;

			TriggerEvent();
		}

		private void OnStringEvent(string value)
		{
			if (useStringFilter && !string.Equals(value, stringParameter, StringComparison.OrdinalIgnoreCase))
				return;

			TriggerEvent();
		}

		private void OnItemCollectedEvent(string itemType, int quantity)
		{
			if (useStringFilter && !string.Equals(itemType, stringParameter, StringComparison.OrdinalIgnoreCase))
				return;

			if (useIntFilter && quantity < intParameter)
				return;

			TriggerEvent();
		}

		private void OnStatChangedEvent(string statName, float newValue)
		{
			if (useStringFilter && !string.Equals(statName, stringParameter, StringComparison.OrdinalIgnoreCase))
				return;

			if (useIntFilter && newValue < intParameter)
				return;

			TriggerEvent();
		}

		private void TriggerEvent()
		{
			currentTriggers++;
			SaveData();

			EvaluateCondition();
		}

		protected override string Key => $"GameEvent_{eventType}_{GetInstanceID()}_Triggers";

		protected override void LoadConditionData()
		{
			currentTriggers = PlayerPrefs.GetInt(Key, 0);
		}

		protected override void SaveConditionData()
		{
			if (!persistentProgress) return;

			PlayerPrefs.SetInt(Key, currentTriggers);
			PlayerPrefs.Save();
		}

		public override void ResetData()
		{
			currentTriggers = 0;
		}

		protected override void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				var filterText = "";
				if (useStringFilter) filterText += $" (with '{stringParameter}')";
				if (useIntFilter) filterText += $" (value >= {intParameter})";

				description = $"Trigger '{eventType}' event {requiredTriggers} time(s){filterText}";
			}
		}
	}
}