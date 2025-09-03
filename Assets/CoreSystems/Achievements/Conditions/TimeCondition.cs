using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Condition: Time", fileName = "Condition_Time", order = 13)]
	public class TimeCondition : AchievementCondition
	{
		[SerializeField] private float targetTime = 60f;
		[SerializeField] private TimeType timeType = TimeType.Survival;

		public enum TimeType
		{
			Survival,
			Cumulative
		}

		private float currentTime;

		public override bool IsConditionMet()
		{
			return currentTime >= targetTime;
		}

		public override float GetProgress()
		{
			return Mathf.Clamp01(currentTime / targetTime);
		}

		public override string GetProgressDescription()
		{
			var timeString = FormatTime(currentTime);
			var targetString = FormatTime(targetTime);

			var typeLabel = timeType == TimeType.Survival ? "Survival" : "Total";
			return $"{typeLabel} Time: {timeString} / {targetString}";
		}

		public override void Initialize(bool persistProgress)
		{
			var actualPersistProgress = timeType == TimeType.Cumulative && persistProgress;

			base.Initialize(actualPersistProgress);

			GameEvents.OnTimeElapsed += OnTimeElapsed;
			GameEvents.OnPlayerDeath += OnPlayerDeath;
		}

		public override void Cleanup()
		{
			base.Cleanup();

			GameEvents.OnTimeElapsed -= OnTimeElapsed;
			GameEvents.OnPlayerDeath -= OnPlayerDeath;
		}

		private void OnTimeElapsed(float elapsedTime)
		{
			currentTime += elapsedTime;

			if (timeType == TimeType.Cumulative)
				SaveData();

			EvaluateCondition();
		}


		private void OnPlayerDeath()
		{
			if (timeType == TimeType.Survival)
			{
				currentTime = 0f;
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

		protected override void LoadConditionData()
		{
			if (timeType == TimeType.Cumulative)
			{
				currentTime = PlayerPrefs.GetFloat(Key, 0f);
			}
		}

		public override void ResetData()
		{
			currentTime = 0f;
		}

		protected override void SaveConditionData()
		{
			if (timeType == TimeType.Cumulative)
			{
				PlayerPrefs.SetFloat(Key, currentTime);
				PlayerPrefs.Save();
			}
		}

		public override string Key =>
			timeType == TimeType.Survival ? $"TimeCondition_{GetInstanceID()}" : $"TimeCondition_{GetInstanceID()}_Cumulative";

		public override string DefaultDescription => timeType == TimeType.Survival
			? "Survive for {1}"
			: "Play for a total of {1}";

		protected override object[] GetDescriptionFormatArgs()
		{
			return new object[]
			{
				FormatTime(currentTime),
				FormatTime(targetTime),
				GetProgress() * 100f,
				timeType.ToString(),
				currentTime,
				targetTime
			};
		}
	}
}