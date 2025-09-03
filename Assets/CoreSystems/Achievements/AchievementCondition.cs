using System;
using UnityEngine;

namespace CoreSystems.Achievements
{
	public abstract class AchievementCondition : ScriptableObject
	{
		[SerializeField] protected bool overrideDescription;
		[SerializeField, TextArea(2, 3)] protected string customDescriptionTemplate;

		public Action OnConditionMet;

		protected bool persistentProgress;

		public string Description => FormatDescriptionText(overrideDescription ? customDescriptionTemplate : DefaultDescription);

		public abstract string DefaultDescription { get; }
		public abstract bool IsConditionMet();
		public abstract float GetProgress();
		public virtual string GetProgressDescription() => $"Progress description not implemented for {Key}";
		protected abstract void SaveConditionData();
		protected abstract void LoadConditionData();
		public abstract void ResetData();

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

		public virtual string Key => $"DefaultConditionKey_{name}_{GetInstanceID()}";

		/// <summary>
		/// Override this method to provide formatting arguments for the description text.
		/// </summary>
		protected virtual object[] GetDescriptionFormatArgs()
		{
			return new object[] { };
		}

		/// <summary>
		/// Formats the description text with placeholders like {0}, {1}, etc.
		/// </summary>
		protected virtual string FormatDescriptionText(string text)
		{
			try
			{
				var args = GetDescriptionFormatArgs();
				return string.Format(text, args);
			}
			catch (FormatException)
			{
				return text;
			}
		}
	}
}