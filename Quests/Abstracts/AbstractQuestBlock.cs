using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public abstract class AbstractQuestBlock : MainRefs
{
	public string questSystemID;
	public GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
	public GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
	[NonSerialized] public GUIStyle boldAndItalicStyle;
	protected QuestsSavingData QuestsSavingData => GetRef<AbstractSavingManager>().GetSavingData<QuestsSavingData>(SavingDataType.Quests);

	private void OnEnable()
	{
		boldAndItalicStyle = new GUIStyle();
		boldAndItalicStyle.fontStyle = FontStyle.BoldAndItalic;
		boldAndItalicStyle.normal.textColor = Color.grey;
	}

	public abstract void GetGUI(AbstractQuestBlockPart blockPart, IGeneralFunctionalWindow window);
	public abstract AbstractQuestBlockPart AddNewItem(Quests quests);
	public abstract void RemoveItem(AbstractQuestBlockPart blockPart);
	public abstract AbstractQuestBlockPart GetBlockPart(int id);
	public virtual bool IsGoalReached(AbstractQuestBlockPart blockPart)
	{
		if (blockPart.isGoalReached)
		{
			blockPart.isGoalReached = false;
			return true;
		}
		return blockPart.isGoalReached;
	}
	public abstract GUIContent GetName();
	public abstract void OnQuestStartProcedure(AbstractQuestBlockPart blockPart);
	public abstract List<AbstractQuestBlockPart> GetDataList();
	public abstract void GetDataListGUI(int index, IGeneralFunctionalWindow window);
	public abstract void SetCopyIndex(int index, ref AbstractQuestBlockPart blockPart);
	public virtual bool IsShowQuestPanel(AbstractQuestBlockPart blockPart) { return false; }
	public abstract AbstractQuestBlockPart CopyFromAnotherBlockPart(AbstractQuestBlockPart fromBlockPart);
	protected virtual void DrawTag(AbstractQuestBlockPart blockPart, IGeneralFunctionalWindow window)
	{
#if UNITY_EDITOR
		DrawRevertTagsIfExit(blockPart, window);
		var icon = Resources.Load<Texture>("BowIcon");
		var options = new GUIStyle();
		options.fixedHeight = 23;
		options.fixedWidth = 23;
		options.alignment = TextAnchor.MiddleCenter;
		EditorGUILayout.BeginVertical();
		if (IsShowQuestPanel(blockPart))
			blockPart.questPanelPrefab = (GameObject)EditorGUILayout.ObjectField("Panel prefab", blockPart.questPanelPrefab, typeof(GameObject), true);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Tag", blockPart.tag);
		if (GUILayout.Button("Copy Tag")) EditorGUIUtility.systemCopyBuffer = blockPart.tag;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical();
		for (int i = 0; i < blockPart.startQuestsWithTagsList.Count; i++)
		{
			var rect = EditorGUILayout.BeginHorizontal();
			GUILayout.Box(icon, options);
			var questData = window.GetQuestDatasList().Find(a => a.blockPart.tag == blockPart.startQuestsWithTagsList[i]);
			var label = "Start Quest With Tag";

			if (questData != null)
			{
				if (questData.blockPart.header != "") label = questData.blockPart.header;
				else label = questData.questBlock.GetName().text;
			}

			blockPart.startQuestsWithTagsList[i] = EditorGUILayout.TextField(new GUIContent { text = label, tooltip = $"{window.GetIndexByTag(blockPart.startQuestsWithTagsList[i])} {label} " }, blockPart.startQuestsWithTagsList[i]);
			if (GUILayout.Button("+", GUILayout.Width(20))) blockPart.startQuestsWithTagsList.Add("");
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				blockPart.startQuestsWithTagsList.RemoveAt(i);
				EditorGUILayout.EndHorizontal();
				break;
			}

			EditorGUILayout.EndHorizontal();
			if (rect.size != Vector2.zero && questData != null)
				window.GetMainScript().ClickRectForRewind(rect, questData.rect);
		}

		if (blockPart.startQuestsWithTagsList.Count == 0)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Box(icon, options);
			EditorGUILayout.LabelField("Add tag for start new quest");
			if (GUILayout.Button("+", GUILayout.Width(20))) blockPart.startQuestsWithTagsList.Add("");
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndVertical();
#endif
	}

	protected void Destroy(GameObject obj)
	{
		if (!obj) return;
		var unsubscriber = obj.GetComponent<IQuestUnsubscribe>();
		if (unsubscriber != null) unsubscriber.Unsubscribe();
		UnityEngine.Object.Destroy(obj);
	}

	protected void SetCopyParameters(AbstractQuestBlockPart blockPart, AbstractQuestBlockPart newBlock)
	{
		newBlock.id = blockPart.id;
		newBlock.tag = blockPart.tag;
		newBlock.color = blockPart.color;
		newBlock.startQuestsWithTagsList = blockPart.startQuestsWithTagsList;
		if (blockPart.header == "") newBlock.header = GetName().text;
		else newBlock.header = blockPart.header;
	}

	protected virtual void DrawRevertTagsIfExit(AbstractQuestBlockPart blockPart, IGeneralFunctionalWindow window)
	{
#if UNITY_EDITOR
		if (blockPart.revertQuestsOnExitList.Count == 0)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Tags to revert state if exit", boldAndItalicStyle);

			if (GUILayout.Button("+", GUILayout.Width(20)))
			{
				blockPart.revertQuestsOnExitList.Add(new AbstractQuestBlockPart.RevertQuestsOnExit());
			}

			EditorGUILayout.EndHorizontal();
		}
		else EditorGUILayout.LabelField("Tags to revert state if exit", boldAndItalicStyle);
		EditorGUILayout.BeginVertical();

		for (int i = 0; i < blockPart.revertQuestsOnExitList.Count; i++)
		{
			var tag = blockPart.revertQuestsOnExitList[i].tag;
			var questData = window.GetQuestDatasList().Find(a => a.blockPart.tag == tag);
			var label = questData != null && questData.blockPart.header != "" ? questData.blockPart.header : "Add tag to revert if exit";
			var rect1 = EditorGUILayout.BeginHorizontal();
			blockPart.revertQuestsOnExitList[i].tag = EditorGUILayout.TextField(new GUIContent { text = label, tooltip = $"{window.GetIndexByTag(tag)} {label} " }, tag);
			blockPart.revertQuestsOnExitList[i].questProgressType = (QuestProgressType)EditorGUILayout.EnumPopup(blockPart.revertQuestsOnExitList[i].questProgressType, GUILayout.Width(100));

			if (GUILayout.Button("+", GUILayout.Width(20)))
			{
				blockPart.revertQuestsOnExitList.Add(new AbstractQuestBlockPart.RevertQuestsOnExit());
			}

			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				blockPart.revertQuestsOnExitList.RemoveAt(i);
				EditorGUILayout.EndHorizontal();
				break;
			}
			EditorGUILayout.EndHorizontal();
			if (rect1.size != Vector2.zero && questData != null)
				window.GetMainScript().ClickRectForRewind(rect1, questData.rect);
		}

		EditorGUILayout.EndVertical();
#endif
	}

	public void RevertTagsOnExit(AbstractQuestBlockPart blockPart)
	{
		var fullTag = $"{blockPart.tag}_{questSystemID}";

		if (!GetRef<QuestsManager>().GetQuestState(fullTag).isComplete)
		{
			for (int i = 0; i < blockPart.revertQuestsOnExitList.Count; i++)
			{
				QuestsSavingData.ChangeQuestState($"{blockPart.revertQuestsOnExitList[i].tag}_{questSystemID}", blockPart.revertQuestsOnExitList[i].questProgressType, false);
			}
		}
	}
}
