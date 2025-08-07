using System;
using CoreSystems;
using CoreSystems.Audio;
using UnityEngine;
using Utils;

namespace Staging
{
	public class DevPlayground : MonoBehaviour
	{
		[SerializeField, PreviewAudioClip] private AudioClip testClip;
		[SerializeField, PreviewAudioClip] private string stringClip;
		[SerializeField] SerializableNullable<float> testNullableFloat;

		private AudioManager audioManager;

		private void Start()
		{
			audioManager = AudioManager.Instance;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.I))
			{
				audioManager.Play(testClip);
			}

			if (Input.GetKeyDown(KeyCode.O))
			{
				audioManager.Play(stringClip);
			}

			if (Input.GetKey(KeyCode.P))
			{
				audioManager.Play(testClip);
			}
		}

		private void OnGUI()
		{
			// vertical layout
			GUILayout.BeginVertical("box");
			// increase font size for buttons
			GUI.skin.button.fontSize = 30;
			GUI.skin.label.fontSize = 30;


			GUILayout.Label("Dev Playground", GUILayout.Height(200));
			if (GUILayout.Button("Test Achievement"))
			{
				GameEvents.TriggerItemCollected("coin", 1);
			}

			if (GUILayout.Button("Test Achievement 2"))
			{
				GameEvents.TriggerSignal("TEST");
			}

			if (GUILayout.Button("Test Achievement 3"))
			{
				GameEvents.TriggerSignal("SPECIAL");
			}

			GUILayout.EndVertical();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			CustomGizmos.DrawCross(new Vector2(0, 0), 2f);
		}

		[EditorButton]
		private void TestEditorButton()
		{
			Debug.Log("Button clicked!");
		}
	}
}