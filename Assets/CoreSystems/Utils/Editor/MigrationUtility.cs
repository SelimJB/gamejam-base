using System;
using UnityEditor;
using UnityEngine;

namespace Utils.EditorTools
{
	public static class MigrationUtils
	{
		public static void MigrateScriptableObject<T>(Action<T> migrateScriptableObject, string searchPath = null) where T : ScriptableObject
		{
			var scriptableObjects = AssetDatabaseUtils.FindScriptableObjectsOfType<T>(searchPath);

			Debug.Log($"Re-serializing {scriptableObjects.Length} {typeof(T).Name} objects...");

			foreach (var so in scriptableObjects)
				migrateScriptableObject(so);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public static void MigratePrefabsWithComponent<T>(Action<T> migrateComponent, string searchPath = null) where T : Component
		{
			var components = AssetDatabaseUtils.FindPrefabsWithComponent<T>(searchPath);

			Debug.Log($"Migrating {components.Length} prefab(s) with component {typeof(T).Name}...");

			foreach (var comp in components)
			{
				migrateComponent(comp);
				EditorUtility.SetDirty(comp.gameObject);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}