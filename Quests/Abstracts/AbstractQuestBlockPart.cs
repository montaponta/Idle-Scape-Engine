using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public abstract class AbstractQuestBlockPart : ICloneable
{
	public int id;
	public string tag;
	public Color color;
	public List<string> startQuestsWithTagsList = new List<string>();
	public string header = "";
	public GameObject questPanelPrefab;
	[NonSerialized] public bool isGoalReached;
	[NonSerialized] public GameObject spawnedGO;
	[NonSerialized] public bool canLaunchTags = true;
	public List<RevertQuestsOnExit> revertQuestsOnExitList = new List<RevertQuestsOnExit>();

	[Serializable]
	public class RevertQuestsOnExit
	{
		public string tag;
		public QuestProgressType questProgressType;
	}

	public int GenerateID(List<int> list)
	{
		id = UnityEngine.Random.Range(1000, 10000);

		while (list.Where(a => a == id).Any())
			id = UnityEngine.Random.Range(1000, 10000);
		tag = "quest_" + id;
		return id;
	}

	public string GetFullTag(Quests quests)
	{
		return $"{tag}_{quests.questSystemID}";
	}

	public virtual object Clone()
	{
		var blockPart = (AbstractQuestBlockPart)MemberwiseClone();
		blockPart.revertQuestsOnExitList = new List<RevertQuestsOnExit>();
		return blockPart;
	}

	/// <summary>
	/// If tags != null then method do Set. Else do Get and return data tags
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public virtual List<string> GetSetDataTags(List<string> tags) { return new List<string>(); }
	public virtual string[] GetTexts() { return new string[0]; }
}
