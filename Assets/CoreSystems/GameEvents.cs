using System;
using CoreSystems.Achievements;

namespace CoreSystems
{
	public static class GameEvents
	{
		public static event Action<float> OnTimeElapsed;
		public static event Action<MetricType, int> OnMetricIncreased;
		public static event Action<MilestoneType> OnMilestoneReached;
		public static event Action OnPlayerDeath;

		public static void ClearAllEvents()
		{
			OnTimeElapsed = null;
			OnMetricIncreased = null;
			OnMilestoneReached = null;
			OnPlayerDeath = null;
		}

		public static void ReportMetricIncrease(MetricType metric, int quantity = 1)
		{
			OnMetricIncreased?.Invoke(metric, quantity);
		}

		public static void ReportMilestone(MilestoneType signal)
		{
			OnMilestoneReached?.Invoke(signal);
		}

		public static void ReportTimeElapsed(float time)
		{
			OnTimeElapsed?.Invoke(time);
		}

		public static void ReportPlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}
	}
}