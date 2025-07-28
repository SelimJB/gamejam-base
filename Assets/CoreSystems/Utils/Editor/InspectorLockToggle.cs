using UnityEditor;
using UnityEngine;

public static class InspectorLockToggle
{
	[MenuItem("Tools/Commands/Toggle Inspector Lock %i")] // Ctrl/Cmd + I
	private static void ToggleInspectorLock()
	{
		var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		var window = EditorWindow.GetWindow(inspectorType);
		var isLockedProperty = inspectorType.GetProperty("isLocked");

		bool currentLock = (bool)isLockedProperty.GetValue(window);
		isLockedProperty.SetValue(window, !currentLock);
		window.Repaint();
	}
}