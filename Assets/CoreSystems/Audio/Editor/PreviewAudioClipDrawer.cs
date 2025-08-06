using UnityEditor;
using UnityEngine;

namespace CoreSystems.Audio.Editor
{
	[CustomPropertyDrawer(typeof(PreviewAudioClipAttribute))]
	public class PreviewAudioClipDrawer : PropertyDrawer
	{
		private static AudioSource previewSource;
		private static bool showProfileDropdown = false;
		private static SerializedProperty currentProperty;

		private void EnsureAudioSource()
		{
			if (previewSource != null) return;

			var go = EditorUtility.CreateGameObjectWithHideFlags("Audio Preview",
				HideFlags.HideAndDontSave,
				typeof(AudioSource));
			previewSource = go.GetComponent<AudioSource>();
			previewSource.spatialBlend = 0f;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var baseHeight = EditorGUIUtility.singleLineHeight;

			if (property == null || !IsPropertyValid(property))
			{
				return baseHeight;
			}

			if (showProfileDropdown && currentProperty != null && IsPropertyValid(currentProperty) &&
			    currentProperty.propertyPath == property.propertyPath)
			{
				var dropdownHeight = CalculateDropdownHeight(property);
				return baseHeight + dropdownHeight;
			}

			return baseHeight;
		}

		private bool IsPropertyValid(SerializedProperty property)
		{
			if (property == null) return false;

			try
			{
				var _ = property.propertyPath;
				return property.serializedObject != null && property.serializedObject.targetObject != null;
			}
			catch
			{
				return false;
			}
		}

		private float CalculateDropdownHeight(SerializedProperty property)
		{
			if (property == null || !IsPropertyValid(property)) return 100f; // Fallback height

			var lineHeight = EditorGUIUtility.singleLineHeight;
			var spacing = 2f;
			var margin = 10f; // Top and bottom margins
			var height = margin;

			// Base elements: Volume, Loop, DelayBetweenPlays, Randomize Pitch
			height += (lineHeight + spacing) * 4;

			// Check if we need to show pitch min/max
			bool showPitchRange = false;
			
			try
			{
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					var clip = property.objectReferenceValue as AudioClip;
					if (clip != null)
					{
						var manager = Object.FindObjectOfType<AudioManager>();
						if (manager != null && manager.AudioLibrary != null)
						{
							var clipProfile = manager.AudioLibrary.GetClipProfile(clip.name);
							if (clipProfile != null)
							{
								showPitchRange = clipProfile.RandomizePitch;
							}
						}
					}
				}
				else if (property.propertyType == SerializedPropertyType.String)
				{
					var clipName = property.stringValue;
					var manager = Object.FindObjectOfType<AudioManager>();
					if (manager != null && manager.AudioLibrary != null && !string.IsNullOrEmpty(clipName))
					{
						var clipProfile = manager.AudioLibrary.GetClipProfile(clipName);
						if (clipProfile != null)
						{
							showPitchRange = clipProfile.RandomizePitch;
						}
					}
				}
			}
			catch
			{
				// If any error occurs, default to not showing pitch range
				showPitchRange = false;
			}

			// Add height for pitch min/max if needed
			if (showPitchRange)
			{
				height += (lineHeight + spacing) * 2; // Min and Max fields
			}

			// Add height for Ping button
			height += lineHeight + spacing;

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property == null || !IsPropertyValid(property))
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			var playButtonWidth = 40f;
			var profileButtonWidth = 90f;
			var totalButtonsWidth = playButtonWidth + profileButtonWidth;

			var mainLineHeight = EditorGUIUtility.singleLineHeight;
			Rect mainRect = new(position.x, position.y, position.width, mainLineHeight);

			Rect fieldRect = new(mainRect.x, mainRect.y, mainRect.width - totalButtonsWidth, mainLineHeight);
			Rect playButtonRect = new(mainRect.x + mainRect.width - totalButtonsWidth, mainRect.y, playButtonWidth, mainLineHeight);
			Rect profileButtonRect = new(mainRect.x + mainRect.width - profileButtonWidth, mainRect.y, profileButtonWidth, mainLineHeight);

			EditorGUI.PropertyField(fieldRect, property, label);

			AudioClip clip = null;
			AudioClipProfile clipProfile = null;
			var hasProfile = false;
			var hasAudioManager = false;

			try
			{
				var manager = Object.FindObjectOfType<AudioManager>();
				hasAudioManager = manager != null && manager.AudioLibrary != null;

				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					clip = property.objectReferenceValue as AudioClip;
					if (clip != null && hasAudioManager)
					{
						clipProfile = manager.AudioLibrary.GetClipProfile(clip.name);
						hasProfile = clipProfile != null;
					}
				}
				else if (property.propertyType == SerializedPropertyType.String)
				{
					var clipName = property.stringValue;
					if (hasAudioManager && !string.IsNullOrEmpty(clipName))
					{
						clip = manager.AudioLibrary.GetClip(clipName);
						if (clip != null)
						{
							clipProfile = manager.AudioLibrary.GetClipProfile(clipName);
							hasProfile = clipProfile != null;
						}
					}
				}
			}
			catch
			{
				clip = null;
				clipProfile = null;
				hasProfile = false;
				hasAudioManager = false;
			}

			var originalColor = GUI.color;

			GUI.enabled = clip != null;
			if (GUI.Button(playButtonRect, "▶"))
			{
				PlayClipWithProfile(clip, clipProfile);
			}

			GUI.enabled = true;

			var isCurrentPropertyDropdown = showProfileDropdown && currentProperty != null &&
			                                IsPropertyValid(currentProperty) &&
			                                currentProperty.propertyPath == property.propertyPath;

			if (hasProfile)
			{
				GUI.color = new Color(0.7f, 1f, 0.7f);
				GUI.enabled = true;

				if (GUI.Button(profileButtonRect, isCurrentPropertyDropdown ? "Profile ▼" : "Profile ▶"))
				{
					if (isCurrentPropertyDropdown)
					{
						showProfileDropdown = false;
						currentProperty = null;
					}
					else
					{
						showProfileDropdown = true;
						currentProperty = property;
					}
				}
			}
			else if (clip != null && hasAudioManager)
			{
				GUI.color = new Color(0.7f, 0.7f, 1f);
				GUI.enabled = true;

				if (GUI.Button(profileButtonRect, "Create Profile"))
				{
					CreateClipProfile(clip);
				}
			}
			else
			{
				GUI.color = new Color(0.6f, 0.6f, 0.6f);
				GUI.enabled = false;

				var buttonText = !hasAudioManager ? "No Manager" : "No Clip";
				GUI.Button(profileButtonRect, buttonText);
			}

			GUI.enabled = true;
			GUI.color = originalColor;

			if (isCurrentPropertyDropdown && hasProfile && clipProfile != null)
			{
				DrawProfileDropdown(position, clipProfile, mainLineHeight, property);
			}
		}

		private void DrawProfileDropdown(Rect position, AudioClipProfile profile, float mainLineHeight, SerializedProperty property)
		{
			var dropdownHeight = CalculateDropdownHeight(property);
			var dropdownRect = new Rect(position.x, position.y + mainLineHeight, position.width, dropdownHeight);

			EditorGUI.DrawRect(dropdownRect, new Color(0.2f, 0.2f, 0.2f, 0.9f));

			var margin = 5f;
			var contentRect = new Rect(dropdownRect.x + margin,
				dropdownRect.y + margin,
				dropdownRect.width - margin * 2,
				dropdownRect.height - margin * 2);

			var lineHeight = EditorGUIUtility.singleLineHeight;
			var currentY = contentRect.y;

			if (profile == null) return;

			var serializedProfile = new SerializedObject(profile);

			EditorGUI.BeginChangeCheck();

			var volumeRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var volumeProp = serializedProfile.FindProperty("volume");
			if (volumeProp != null)
			{
				EditorGUI.PropertyField(volumeRect, volumeProp, new GUIContent("Volume"));
			}

			currentY += lineHeight + 2f;

			var loopRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var loopProp = serializedProfile.FindProperty("loop");
			if (loopProp != null)
			{
				EditorGUI.PropertyField(loopRect, loopProp, new GUIContent("Loop"));
			}

			currentY += lineHeight + 2f;

			var delayBetweenPlaysRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var delayBetweenPlaysProp = serializedProfile.FindProperty("delayBetweenPlays");
			if (delayBetweenPlaysProp != null)
			{
				EditorGUI.PropertyField(delayBetweenPlaysRect, delayBetweenPlaysProp, new GUIContent("Delay Between Plays"));
				currentY += lineHeight + 2f;
			}

			var randomizePitchRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var randomizePitchProp = serializedProfile.FindProperty("randomizePitch");
			if (randomizePitchProp != null)
			{
				EditorGUI.PropertyField(randomizePitchRect, randomizePitchProp, new GUIContent("Randomize Pitch"));
				currentY += lineHeight + 2f;

				if (randomizePitchProp.boolValue)
				{
					var pitchMinRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
					var pitchMinProp = serializedProfile.FindProperty("pitchMin");
					if (pitchMinProp != null)
					{
						EditorGUI.PropertyField(pitchMinRect, pitchMinProp, new GUIContent("Pitch Min"));
					}

					currentY += lineHeight + 2f;

					var pitchMaxRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
					var pitchMaxProp = serializedProfile.FindProperty("pitchMax");
					if (pitchMaxProp != null)
					{
						EditorGUI.PropertyField(pitchMaxRect, pitchMaxProp, new GUIContent("Pitch Max"));
					}

					currentY += lineHeight + 2f;
				}
			}

			var pingButtonRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			if (GUI.Button(pingButtonRect, "Ping Profile in Project"))
			{
				EditorGUIUtility.PingObject(profile);
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedProfile.ApplyModifiedProperties();
				EditorUtility.SetDirty(profile);
			}
		}

		private void PlayClipWithProfile(AudioClip clip, AudioClipProfile profile)
		{
			if (clip == null) return;

			EnsureAudioSource();

			if (previewSource.isPlaying)
				previewSource.Stop();

			var volume = 1f;
			var pitch = 1f;
			var loop = false;

			if (profile != null)
			{
				volume = profile.Volume;
				pitch = profile.RandomizePitch ? Random.Range(profile.PitchMin, profile.PitchMax) : 1f;
				loop = profile.Loop;
			}

			previewSource.clip = clip;
			previewSource.volume = volume;
			previewSource.pitch = pitch;
			previewSource.loop = loop;
			previewSource.Play();

			if (profile != null)
			{
				Debug.Log($"Playing '{clip.name}' with ClipProfile: Volume={volume:F2}, Pitch={pitch:F2}, Loop={loop}");
			}
			else
			{
				Debug.Log($"Playing '{clip.name}' without ClipProfile (default settings)");
			}
		}

		private void CreateClipProfile(AudioClip clip)
		{
			if (clip == null) return;

			var profile = ScriptableObject.CreateInstance<AudioClipProfile>();
			profile.SetClip(clip);

			var serializedProfile = new SerializedObject(profile);
			serializedProfile.FindProperty("volume").floatValue = 1f;
			serializedProfile.FindProperty("loop").boolValue = false;
			serializedProfile.FindProperty("randomizePitch").boolValue = false;
			serializedProfile.FindProperty("pitchMin").floatValue = 0.95f;
			serializedProfile.FindProperty("pitchMax").floatValue = 1.05f;
			serializedProfile.ApplyModifiedProperties();

			var clipPath = AssetDatabase.GetAssetPath(clip);
			var clipDirectory = System.IO.Path.GetDirectoryName(clipPath);
			var profilesDirectory = System.IO.Path.Combine(clipDirectory, "AudioClipProfiles");

			if (!System.IO.Directory.Exists(profilesDirectory))
			{
				System.IO.Directory.CreateDirectory(profilesDirectory);
				AssetDatabase.Refresh();
			}

			var clipName = clip.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
			var assetPath = System.IO.Path.Combine(profilesDirectory, $"{clipName}_Profile.asset");

			assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

			AssetDatabase.CreateAsset(profile, assetPath);
			AssetDatabase.SaveAssets();

			var manager = Object.FindObjectOfType<AudioManager>();
			if (manager != null && manager.AudioLibrary != null)
			{
				var audioLibrary = manager.AudioLibrary;
				var serializedLibrary = new SerializedObject(audioLibrary);
				var clipEntriesProperty = serializedLibrary.FindProperty("clipEntries");

				clipEntriesProperty.arraySize++;
				var newEntryProperty = clipEntriesProperty.GetArrayElementAtIndex(clipEntriesProperty.arraySize - 1);
				newEntryProperty.objectReferenceValue = profile;

				serializedLibrary.ApplyModifiedProperties();
				EditorUtility.SetDirty(audioLibrary);
			}

			EditorGUIUtility.PingObject(profile);

			Debug.Log($"Created AudioClipProfile for '{clip.name}' at {assetPath}");
		}

		[InitializeOnLoadMethod]
		private static void RegisterOnDisable()
		{
			EditorApplication.quitting += Cleanup;
			AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
			Selection.selectionChanged += CleanupDropdownState;
		}

		private static void Cleanup()
		{
			if (previewSource == null) return;

			Object.DestroyImmediate(previewSource.gameObject);
			previewSource = null;
			CleanupDropdownState();
		}

		private static void CleanupDropdownState()
		{
			showProfileDropdown = false;
			currentProperty = null;
		}
	}
}