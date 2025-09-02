using CoreSystems;
using CoreSystems.Achievements.UI;
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
		[SerializeField] private AchievementMenu achievementMenu;

		private AudioManager audioManager;

		private void Start()
		{
			audioManager = AudioManager.Instance;
		}

		private void Update()
		{
			GameEvents.TriggerTimeElapsed(Time.deltaTime);

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
			GUILayout.BeginVertical("box");
			GUI.skin.button.fontSize = 40;
			GUI.skin.label.fontSize = 40;


			GUILayout.Label("Dev Playground");

			if (GUILayout.Button("Toggle Achievement Menu"))
			{
				achievementMenu.ToggleVisibility();
			}

			if (GUILayout.Button("Unlock Button Test Achievement"))
			{
				GameEvents.TriggerMilestone("TEST");
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