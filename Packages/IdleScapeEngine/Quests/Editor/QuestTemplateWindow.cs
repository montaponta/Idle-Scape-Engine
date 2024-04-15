using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestTemplateWindow : EditorWindow, IGeneralFunctionalWindow
{
	GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
	GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
	Quests targetObject;
	private int templateArrIndex = -1;
	private QuestTemplate template;

	public void SetTargetObject(Quests target)
	{
		targetObject = target;
	}

	private void OnGUI()
	{
		if (targetObject == null)
		{
			Close();
			return;
		}

		GUILayout.Space(20);
		var strings = targetObject.questTemplatesList.Select(a => a.name).ToList();
		strings.Insert(0, "Create new template");
		var guidArr = AssetDatabase.FindAssets(null, new string[] { "Assets/Prefabs/QuestsTemplates" });
		Dictionary<string, string> templatesPair = new Dictionary<string, string>();

		foreach (var item in guidArr)
		{
			var path = AssetDatabase.GUIDToAssetPath(item);
			var name = path.Replace("Assets/Prefabs/QuestsTemplates/", "");
			name = name.Replace(".prefab", "");

			if (!strings.Contains(name))
			{
				strings.Add($"Import {name}");
				templatesPair.Add(name, path);
			}
		}

		templateArrIndex = EditorGUILayout.Popup("Choose template", templateArrIndex, strings.ToArray());
		targetObject.scrollPos = EditorGUILayout.BeginScrollView(targetObject.scrollPos);
		EditorGUILayout.BeginVertical(verticalOption);
		var templateIndex = templateArrIndex - 1;

		if (templateArrIndex == 0)
		{
			targetObject.questTemplatesList.Add(new QuestTemplate());
			templateIndex = targetObject.questTemplatesList.Count - 1;
			templateArrIndex = targetObject.questTemplatesList.Count;
		}

		template = templateIndex > -1 && templateArrIndex <= targetObject.questTemplatesList.Count ? targetObject.questTemplatesList[templateIndex] : null;

		if (template != null && template.questDatasList != null)
		{
			template.name = EditorGUILayout.TextField(template.name);
			GUILayout.Space(20);

			for (int i = 0; i < template.questDatasList.Count; i++)
			{
				if (targetObject.questWindow == null)
				{
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndScrollView();
					Close();
					return;
				}

				ShowHeader(i);
				ShowBody(i);
				GUILayout.Space(20);
			}
		}

		if (templateArrIndex > 0 && templateArrIndex < strings.Count)
		{
			var importTemplateName = strings[templateArrIndex].Replace("Import ", "");
			if (templatesPair.ContainsKey(importTemplateName))
				ImportTemplate(templatesPair[importTemplateName]);
		}


		ShowFooter();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(targetObject);

			foreach (var item in targetObject.questModulesList)
			{
				EditorUtility.SetDirty(item);
			}
		}

		GUILayout.Space(20);
	}

	private void ShowHeader(int index)
	{
		var data = template.questDatasList[index];
		Rect rect = EditorGUILayout.BeginHorizontal(horizontalOption);
		if (rect.height > 0) data.rect = rect;
		var content = new GUIContent { text = index.ToString(), tooltip = $"{data.questBlock.name} {data.questBlock.GetName().tooltip}" };
		GUILayout.Label(content, EditorStyles.largeLabel);
		var str = "";
		if (data.blockPart.header == str) str = GUILayout.TextField(data.questBlock.GetName().text);
		else str = GUILayout.TextField(data.blockPart.header);
		if (str != data.questBlock.GetName().text && str != "") data.blockPart.header = str;

		if (GUILayout.Button("Delete Block"))
		{
			RemoveQuestDataFromList(data);
		}

		var block = data.questBlock.GetBlockPart(data.id);
		if (block != null) block.color = EditorGUILayout.ColorField(block.color, GUILayout.Width(40));
		EditorGUILayout.EndHorizontal();
	}

	private void ShowBody(int index)
	{
		if (index > template.questDatasList.Count - 1) return;
		var data = template.questDatasList[index];
		var rect = EditorGUILayout.BeginVertical(GUILayout.Height(5));
		targetObject.GetGUI(data);
		EditorGUILayout.BeginHorizontal(horizontalOption);
		int insertIndex = -2;
		if (targetObject.insertBlockIndex == 1) insertIndex = index - 1;
		if (targetObject.insertBlockIndex == 2) insertIndex = index;
		if (targetObject.insertBlockIndex == 3) insertIndex = -2;

		var newBlockIndex = ShowStepAddFunction("Add new quest block", "Создать новый блок");
		if (newBlockIndex >= 0)
		{
			targetObject.AddNewDataToList(template.questDatasList, newBlockIndex, insertIndex);
		}

		var linkedBlockIndex = ShowStepAddFunction("Add new linked quest block", "Создать блок и сделать на него ссылку");
		if (linkedBlockIndex >= 0)
		{
			var questData = targetObject.AddNewDataToList(template.questDatasList, linkedBlockIndex, insertIndex);
			data.blockPart.startQuestsWithTagsList.Add(questData.blockPart.tag);
		}

		var linkToThisBlockIndex = ShowStepAddFunction("Add new block link to this", "Создать блок и добавить в него ссылку на этот блок");
		if (linkToThisBlockIndex >= 0)
		{
			var questData = targetObject.AddNewDataToList(template.questDatasList, linkToThisBlockIndex, insertIndex);
			questData.blockPart.startQuestsWithTagsList.Add(data.blockPart.tag);
		}

		string[] arr = new string[] { "BlockInsertStrategy", "InsertBefore", "InsertAfter", "InsertEnd" };
		targetObject.insertBlockIndex = EditorGUILayout.Popup(targetObject.insertBlockIndex, arr);

		GUIContent content = new GUIContent { image = Resources.Load<Texture>("up-arrow"), tooltip = "Move block up" };
		GUIContent content1 = new GUIContent { image = Resources.Load<Texture>("down-arrow"), tooltip = "Move block down" };
		if (GUILayout.Button(content, GUILayout.Width(23), GUILayout.Height(20))) MoveStepBlock(index, -1);
		if (GUILayout.Button(content1, GUILayout.Width(23), GUILayout.Height(20))) MoveStepBlock(index, 1);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		if (rect.height > 0) data.rect.height += rect.height;
	}

	private void ShowFooter()
	{
		if (targetObject.questModulesList == null) return;
		if (template == null) return;
		var index = ShowStepAddFunction("Add New Quest Block");
		if (index >= 0) targetObject.AddNewDataToList(template.questDatasList, index);
		if (template != null && GUILayout.Button("Export template")) ExportTemplate();

		if (template != null && GUILayout.Button("Delete template"))
		{
			var count = template.questDatasList.Count;
			List<QuestData> list = new List<QuestData>(template.questDatasList);

			for (int i = 0; i < count; i++)
			{
				targetObject.RemoveQuestDataFromList(template.questDatasList, list[i]);
			}

			targetObject.questTemplatesList.Remove(template);
			templateArrIndex = -1;
		}
	}

	private int ShowStepAddFunction(string lable, string tooltip = "")
	{
		List<GUIContent> nameList = new List<GUIContent>();
		GUIContent content = new GUIContent { text = lable, tooltip = tooltip };
		nameList.Add(content);

		foreach (var item in targetObject.questModulesList)
		{
			nameList.Add(item.GetName());
		}

		var namesArr = nameList.ToArray();
		var defaultIndex = 0;
		EditorGUILayout.BeginHorizontal(horizontalOption);
		var index = EditorGUILayout.Popup(defaultIndex, namesArr);
		EditorGUILayout.EndHorizontal();
		index--;
		return index;
	}

	private void MoveStepBlock(int index, int direction)
	{
		var prevItem = (index - 1) >= 0 ? template.questDatasList[index - 1] : null;
		var item = template.questDatasList[index];
		var nextItem = (index + 1) <= template.questDatasList.Count - 1 ? template.questDatasList[index + 1] : null;

		if (direction < 0 && prevItem != null)
		{
			template.questDatasList[index - 1] = item;
			template.questDatasList[index] = prevItem;
		}

		if (direction > 0 && nextItem != null)
		{
			template.questDatasList[index + 1] = item;
			template.questDatasList[index] = nextItem;
		}
	}

	public IGeneralFunctionalWindow OpenWindow(string windowName)
	{
		if (windowName == "ViewTreeQuests")
		{
			ViewTreeQuests window = (ViewTreeQuests)EditorWindow.GetWindow(typeof(ViewTreeQuests));
			window.Show();
			window.SetTargetObject(targetObject);
			return window;
		}
		return null;
	}

	public bool CanShowElement(string elementName)
	{
		if (elementName == "View Branch") return false;
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
		return template.questDatasList;
	}

	public int GetIndexByTag(string tag)
	{
		for (int i = 0; i < template.questDatasList.Count; i++)
		{
			if (template.questDatasList[i].blockPart.tag == tag)
			{
				return i;
			}
		}

		return -1;
	}

	private void ExportTemplate()
	{
		var templateGO = new GameObject(template.name);
		var questsManager = FindObjectOfType<QuestsManager>();
		List<AbstractQuestBlock> questModulesList = new List<AbstractQuestBlock>();
		List<QuestData> questDatas = new List<QuestData>();

		foreach (var item in template.questDatasList)
		{
			var block = questsManager.questBlocksList.Find(a => a.name == item.questBlock.name);
			var go = Instantiate(block, templateGO.transform);
			go.name = block.name;
			questModulesList.Add(go.GetComponent<AbstractQuestBlock>());
		}

		for (int i = 0; i < template.questDatasList.Count; i++)
		{
			var blockPart = questModulesList[i].CopyFromAnotherBlockPart(template.questDatasList[i].blockPart);
			var questData = new QuestData { questBlock = questModulesList[i], blockPart = blockPart, id = blockPart.GenerateID(targetObject.GetIdsList()) };
			questDatas.Add(questData);
		}

		targetObject.MatchTags(template.questDatasList, questDatas);

		if (!Directory.Exists("Assets/Prefabs"))
			AssetDatabase.CreateFolder("Assets", "Prefabs");
		if (!Directory.Exists("Assets/Prefabs/QuestsTemplates"))
			AssetDatabase.CreateFolder("Assets/Prefabs", "QuestsTemplates");
		string path = $"Assets/Prefabs/QuestsTemplates/{templateGO.name}.prefab";
		path = AssetDatabase.GenerateUniqueAssetPath(path);
		PrefabUtility.SaveAsPrefabAssetAndConnect(templateGO, path, InteractionMode.UserAction);
		DestroyImmediate(templateGO);
	}

	private void ImportTemplate(string path)
	{
		var prefab = PrefabUtility.LoadPrefabContents(path);
		var prefabModules = prefab.transform.GetComponentsInChildren<AbstractQuestBlock>();
		var questsManager = FindObjectOfType<QuestsManager>();
		List<QuestData> questDatas = new List<QuestData>();
		List<QuestData> prefabQuestDatas = new List<QuestData>();
		QuestTemplate newTemplate = new QuestTemplate();
		newTemplate.name = prefab.name;

		foreach (var item in prefabModules)
		{
			AbstractQuestBlock questBlockToImport = targetObject.questModulesList.Find(a => a.name == item.name);

			if (questBlockToImport == null)
			{
				var module = questsManager.questBlocksList.Find(a => a.name == item.name);
				var newModule = Instantiate(module, targetObject.transform);
				newModule.name = item.name;
				questBlockToImport = newModule.GetComponent<AbstractQuestBlock>();
			}

			var blockPart = questBlockToImport.CopyFromAnotherBlockPart(item.GetDataList()[0]);
			blockPart.GenerateID(targetObject.GetIdsList());
			QuestData questData = new QuestData { questBlock = questBlockToImport, blockPart = blockPart, id = blockPart.id };
			questDatas.Add(questData);
			QuestData prefabQuestData = new QuestData { questBlock = item, blockPart = item.GetDataList()[0], id = blockPart.id };
			prefabQuestDatas.Add(prefabQuestData);
		}

		newTemplate.questDatasList = questDatas;
		targetObject.MatchTags(prefabQuestDatas, questDatas);
		targetObject.questTemplatesList.Add(newTemplate);
		PrefabUtility.UnloadPrefabContents(prefab);
	}

	private void RemoveQuestDataFromList(QuestData data)
	{
		var list = template.questDatasList.Where(a => a.blockPart.startQuestsWithTagsList.Where(b => b == data.blockPart.tag).Any()).ToList();
		foreach (var item in list) item.blockPart.startQuestsWithTagsList.Remove(data.blockPart.tag);
		var list1 = template.questDatasList.Where(a => a.blockPart.GetSetDataTags(null).Where(b => b == data.blockPart.tag).Any()).ToList();
		foreach (var item in list1) item.blockPart.GetSetDataTags(null).Remove(data.blockPart.tag);
		targetObject.RemoveQuestDataFromList(template.questDatasList, data);
	}
}
