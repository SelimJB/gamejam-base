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
			var buttonWidth = 50f;
			Rect fieldRect = new(position.x, position.y, position.width - buttonWidth - 4f, position.height);
			Rect buttonRect = new(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height);

			EditorGUI.PropertyField(fieldRect, property, label);


			if (GUI.Button(buttonRect, "▶"))
			{
				EnsureAudioSource();

				AudioClip clip = null;

				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					clip = property.objectReferenceValue as AudioClip;
				}
				else if (property.propertyType == SerializedPropertyType.String)
				{
					var clipName = property.stringValue;
					var manager = Object.FindObjectOfType<AudioManager>();

					if (manager == null)
					{
						Debug.LogWarning("No AudioManager found in the scene.");
						return;
					}

					clip = manager.AudioLibrary.GetClip(clipName);

					if (clip == null)
					{
						Debug.LogWarning($"Audio clip '{clipName}' not found in AudioLibrary.");
						return;
					}
				}

				if (clip == null) return;

				if (previewSource.isPlaying)
					previewSource.Stop();

				previewSource.clip = clip;
				previewSource.Play();
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