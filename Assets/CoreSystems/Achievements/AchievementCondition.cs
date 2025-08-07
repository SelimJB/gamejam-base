using System;
using UnityEngine;

namespace CoreSystems.Achievements
{
	public abstract class AchievementCondition : ScriptableObject
	{
		[Header("Condition Info")]
		[SerializeField] protected string conditionName;
		[SerializeField, TextArea(2, 3)] protected string description;

		public Action OnConditionMet;

		protected bool persistentProgress;

		public string ConditionName => conditionName;
		public string Description => description;

		public abstract bool IsConditionMet();
		public abstract float GetProgress();

		public virtual string GetProgressDescription() => $"Progress description not implemented for {conditionName}";

		public virtual void Initialize(bool persistentProgress)
		{
			this.persistentProgress = persistentProgress;

			if (this.persistentProgress) LoadData();
			else ResetData();
		}

		/// <summary>
		/// Should be called by the inheriting class when progress is made to check if the condition is met and invoke the OnConditionMet event. 
		/// </summary>
		protected void EvaluateCondition()
		{
			if (IsConditionMet())
				OnConditionMet?.Invoke();
		}

		public virtual void Cleanup()
		{
			OnConditionMet = null;
		}

		protected virtual void OnValidate()
		{
			if (string.IsNullOrEmpty(conditionName))
			{
				conditionName = GetType().Name.Replace("Condition", "");
			}
		}

		public void SaveData()
		{
			if (!persistentProgress) return;

			SaveConditionData();
		}

		private void LoadData()
		{
			if (!persistentProgress) return;

			LoadConditionData();
		}

		protected virtual string Key => $"Condition_{conditionName}_{GetInstanceID()}";
		protected abstract void SaveConditionData();
		protected abstract void LoadConditionData();
		public abstract void ResetData();
	}
}