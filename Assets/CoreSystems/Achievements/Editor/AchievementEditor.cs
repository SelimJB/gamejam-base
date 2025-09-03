using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CoreSystems.Achievements.Editor
{
	[CustomEditor(typeof(Achievement))]
	public class AchievementEditor : UnityEditor.Editor
	{
		private bool showConditions = true;
		private bool useEnhancedView = false;
		private Dictionary<Type, string> conditionTypes;
		private int selectedConditionType = 0;

		private void OnEnable()
		{
			conditionTypes = new Dictionary<Type, string>();

			var baseType = typeof(AchievementCondition);
			var assembly = baseType.Assembly;
			var types = assembly.GetTypes()
				.Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract)
				.ToArray();

			foreach (var type in types)
			{
				var createAssetMenuAttr = type.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false)
					.FirstOrDefault() as CreateAssetMenuAttribute;

				var displayName = createAssetMenuAttr?.menuName ?? type.Name;
				displayName = displayName.Split('/').Last();

				conditionTypes[type] = displayName;
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var achievement = target as Achievement;

			EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Title:", EditorStyles.boldLabel, GUILayout.Width(80));
			EditorGUILayout.LabelField(achievement.name);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
			var descriptionStyle = new GUIStyle(EditorStyles.label);
			descriptionStyle.wordWrap = true;
			descriptionStyle.richText = true;
			var description = achievement.GetDescription();
			var content = new GUIContent(description);
			var height = descriptionStyle.CalcHeight(content, EditorGUIUtility.currentViewWidth - 40);
			EditorGUILayout.LabelField(description, descriptionStyle, GUILayout.Height(height));

			EditorGUILayout.Space(20);

			EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("flavorText"));

			var overrideDescriptionProp = serializedObject.FindProperty("overrideDescription");
			EditorGUILayout.PropertyField(overrideDescriptionProp);

			if (achievement != null && overrideDescriptionProp.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("customDescription"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isHidden"));

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("unlockSound"));

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("displaySoftMask"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("iconPadding"));

			EditorGUILayout.Space();

			DrawClassicConditionsSection();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawClassicConditionsSection()
		{
			var conditionsProperty = serializedObject.FindProperty("conditions");

			EditorGUILayout.PropertyField(conditionsProperty, true);

			EditorGUILayout.Space(20);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Create New Condition"))
			{
				ShowConditionCreationMenu(target as Achievement, conditionsProperty);
			}

			if (conditionsProperty.arraySize > 0)
			{
				if (conditionsProperty.arraySize > 1)
				{
					if (GUILayout.Button("Select to delete", GUILayout.Width(120)))
					{
						ChooseToDelete(conditionsProperty);
					}
				}

				if (GUILayout.Button("Delete All", GUILayout.Width(80)))
				{
					if (EditorUtility.DisplayDialog("Delete All Conditions",
						    "Are you sure you want to delete all conditions?\n\nThis will also delete the condition assets.",
						    "Remove All",
						    "Cancel"))
						DeleteAllConditions(conditionsProperty);
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		private void ChooseToDelete(SerializedProperty conditionsProperty)
		{
			if (conditionsProperty.arraySize == 0) return;

			if (conditionsProperty.arraySize == 1)
			{
				var condition = conditionsProperty.GetArrayElementAtIndex(0).objectReferenceValue as AchievementCondition;
				if (condition != null)
				{
					if (EditorUtility.DisplayDialog("Remove Condition",
						    $"Are you sure you want to remove the condition '{condition.name}'?\n\nThis will also delete the condition asset.",
						    "Remove",
						    "Cancel"))
					{
						RemoveCondition(conditionsProperty, 0, condition);
					}
				}

				return;
			}

			var menu = new GenericMenu();
			for (var i = 0; i < conditionsProperty.arraySize; i++)
			{
				var index = i;
				var conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
				var condition = conditionProperty.objectReferenceValue as AchievementCondition;

				if (condition == null) continue;

				var displayName = $"Delete {i}: {condition.name}";

				menu.AddItem(new GUIContent(displayName),
					false,
					() =>
					{
						if (EditorUtility.DisplayDialog("Remove Condition",
							    $"Are you sure you want to remove this {condition.GetType().Name}?",
							    "Remove",
							    "Cancel"))
						{
							RemoveCondition(conditionsProperty, index, condition);
						}
					});
			}

			menu.ShowAsContext();
		}

		private void DeleteAllConditions(SerializedProperty conditionsArray)
		{
			for (var i = 0; i < conditionsArray.arraySize; i++)
			{
				var conditionProperty = conditionsArray.GetArrayElementAtIndex(i);
				var condition = conditionProperty.objectReferenceValue as AchievementCondition;

				if (condition == null) continue;

				var conditionPath = AssetDatabase.GetAssetPath(condition);
				AssetDatabase.DeleteAsset(conditionPath);
			}

			conditionsArray.arraySize = 0;

			serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
		}

		private void ShowConditionCreationMenu(Achievement achievement, SerializedProperty conditionsProperty)
		{
			var menu = new GenericMenu();

			foreach (var kvp in conditionTypes)
			{
				var conditionType = kvp.Key;
				var displayName = kvp.Value;

				menu.AddItem(new GUIContent(displayName), false, () => { CreateAndAddCondition(achievement, conditionType, conditionsProperty); });
			}

			menu.ShowAsContext();
		}

		private void CreateAndAddCondition(Achievement achievement, Type conditionType, SerializedProperty conditionsProperty)
		{
			var condition = CreateInstance(conditionType) as AchievementCondition;

			if (condition == null)
			{
				Debug.LogError($"Failed to create condition of type {conditionType.Name}");
				return;
			}

			var achievementTarget = achievement ?? target as Achievement;
			if (achievementTarget == null)
			{
				Debug.LogError("No achievement target found");
				DestroyImmediate(condition);
				return;
			}

			var achievementName = achievementTarget.name;
			achievementName = achievementName.StartsWith("Achievement_") ? achievementName.Substring("Achievement_".Length) : achievementName;
			var conditionTypeName = conditionType.Name.Replace("Condition", "");
			var conditionName = $"Condition_{conditionTypeName}_{achievementName}_{conditionsProperty.arraySize + 1}";

			condition.name = conditionName;

			var achievementPath = AssetDatabase.GetAssetPath(achievementTarget);
			var directory = System.IO.Path.GetDirectoryName(achievementPath);
			var conditionPath = $"{directory}/{conditionName}.asset";

			conditionPath = AssetDatabase.GenerateUniqueAssetPath(conditionPath);

			AssetDatabase.CreateAsset(condition, conditionPath);
			AssetDatabase.SaveAssets();

			conditionsProperty.arraySize++;
			var newConditionProperty = conditionsProperty.GetArrayElementAtIndex(conditionsProperty.arraySize - 1);
			newConditionProperty.objectReferenceValue = condition;

			serializedObject.ApplyModifiedProperties();

			EditorGUIUtility.PingObject(condition);

			Debug.Log($"Created new condition: {conditionName}");
		}

		private void RemoveCondition(SerializedProperty conditionsArray, int index, AchievementCondition condition)
		{
			if (condition != null)
			{
				var conditionPath = AssetDatabase.GetAssetPath(condition);
				AssetDatabase.DeleteAsset(conditionPath);
			}

			var elementProperty = conditionsArray.GetArrayElementAtIndex(index);
			if (elementProperty.objectReferenceValue != null)
			{
				elementProperty.objectReferenceValue = null;
			}

			conditionsArray.DeleteArrayElementAtIndex(index);

			serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
		}
	}
}