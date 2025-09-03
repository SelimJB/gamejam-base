using UnityEditor;
using UnityEngine;

namespace CoreSystems.Achievements.Editor
{
	[CustomEditor(typeof(AchievementCondition), true)]
	public class AchievementConditionEditor : UnityEditor.Editor
	{
		private static readonly string customDescriptiontemplate = "customDescriptionTemplate";
		private static readonly string overideDescription = "overrideDescription";

		public override void OnInspectorGUI()
		{
			var condition = (AchievementCondition)target;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Key:", EditorStyles.boldLabel, GUILayout.Width(80));
			EditorGUILayout.LabelField(condition.Key);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel, GUILayout.Width(80));
			EditorGUILayout.LabelField(condition.Description);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(5);

			serializedObject.Update();

			var property = serializedObject.GetIterator();
			property.NextVisible(true);

			while (property.NextVisible(false))
			{
				if (property.name == customDescriptiontemplate)
				{
					var overrideDescProp = serializedObject.FindProperty(overideDescription);
					if (overrideDescProp != null && overrideDescProp.boolValue)
					{
						EditorGUILayout.PropertyField(property, new GUIContent("Custom Description"));

						EditorGUILayout.HelpBox("You can use placeholders like {0}, {1}, {2}... in the description.\n" +
						                        "Available placeholders depend on the condition type.\n" +
						                        "Example: \"Progress: {0} / {1}\"",
							MessageType.Info);
					}
				}
				else
				{
					EditorGUILayout.PropertyField(property, true);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}