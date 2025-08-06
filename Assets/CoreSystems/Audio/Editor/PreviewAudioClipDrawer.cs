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

			// Check if property is valid before accessing it
			if (property == null || !IsPropertyValid(property))
			{
				return baseHeight;
			}

			if (showProfileDropdown && currentProperty != null && IsPropertyValid(currentProperty) &&
			    currentProperty.propertyPath == property.propertyPath)
			{
				// Calculate dynamic height based on content
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
				// Try to access a basic property to check if it's disposed
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

			// Base elements: Volume, Loop, Randomize Pitch
			height += (lineHeight + spacing) * 3;

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
			// Early exit if property is invalid
			if (property == null || !IsPropertyValid(property))
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			var playButtonWidth = 40f;
			var profileButtonWidth = 60f;
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

			try
			{
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					clip = property.objectReferenceValue as AudioClip;
					if (clip != null)
					{
						var manager = Object.FindObjectOfType<AudioManager>();
						if (manager != null && manager.AudioLibrary != null)
						{
							clipProfile = manager.AudioLibrary.GetClipProfile(clip.name);
							hasProfile = clipProfile != null;
						}
					}
				}
				else if (property.propertyType == SerializedPropertyType.String)
				{
					var clipName = property.stringValue;
					var manager = Object.FindObjectOfType<AudioManager>();

					if (manager != null && manager.AudioLibrary != null && !string.IsNullOrEmpty(clipName))
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
				// If any error occurs accessing property, just show basic buttons
				clip = null;
				clipProfile = null;
				hasProfile = false;
			}

			var originalColor = GUI.color;

			GUI.enabled = clip != null;
			if (GUI.Button(playButtonRect, "▶"))
			{
				PlayClipWithProfile(clip, clipProfile);
			}
			GUI.enabled = true;

			GUI.color = hasProfile ? new Color(0.7f, 1f, 0.7f) : new Color(0.6f, 0.6f, 0.6f);
			GUI.enabled = hasProfile;

			var isCurrentPropertyDropdown = showProfileDropdown && currentProperty != null && 
			                               IsPropertyValid(currentProperty) &&
			                               currentProperty.propertyPath == property.propertyPath;

			if (GUI.Button(profileButtonRect, isCurrentPropertyDropdown ? "Profile ▼" : "Profile ▶"))
			{
				if (hasProfile && clipProfile != null)
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

			// Volume slider
			var volumeRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var volumeProp = serializedProfile.FindProperty("volume");
			if (volumeProp != null)
			{
				EditorGUI.PropertyField(volumeRect, volumeProp, new GUIContent("Volume"));
			}
			currentY += lineHeight + 2f;

			// Loop toggle
			var loopRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var loopProp = serializedProfile.FindProperty("loop");
			if (loopProp != null)
			{
				EditorGUI.PropertyField(loopRect, loopProp, new GUIContent("Loop"));
			}
			currentY += lineHeight + 2f;

			// Randomize pitch toggle
			var randomizePitchRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
			var randomizePitchProp = serializedProfile.FindProperty("randomizePitch");
			if (randomizePitchProp != null)
			{
				EditorGUI.PropertyField(randomizePitchRect, randomizePitchProp, new GUIContent("Randomize Pitch"));
				currentY += lineHeight + 2f;

				// Pitch range (one below the other if randomize is enabled)
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

			// Ping Profile button
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

		[InitializeOnLoadMethod]
		private static void RegisterOnDisable()
		{
			EditorApplication.quitting += Cleanup;
			AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
			// Reset dropdown state when selection changes or inspector updates
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
			// Reset dropdown state
			showProfileDropdown = false;
			currentProperty = null;
		}
	}
}