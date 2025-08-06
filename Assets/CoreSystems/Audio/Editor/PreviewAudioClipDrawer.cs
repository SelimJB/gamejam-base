using UnityEditor;
using UnityEngine;

namespace CoreSystems.Audio.Editor
{
	[CustomPropertyDrawer(typeof(PreviewAudioClipAttribute))]
	public class PreviewAudioClipDrawer : PropertyDrawer
	{
		private static AudioSource previewSource;

		private void EnsureAudioSource()
		{
			if (previewSource != null) return;

			var go = EditorUtility.CreateGameObjectWithHideFlags("Audio Preview",
				HideFlags.HideAndDontSave,
				typeof(AudioSource));
			previewSource = go.GetComponent<AudioSource>();
			previewSource.spatialBlend = 0f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var playButtonWidth = 40f;
			var profileButtonWidth = 50f;
			var totalButtonsWidth = playButtonWidth + profileButtonWidth;

			Rect fieldRect = new(position.x, position.y, position.width - totalButtonsWidth, position.height);
			Rect profileButtonRect = new(position.x + position.width - totalButtonsWidth, position.y, profileButtonWidth, position.height);
			Rect playButtonRect = new(position.x + position.width - playButtonWidth, position.y, playButtonWidth, position.height);

			EditorGUI.PropertyField(fieldRect, property, label);

			AudioClip clip = null;
			AudioClipProfile clipProfile = null;
			var hasProfile = false;

			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				clip = property.objectReferenceValue as AudioClip;
				if (clip != null)
				{
					var manager = Object.FindObjectOfType<AudioManager>();
					if (manager != null)
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

				if (manager != null && !string.IsNullOrEmpty(clipName))
				{
					clip = manager.AudioLibrary.GetClip(clipName);
					if (clip != null)
					{
						clipProfile = manager.AudioLibrary.GetClipProfile(clipName);
						hasProfile = clipProfile != null;
					}
				}
			}

			var originalColor = GUI.color;
			GUI.color = hasProfile ? new Color(0.7f, 1f, 0.7f) : new Color(0.6f, 0.6f, 0.6f);
			GUI.enabled = hasProfile;

			if (GUI.Button(profileButtonRect, "Profile"))
			{
				if (hasProfile && clipProfile != null)
				{
					EditorGUIUtility.PingObject(clipProfile);
				}
			}

			GUI.enabled = true;
			GUI.color = originalColor;

			GUI.enabled = clip != null;
			if (GUI.Button(playButtonRect, "▶"))
			{
				PlayClipWithProfile(clip, clipProfile);
			}

			GUI.enabled = true;
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
		}

		private static void Cleanup()
		{
			if (previewSource == null) return;

			Object.DestroyImmediate(previewSource.gameObject);
			previewSource = null;
		}
	}
}