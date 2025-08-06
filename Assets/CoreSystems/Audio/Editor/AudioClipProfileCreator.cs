using UnityEditor;
using UnityEngine;
using System.IO;

namespace CoreSystems.Audio.Editor
{
	public static class AudioClipProfileCreator
	{
		[MenuItem("Assets/Create/ScriptableObjects/Audio/Create AudioClipProfiles from Clips", priority = 1)]
		public static void CreateAudioClipProfilesFromClips()
		{
			var selectedClips = Selection.objects;
			var createdCount = 0;

			foreach (var obj in selectedClips)
			{
				if (obj is not AudioClip clip)
					continue;

				var audioItem = ScriptableObject.CreateInstance<AudioClipProfile>();
				audioItem.name = clip.name;
				audioItem.SetClip(clip);

				var clipPath = AssetDatabase.GetAssetPath(clip);
				var clipDirectory = Path.GetDirectoryName(clipPath);
				var profilesDirectory = Path.Combine(clipDirectory, "AudioClipProfiles");

				if (!Directory.Exists(profilesDirectory))
				{
					Directory.CreateDirectory(profilesDirectory);
					AssetDatabase.Refresh();
				}

				var clipName = clip.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
				var assetPath = Path.Combine(profilesDirectory, $"{clipName}_AudioClipProfile.asset");
				assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

				AssetDatabase.CreateAsset(audioItem, assetPath);
				createdCount++;
			}

			if (createdCount > 0)
			{
				AssetDatabase.SaveAssets();
				EditorUtility.FocusProjectWindow();
				Debug.Log($"Created {createdCount} AudioClipProfile(s) from selection.");
			}
			else
			{
				Debug.LogWarning("No AudioClip selected to create AudioClipProfiles.");
			}
		}

		[MenuItem("Assets/Create/ScriptableObjects/Audio/Create AudioClipProfiles from Clips", true)]
		public static bool ValidateCreateAudioClipProfilesFromClips()
		{
			foreach (var obj in Selection.objects)
				if (obj is AudioClip)
					return true;

			return false;
		}
	}
}