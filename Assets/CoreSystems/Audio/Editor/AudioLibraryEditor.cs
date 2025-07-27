using UnityEditor;
using UnityEngine;
using System.Linq;
using Utils;

namespace CoreSystems.Audio.Editor
{
	[CustomEditor(typeof(AudioLibrary))]
	public class AudioLibraryEditor : UnityEditor.Editor
	{
		private SerializedProperty clipsProp;
		private SerializedProperty profilesProp;

		private void OnEnable()
		{
			clipsProp = serializedObject.FindProperty("clips");
			profilesProp = serializedObject.FindProperty("clipEntries");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultInspector();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Auto-Fill Tools", EditorStyles.boldLabel);

			if (GUILayout.Button("Auto-Fill AudioClips"))
			{
				var allClips = AssetDatabaseUtils.FindAssetsOfType<AudioClip>();

				clipsProp.ClearArray();
				for (var i = 0; i < allClips.Length; i++)
				{
					clipsProp.InsertArrayElementAtIndex(i);
					clipsProp.GetArrayElementAtIndex(i).objectReferenceValue = allClips[i];
				}

				Debug.Log($"Auto-filled {allClips.Length} AudioClip(s).");
			}

			if (GUILayout.Button("Auto-Fill AudioClipProfiles"))
			{
				var allProfiles = AssetDatabaseUtils.FindScriptableObjectsOfType<AudioClipProfile>()
					.Where(p => p != null && p.Clip != null)
					.ToArray();

				profilesProp.ClearArray();
				for (var i = 0; i < allProfiles.Length; i++)
				{
					profilesProp.InsertArrayElementAtIndex(i);
					profilesProp.GetArrayElementAtIndex(i).objectReferenceValue = allProfiles[i];
				}

				Debug.Log($"Auto-filled {allProfiles.Length} AudioClipProfile(s).");
			}

			if (!GUI.changed) return;

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}