using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CoreSystems.Achievements.Editor
{
	[CustomEditor(typeof(AchievementCondition), true)]
	public class AchievementConditionEditor : UnityEditor.Editor
	{
		private static readonly string customDescriptiontemplate = "customDescriptionTemplate";
		private static readonly string overideDescription = "overrideDescription";

		private readonly List<Achievement> achievementsUsingThisCondition = new();
		private bool showAchievementsList = true;

		public override void OnInspectorGUI()
		{
			var condition = (AchievementCondition)target;

			EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Key:", EditorStyles.boldLabel, GUILayout.Width(80));
			EditorGUILayout.LabelField(condition.Key);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel, GUILayout.Width(80));
			EditorGUILayout.LabelField(condition.Description);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(20);
			EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

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

			EditorGUILayout.Space(20);

			FindAchievementsUsingCondition(condition);
			DrawAchievementsList();

			serializedObject.ApplyModifiedProperties();
		}

		private void FindAchievementsUsingCondition(AchievementCondition condition)
		{
			achievementsUsingThisCondition.Clear();

			var achievementGuids = AssetDatabase.FindAssets("t:Achievement");

			foreach (var guid in achievementGuids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				Achievement achievement = AssetDatabase.LoadAssetAtPath<Achievement>(assetPath);

				if (achievement != null && achievement.Conditions != null)
				{
					if (achievement.Conditions.Contains(condition))
					{
						achievementsUsingThisCondition.Add(achievement);
					}
				}
			}
		}

		private void DrawAchievementsList()
		{
			if (achievementsUsingThisCondition.Count == 0)
			{
				EditorGUILayout.HelpBox("This condition is not used by any achievements.", MessageType.Info);
				return;
			}

			EditorGUILayout.LabelField($"Used by {achievementsUsingThisCondition.Count} Achievement(s)", EditorStyles.boldLabel);

			EditorGUI.indentLevel++;

			EditorGUILayout.BeginVertical(GUI.skin.box);

			foreach (var achievement in achievementsUsingThisCondition)
			{
				if (achievement == null) continue;

				EditorGUILayout.BeginHorizontal();

				if (achievement.Icon != null)
				{
					var iconTexture = AssetPreview.GetAssetPreview(achievement.Icon);
					if (iconTexture != null)
					{
						GUILayout.Label(iconTexture, GUILayout.Width(20), GUILayout.Height(20));
					}
				}


				GUILayout.Label(achievement.Title, EditorStyles.boldLabel);

				if (GUILayout.Button("Ping", GUILayout.Width(50)))
				{
					EditorGUIUtility.PingObject(achievement);
				}

				if (GUILayout.Button("Edit", GUILayout.Width(50)))
				{
					Selection.activeObject = achievement;
					EditorGUIUtility.PingObject(achievement);
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space(2);
			}

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}
	}
}