using UnityEditor;
using UnityEngine;

namespace EggSummoner.Editor.AssetUtils
{
	public class AssetUtils
	{
		[MenuItem("Assets/Copy Git Grep Guid", false, 100000)]
		public static void CopyGitGrepGUID()
		{
			string command = "";
			int i = 0;

			foreach (var item in Selection.assetGUIDs)
			{
				command += "git grep " + item + (i < Selection.assetGUIDs.Length ? "\r" : "");
				i++;
			}

			Debug.Log(command);
			EditorGUIUtility.systemCopyBuffer = command;
		}

		[MenuItem("Tools/Assets/Reserialize Selected Assets", priority = 100)]
		private static void ReserializeSelectedAssets()
		{
			var selectedObjects = Selection.objects;
			foreach (var obj in selectedObjects)
			{
				var path = AssetDatabase.GetAssetPath(obj);

				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
				EditorUtility.SetDirty(obj);
				Debug.Log("Reserialized asset: " + path);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		[MenuItem("Tools/Assets/Force Reserialize All Assets", priority = 100)]
		private static void ForceReserializeAllAssets()
		{
			var assetPaths = AssetDatabase.GetAllAssetPaths();
			AssetDatabase.ForceReserializeAssets(assetPaths);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Debug.Log("All assets have been reserialized.");
		}
	}
}