using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ViewListQuests : EditorWindow, IGeneralFunctionalWindow
{
	GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
	GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
	private Quests targetObject;
	private int index;
	private Vector2 scrollPos;

	public void SetTargetObject(Quests target, int index)
	{
		targetObject = target;
		this.index = index;
	}

	private void OnGUI()
	{
		if (targetObject == null)
		{
			Close();
			return;
		}

		var list = targetObject.questDatasList[index].questBlock.GetDataList();
		var stepsList = targetObject.questDatasList.FindAll(a => list.Contains(a.blockPart));
		GUILayout.Space(20);
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		var rect = EditorGUILayout.BeginVertical(verticalOption);

		for (int i = 0; i < stepsList.Count; i++)
		{
			if (targetObject.questWindow == null)
			{
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
				Close();
				return;
			}

			ShowHeader(stepsList[i].blockPart);
			ShowBody(stepsList[i].blockPart);
			GUILayout.Space(20);
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}
	private void ShowHeader(AbstractQuestBlockPart blockPart)
	{
		var name = blockPart.header != "" ? new GUIContent { text = blockPart.header } : targetObject.questDatasList[index].questBlock.GetName();
		var i = targetObject.questDatasList[index].questBlock.GetDataList().FindIndex(a => a == blockPart);
		var blockPartLocal = targetObject.questDatasList[index].questBlock.GetDataList()[i];
		var stepDataIndex = targetObject.questDatasList.FindIndex(a => a.blockPart == blockPart);
		var rect = EditorGUILayout.BeginHorizontal(horizontalOption);

		if (targetObject.questDatasList[index].blockPart == blockPartLocal)
		{
			EditorGUI.DrawRect(new Rect(0, rect.y, rect.width, rect.height), new Color(1, 1, 0, 0.3f));
		}

		GUILayout.Label(stepDataIndex.ToString(), EditorStyles.largeLabel);
		GUILayout.TextField(name.text);

		if (GUILayout.Button("Copy"))
		{
			targetObject.questDatasList[index].questBlock.SetCopyIndex(i, ref targetObject.questDatasList[index].blockPart);
			Close();
		}

		EditorGUILayout.EndHorizontal();

	}

	private void ShowBody(AbstractQuestBlockPart blockPart)
	{
		EditorGUILayout.BeginVertical(GUILayout.Height(5));
		var i = targetObject.questDatasList[index].questBlock.GetDataList().FindIndex(a => a == blockPart);
		targetObject.questDatasList[index].questBlock.GetDataListGUI(i, this);
		EditorGUILayout.EndVertical();
	}

	public IGeneralFunctionalWindow OpenWindow(string windowName)
	{
		return null;
	}

	public bool CanShowElement(string elementName)
	{
		return true;
	}

	public void SetParameters(AbstractQuestBlockPart blockPart, object obj)
	{
		;
	}

	public Quests GetMainScript()
	{
		return targetObject;
	}

	public List<QuestData> GetQuestDatasList()
	{
		return targetObject.questDatasList;
	}

	public int GetIndexByTag(string tag)
	{
		return targetObject.GetIndexByTag(tag);
	}
}
