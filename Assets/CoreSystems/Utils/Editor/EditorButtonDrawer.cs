using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class EditorButtonDrawer : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var methods = target.GetType()
			.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		foreach (var method in methods)
		{
			if (method.GetCustomAttribute<EditorButton>() != null)
			{
				if (GUILayout.Button(method.Name))
				{
					method.Invoke(target, null);
				}
			}
		}
	}
}