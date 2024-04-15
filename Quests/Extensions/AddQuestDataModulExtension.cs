using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class AddQuestDataModulExtension : AbstractQuestExtension
{
#if UNITY_EDITOR
	public override void GetExtension(Quests quests)
	{
		base.GetExtension(quests);
		DrawFoldout("AddQuestDataModulExtension");

		if (foldout)
		{
			var index = -1;
			List<string> names = new List<string>();
			foreach (Transform item in quests.transform) names.Add(item.name);
			var content = new GUIContent { text = "Add quest data modul", tooltip = "Добавить новый квест модуль данных в данную систему квестов" };
			var questsManager = FindObjectOfType<QuestsManager>();
			var blocksNames = questsManager.GetQuestBlocksArray().Except(names).ToArray();
			index = EditorGUILayout.Popup(content, index, blocksNames);

			if (index > -1)
			{
				var block = questsManager.questBlocksList.Find(a => a.name == blocksNames[index]);
				var go = Instantiate(block, quests.transform);
				go.name = blocksNames[index];
				block.GetComponent<AbstractQuestBlock>().questSystemID = quests.questSystemID;
			}
		}
	}
#endif
}
