using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Utils.Editor
{
	public static class GitGrepGuidFinder
	{
		/// <param name="workingDirectory">Default is Application.dataPath</param>
		/// <param name="filter">Optional filter for the git grep command. Example: "'*.cs' '*.meta'"</param>
		/// <returns>A list of file paths that reference the given GUID</returns>
		public static List<string> SearchGUIDReferences(string guid, string workingDirectory = null, string filter = null)
		{
			var results = new List<string>();

			var psi = new ProcessStartInfo
			{
				FileName = "git",
				Arguments = filter == null ? $"grep -l {guid}" : $"grep -l {guid} -- {filter}",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = workingDirectory ?? Application.dataPath
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.Start();

				while (!process.StandardOutput.EndOfStream)
				{
					var filePath = process.StandardOutput.ReadLine();
					if (!string.IsNullOrEmpty(filePath))
					{
						results.Add(filePath);
					}
				}
			}

			return results;
		}


		/// <param name="workingDirectory">Default is Application.dataPath</param>
		/// <param name="filter">Optional filter for the git grep command. Example: "'*.cs' '*.meta'"</param>
		/// <returns>A list of file paths that reference the given GUID</returns>
		public static List<string> SearchGUIDReferencesWithProgress(string guid, string workingDirectory = null, string filter = null,
			bool verbose = false)
		{
			var time = System.DateTime.Now;
			List<string> results;

			try
			{
				EditorUtility.DisplayCancelableProgressBar("Searching for GUID references", $"Searching for GUID {guid}...", 0f);

				results = SearchGUIDReferences(guid, workingDirectory, filter);

				EditorUtility.DisplayCancelableProgressBar("Searching for GUID references", $"Searching for GUID {guid}...", 1f);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			if (verbose) Debug.Log($"Found {results.Count} references to GUID {guid} in {System.DateTime.Now - time}");

			return results;
		}

		/// <param name="workingDirectory">Default is Application.dataPath</param>
		/// <param name="filter">Optional filter for the git grep command. Example: "'*.cs' '*.meta'"</param>
		/// <returns>A dictionary of GUIDs and their references</returns>
		public static Dictionary<string, List<string>> SearchMultipleGUIDReferencesWithProgress(IEnumerable<string> guids,
			string workingDirectory = null,
			string filter = null, bool verbose = false)
		{
			var time = System.DateTime.Now;
			var results = new Dictionary<string, List<string>>();
			var total = guids.Count();

			try
			{
				EditorUtility.DisplayCancelableProgressBar("Searching for GUID references", "", 0f);

				var count = 0;
				foreach (var guid in guids)
				{
					results[guid] = SearchGUIDReferences(guid, workingDirectory, filter);
					count++;
					if (EditorUtility.DisplayCancelableProgressBar($"Searching for GUID references : ({count}/{total})",
						    $"{results[guid].Count} references to GUID {guid} found. ({count}/{total})",
						    (float)count / total))
					{
						Debug.LogError("Search for GUID references cancelled");
						break;
					}
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			if (verbose) Debug.Log($"Search for {guids.Count()} GUIDs done in {System.DateTime.Now - time}");

			return results;
		}
	}
}