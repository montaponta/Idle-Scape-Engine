using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestsManager : MainRefs
{
	public static QuestsManager shared;
	public List<GameObject> questBlocksList;
	public Action<QuestData> OnQuestStart, OnQuestComplete;
	public Dictionary<string, Quests> questsPair = new Dictionary<string, Quests>();

	private void Awake()
	{
		shared = this;
		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<QuestsManager>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	public void StartQuest(string tag, string questSystemID)
	{
		if (questsPair.ContainsKey(questSystemID)) questsPair[questSystemID].StartQuest(tag);
	}

	public void StartFirstQuest()
	{
		var quests = questsPair.First().Value;
		StartQuest(quests.questDatasList[0].blockPart.tag, quests.questSystemID);
	}

	public void FinishQuest(string tag, string questSystemID)
	{
		if (questsPair.ContainsKey(questSystemID)) questsPair[questSystemID].FinishQuest(tag);
	}

	public string[] GetQuestBlocksArray()
	{
		return questBlocksList.Select(a => a.name).ToArray();
	}

	public void AddQuestSystemToPair(Quests quests)
	{
		if (!questsPair.ContainsKey(quests.questSystemID)) questsPair.Add(quests.questSystemID, quests);
	}

	public void RemoveQuestSystemFromPair(Quests quests)
	{
		questsPair.Remove(quests.questSystemID);
	}

	public void QuestLaunched(QuestData questData)
	{
		OnQuestStart?.Invoke(questData);
	}

	public void QuestCompleted(QuestData questData)
	{
		OnQuestComplete?.Invoke(questData);
	}

	public Quests GetQuestSystemRuntime(string questSystemID)
	{
		return questsPair[questSystemID];
	}

	public bool IsQuestSystemActive(string questSystemID)
	{
		if (!questsPair.ContainsKey(questSystemID)) return false;
		else return questsPair[questSystemID].isQuestSystemActive;
	}

	public Quests GetActiveQuestSystemByIndex(int index)
	{
		var list = questsPair.Values.ToList();
		return list[index];
	}

	public (bool isComplete, QuestProgressType progressType) GetQuestState(string tag, string questSystemID)
	{
		var fullTag = $"{tag}_{questSystemID}";
		var pair = GetRef<AbstractSavingManager>().GetSavingData<QuestsSavingData>().questPair;
		if (!pair.ContainsKey(fullTag)) return (false, QuestProgressType.none);
		return (pair[fullTag] == QuestProgressType.completed, pair[fullTag]);
	}

	public (bool isComplete, QuestProgressType progressType) GetQuestState(string fullTag)
	{
		var pair = GetRef<AbstractSavingManager>().GetSavingData<QuestsSavingData>().questPair;
		if (!pair.ContainsKey(fullTag)) return (false, QuestProgressType.none);
		return (pair[fullTag] == QuestProgressType.completed, pair[fullTag]);
	}

	public void ApplicationQuit()
	{
		foreach (var item in questsPair.Values)
		{
			item.ApplicationQuit();
		}

		GetRef<AbstractSavingManager>().GetSavingData<QuestsSavingData>().SaveDataImmediately();
	}
}
