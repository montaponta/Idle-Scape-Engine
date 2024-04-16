using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SomeTextInInspector))]
public class SomeTextInInspectorPropertyDrawer : DecoratorDrawer
{
	public override void OnGUI(Rect position)
	{
		SomeTextInInspector target = (SomeTextInInspector)attribute;
		GUIStyle style = new GUIStyle();
		style.fontStyle = target.fontStyle;
		EditorGUI.LabelField(position, target.text, style);
	}
}
