using UnityEditor;
using UnityEngine;

public class ListPopupWindow : PopupWindowContent
{
	private string[] arr;
	public string key;
	public int selectedIndex;
	private GUIStyle style;
	private Vector2 scroll;
	public bool isReadyToNull;

	public ListPopupWindow(string key, int index, string[] displayedOptions)
	{
		arr = displayedOptions;
		this.key = key;
		this.selectedIndex = index;
		style = new GUIStyle(EditorStyles.miniButton);
		style.alignment = TextAnchor.MiddleLeft;
	}

	public override void OnGUI(Rect rect)
	{
		scroll = EditorGUILayout.BeginScrollView(scroll);
		for (var i = 0; i < arr.Length; i++)
		{
			var item = arr[i];
			if (GUILayout.Button(item, style))
			{
				selectedIndex = i;
				isReadyToNull = true;
				editorWindow.Close();
			}
		}
		EditorGUILayout.EndScrollView();
	}
}
