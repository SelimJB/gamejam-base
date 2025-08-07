using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utils.EditorTools
{
	public static class AssetDatabaseUtils
	{
		public static T[] FindScriptableObjectsOfType<T>(string searchPath = null) where T : ScriptableObject
		{
			var guids = searchPath == null
				? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
				: AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchPath });

			var assets = new T[guids.Length];

			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
			}

			return assets;
		}

		public static T[] FindAssetsOfType<T>(string searchPath = null) where T : Object
		{
			var guids = searchPath == null
				? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
				: AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchPath });

			var assets = new T[guids.Length];

			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
			}

			return assets;
		}

		public static T[] FindPrefabsWithComponent<T>(string searchPath = null) where T : Component
		{
			var guids = searchPath == null
				? AssetDatabase.FindAssets($"t:GameObject")
				: AssetDatabase.FindAssets($"t:GameObject", new[] { searchPath });

			var prefabs = new List<T>();

			foreach (var t in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(t);
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

				if (prefab.TryGetComponent(out T component))
					prefabs.Add(component);
			}

			return prefabs.ToArray();
		}

		public static string GetRelativePathFromAbsolutePath(string absolutePath)
		{
			return "Assets" + absolutePath.Substring(Application.dataPath.Length);
		}

		public static T[] FindScriptableObjectsOfTypeInDirectory<T>(string relativePath) where T : ScriptableObject
		{
			if (string.IsNullOrEmpty(relativePath))
			{
				Debug.LogError("Directory cannot be null or empty.");
				return new T[0];
			}

			if (!relativePath.EndsWith("/"))
				relativePath += "/";

			var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { relativePath });
			var assets = new T[guids.Length];

			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
			}

			return assets;
		}
	}
}