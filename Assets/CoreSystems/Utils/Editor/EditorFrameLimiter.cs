#if UNITY_EDITOR 

using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorFrameRateLimiter
{
	private const int EDITOR_TARGET_FPS = 60;

	static EditorFrameRateLimiter()
	{
		QualitySettings.vSyncCount = 1;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredPlayMode)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = EDITOR_TARGET_FPS;
		}
		else if (state == PlayModeStateChange.ExitingPlayMode)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = -1;
		}
	}
}
#endif