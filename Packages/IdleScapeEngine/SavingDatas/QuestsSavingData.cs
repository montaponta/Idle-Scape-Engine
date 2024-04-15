using System.Collections.Generic;
using System.Linq;

public class QuestsSavingData : AbstractSavingData
{
	public Dictionary<string, QuestProgressType> questPair = new Dictionary<string, QuestProgressType>();

	public override bool IsDataEmpty()
	{
		return !questPair.Where(a => a.Value != QuestProgressType.none).Any();
	}

	public override void ResetData(int flag = 0)
	{
		questPair.Clear();
	}

	public void AddQuestDatas(List<QuestData> list, Quests quests)
	{
		foreach (var item in list)
		{
			if (!questPair.ContainsKey(item.blockPart.GetFullTag(quests))) questPair.Add(item.blockPart.GetFullTag(quests), QuestProgressType.none);
		}
	}

	public void ChangeQuestState(string fullTag, QuestProgressType type, bool save = true)
	{
		questPair[fullTag] = type;
		if (save) SaveData(false);
	}

	protected override void SaveDataObject()
	{
		ES3.Save(ToString(), this);
	}
}
