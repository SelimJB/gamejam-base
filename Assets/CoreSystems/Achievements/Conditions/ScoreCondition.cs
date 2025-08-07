using UnityEngine;

namespace CoreSystems.Achievements
{
	[CreateAssetMenu(menuName = "ScriptableObjects/Achievements/Conditions/Score Condition", fileName = "ScoreCondition")]
	public class ScoreCondition : AchievementCondition
	{
		[Header("Score Settings")]
		[SerializeField] private int targetScore = 1000;
		[SerializeField] private bool trackHighestScore = true;

		private int currentScore;
		private int highestScore;

		public override bool IsConditionMet()
		{
			var scoreToCheck = trackHighestScore ? highestScore : currentScore;
			return scoreToCheck >= targetScore;
		}

		public override float GetProgress()
		{
			var scoreToCheck = trackHighestScore ? highestScore : currentScore;
			return Mathf.Clamp01((float)scoreToCheck / targetScore);
		}

		public override string GetProgressDescription()
		{
			var scoreToCheck = trackHighestScore ? highestScore : currentScore;
			return $"Score: {scoreToCheck:N0} / {targetScore:N0}";
		}

		public override void Initialize(bool persistProgress)
		{
			base.Initialize(persistProgress);

			GameEvents.OnScoreChanged += OnScoreChanged;
		}

		public override void Cleanup()
		{
			base.Cleanup();

			GameEvents.OnScoreChanged -= OnScoreChanged;
		}

		private void OnScoreChanged(int newScore)
		{
			currentScore = newScore;

			if (newScore > highestScore)
			{
				highestScore = newScore;
				SaveData();
			}
			
			EvaluateCondition();
		}

		protected override string Key => $"ScoreCondition_{GetInstanceID()}_HighestScore";
		
		protected override void LoadConditionData()
		{
			highestScore = PlayerPrefs.GetInt(Key, 0);
		}

		public override void ResetData()
		{
			currentScore = 0;
			highestScore = 0;

		}

		protected override void SaveConditionData()
		{
			PlayerPrefs.SetInt(Key, highestScore);
			PlayerPrefs.Save();
		}

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(description))
			{
				description = $"Reach a score of {targetScore:N0}";
			}
		}
	}
}