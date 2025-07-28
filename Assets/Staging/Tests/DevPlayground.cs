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