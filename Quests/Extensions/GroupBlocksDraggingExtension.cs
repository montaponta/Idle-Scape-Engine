using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GroupBlocksDraggingExtension : AbstractQuestExtension
{
	private List<QuestData> questDataList = new List<QuestData>();
	private bool isEnable;
	private int strategyIndex;
	private string anchorTag;

#if UNITY_EDITOR
	public override void GetExtension(Quests quests)
	{
		base.GetExtension(quests);
		DrawFoldout("GroupBlocksDraggingExtension");

		if (foldout)
		{
			isEnable = EditorGUILayout.Toggle("Is enable", isEnable);
			var arr = new string[] { "DragBefore", "DragAfter" };
			anchorTag = EditorGUILayout.TextField("Anchor tag", anchorTag);
			strategyIndex = EditorGUILayout.Popup("Dragging strategy", strategyIndex, arr);
			if (GUILayout.Button("Start Dragging")) StartDragging();
		}
	}

	private void StartDragging()
	{
		Quests quests = GetComponent<Quests>();
		questDataList.RemoveAll(a => !quests.questDatasList.Contains(a));

		if (!questDataList.Any() || anchorTag == "")
		{
			Debug.LogError("No items to drag or anchorTag is empty");
			return;
		}

		quests.questDatasList.RemoveAll(a => questDataList.Contains(a));
		var index = quests.GetIndexByTag(anchorTag);
		if (strategyIndex == 1) index += 1;
		quests.questDatasList.InsertRange(index, questDataList);
		questDataList.Clear();
	}

	public override void GetHeaderGUIExtension(QuestData data)
	{
		if (!isEnable) return;
		var b = questDataList.Contains(data);
		var t = b;
		var index = questDataList.FindIndex(a => a == data);
		var str = index > -1 ? index.ToString() : "";
		t = EditorGUILayout.ToggleLeft(str, t, GUILayout.Width(30));
		if (t && !b) questDataList.Add(data);
		if (!t && b) questDataList.Remove(data);
	}
#endif
}
