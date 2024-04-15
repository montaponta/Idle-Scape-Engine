using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ViewTreeQuests : EditorWindow, IGeneralFunctionalWindow
{
	GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
	GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
	private Quests targetObject;
	private int index;
	private Vector2 scrollPos;
	private List<QuestData> questDatasTree = new List<QuestData>();

	public void SetTargetObject(Quests quests)
	{
		targetObject = quests;
	}

	public void SetParameters(AbstractQuestBlockPart blockPart, object obj)
	{
		index = targetObject.questDatasList.FindIndex(a => a.id == blockPart.id);
		string launchTag = blockPart.tag;

		foreach (var item in targetObject.questDatasList)
		{
			foreach (var item1 in targetObject.questDatasList)
			{
				if (item1.blockPart.startQuestsWithTagsList.Find(a => a == launchTag) != null)
				{
					questDatasTree.Insert(0, item1);
					launchTag = item1.blockPart.tag;
					break;
				}
			}
		}

		questDatasTree.Add(targetObject.questDatasList[index]);
		if (obj == null) return;
		launchTag = (string)obj;

		foreach (var item in targetObject.questDatasList)
		{
			foreach (var item1 in targetObject.questDatasList)
			{
				if (item1.blockPart.tag == launchTag)
				{
					questDatasTree.Add(item1);
					if (item1.blockPart.startQuestsWithTagsList.Any())
					{
						launchTag = item1.blockPart.startQuestsWithTagsList[0];
						break;
					}
					else return;
				}
			}
		}
	}

	private void OnGUI()
	{
		if (targetObject == null)
		{
			Close();
			return;
		}

		GUILayout.Space(20);
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		var rect = EditorGUILayout.BeginVertical(verticalOption);

		for (int i = 0; i < questDatasTree.Count; i++)
		{
			if (targetObject.questWindow == null)
			{
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
				Close();
				return;
			}

			ShowHeader(questDatasTree[i]);
			ShowBody(questDatasTree[i]);
			GUILayout.Space(20);
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}
	private void ShowHeader(QuestData questData)
	{
		var blockPart = questData.blockPart;
		var name = blockPart.header != "" ? new GUIContent { text = blockPart.header } : questData.questBlock.GetName();
		var stepDataIndex = targetObject.questDatasList.FindIndex(a => a.blockPart == blockPart);
		var rect = EditorGUILayout.BeginHorizontal(horizontalOption);

		if (targetObject.questDatasList[index].blockPart == blockPart)
		{
			EditorGUI.DrawRect(new Rect(0, rect.y, rect.width, rect.height), new Color(1, 1, 0, 0.3f));
		}

		GUILayout.Label(stepDataIndex.ToString(), EditorStyles.largeLabel);
		GUILayout.Label(name, EditorStyles.largeLabel);
		EditorGUILayout.EndHorizontal();

	}

	private void ShowBody(QuestData questData)
	{
		EditorGUILayout.BeginVertical(GUILayout.Height(5));
		var i = questData.questBlock.GetDataList().FindIndex(a => a.id == questData.id);
		questData.questBlock.GetDataListGUI(i, this);
		EditorGUILayout.EndVertical();
	}

	public IGeneralFunctionalWindow OpenWindow(string windowName)
	{
		return null;
	}

	public bool CanShowElement(string elementName)
	{
		if (elementName == "View Branch") return false;
		return true;
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
