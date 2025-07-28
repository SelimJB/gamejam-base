using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SerializableNullable<int>))]
[CustomPropertyDrawer(typeof(SerializableNullable<float>))]
[CustomPropertyDrawer(typeof(SerializableNullable<bool>))]
[CustomPropertyDrawer(typeof(SerializableNullable<Vector2>))]
[CustomPropertyDrawer(typeof(SerializableNullable<Vector3>))]
public class SerializableNullableDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		Rect labelRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		Rect toggleRect = new Rect(labelRect.x, position.y, 20, position.height);
		Rect fieldRect = new Rect(labelRect.x + 25, position.y, position.width - 25 - labelRect.width, position.height);

		SerializedProperty hasValueProp = property.FindPropertyRelative("hasValue");
		SerializedProperty valueProp = property.FindPropertyRelative("value");

		var hasValue = EditorGUI.Toggle(toggleRect, hasValueProp.boolValue);

		if (hasValue)
		{
			EditorGUI.PropertyField(fieldRect, valueProp, GUIContent.none);
		}
		else
		{
			EditorGUI.LabelField(fieldRect, "(null)");
		}

		hasValueProp.boolValue = hasValue;

		EditorGUI.EndProperty();
	}
}