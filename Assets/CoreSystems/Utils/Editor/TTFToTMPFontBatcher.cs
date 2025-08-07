using TMPro;
using UnityEditor;
using UnityEngine;

namespace Utils.EditorTools
{
	public class TTFToTMPFontBatcher
	{
		[MenuItem("Tools/TMP/Generate Font Assets From TTF")]
		public static void GenerateFontAssets()
		{
			var guids = AssetDatabase.FindAssets("t:Font", new[] { "Assets" });
			var i = 0;

			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (!path.EndsWith(".ttf")) continue;

				var font = AssetDatabase.LoadAssetAtPath<Font>(path);
				if (font == null)
				{
					Debug.LogWarning($"Font at {path} could not be loaded.");
					continue;
				}

				var assetPath = path.Replace(".ttf", "_TMPFont.asset");

				if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
				{
					// AssetDatabase.DeleteAsset(assetPath);
					continue;
				}

				var fontAsset = TMP_FontAsset.CreateFontAsset(font);

				AssetDatabase.CreateAsset(fontAsset, assetPath);

				if (fontAsset.material != null)
					AssetDatabase.AddObjectToAsset(fontAsset.material, assetPath);
				if (fontAsset.atlasTextures != null)
				{
					foreach (var tex in fontAsset.atlasTextures)
					{
						if (tex != null)
							AssetDatabase.AddObjectToAsset(tex, assetPath);
					}
				}

				EditorUtility.SetDirty(fontAsset);
				AssetDatabase.SaveAssets();
				AssetDatabase.ImportAsset(assetPath);

				Debug.Log($"Created TMP Font Asset at {assetPath}");
				i++;
			}

			Debug.Log($"Generated {i} TMP Font Assets from TTF files.");
			AssetDatabase.Refresh();
		}
	}
}