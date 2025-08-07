using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Time Condition", fileName = "TimeCondition")]
	public class TimeCondition : AchievementCondition
	{
		[Header("Time Settings")]
		[SerializeField] private float targetTime = 60f;
		[SerializeField] private TimeType timeType = TimeType.Survival;
		[SerializeField] private bool trackBestTime = true;

		public enum TimeType
		{
			Survival,
			Cumulative
		}

		private float currentTime;
		private float bestTime;
		private float cumulativeTime;

		public override bool IsConditionMet()
		{
			switch (timeType)
			{
				case TimeType.Survival:
					return (trackBestTime ? bestTime : currentTime) >= targetTime;
				case TimeType.Cumulative:
					return cumulativeTime >= targetTime;
				default:
					return false;
			}
		}

		public override float GetProgress()
		{
			var timeToCheck = GetRelevantTime();
			return Mathf.Clamp01(timeToCheck / targetTime);
		}

		public override string GetProgressDescription()
		{
			var timeToCheck = GetRelevantTime();
			var timeString = FormatTime(timeToCheck);
			var targetString = FormatTime(targetTime);

			var typeLabel = timeType == TimeType.Survival ? "Survival" : "Total";
			return $"{typeLabel} Time: {timeString} / {targetString}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			GameEvents.OnSurvivalTime += OnSurvivalTime;
			GameEvents.OnTimeElapsed += OnTimeElapsed;
			GameEvents.OnPlayerDeath += OnPlayerDeath;
		}

		public override void Cleanup()
		{
			base.Cleanup();

			GameEvents.OnSurvivalTime -= OnSurvivalTime;
			GameEvents.OnTimeElapsed -= OnTimeElapsed;
			GameEvents.OnPlayerDeath -= OnPlayerDeath;
		}

		private void OnSurvivalTime(float survivalTime)
		{
			if (timeType == TimeType.Survival)
			{
				currentTime = survivalTime;
			}
		}

		private void OnTimeElapsed(float elapsedTime)
		{
			if (timeType == TimeType.Cumulative)
			{
				cumulativeTime += elapsedTime;
				SaveData();
			}

			EvaluateCondition();
		}

		private void OnPlayerDeath()
		{
			if (timeType == TimeType.Survival && currentTime > bestTime)
			{
				bestTime = currentTime;
				SaveData();
			}

			EvaluateCondition();
		}

		private float GetRelevantTime()
		{
			switch (timeType)
			{
				case TimeType.Survival:
					return trackBestTime ? bestTime : currentTime;
				case TimeType.Cumulative:
					return cumulativeTime;
				default:
					return 0f;
			}
		}

		private string FormatTime(float time)
		{
			var minutes = Mathf.FloorToInt(time / 60f);
			var seconds = Mathf.FloorToInt(time % 60f);

			if (minutes > 0)
				return $"{minutes}m {seconds:00}s";

			return $"{seconds}s";
		}

		private string KeyBest => $"TimeCondition_{GetInstanceID()}";
		private string KeyCumulative => $"TimeCondition_{GetInstanceID()}_Cumulative";


		protected override void LoadConditionData()
		{
			bestTime = PlayerPrefs.GetFloat(KeyBest, 0f);
			cumulativeTime = PlayerPrefs.GetFloat(KeyCumulative, 0f);
		}

		public override void ResetData()
		{
			currentTime = 0f;
			bestTime = 0f;
			cumulativeTime = 0f;
		}

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetFloat(KeyBest, bestTime);
			PlayerPrefs.SetFloat(KeyCumulative, cumulativeTime);
			PlayerPrefs.Save();
		}

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				var typeLabel = timeType == TimeType.Survival ? "Survive for" : "Play for a total of";
				description = $"{typeLabel} {FormatTime(targetTime)}";
			}
		}
	}
}