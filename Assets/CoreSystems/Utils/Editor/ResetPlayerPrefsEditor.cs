using UnityEditor;
using UnityEngine;

namespace Utils.EditorTools
{
	public static class ResetPlayerPrefsEditor
	{
		[MenuItem("Tools/PlayerPrefs/Reset All", priority = 100)]
		public static void ResetAllPlayerPrefs()
		{
			if (!EditorUtility.DisplayDialog("Reset PlayerPrefs", "Are you sure you want to delete all PlayerPrefs?", "Yes", "Cancel")) return;

			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			Debug.Log("✅ PlayerPrefs have been reset.");
		}
	}
}