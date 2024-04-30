using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestWindow : EditorWindow, IGeneralFunctionalWindow
{
	GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
	GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
	Quests targetObject;
	private string searchText = "";
	private Rect clickedRectSearch, rewindToRectSearch;
	private int delayFrameCounter, delayCounter = -1;
	private int chainInsertIndex;
	private int questIndexChoosenForInsert;
	private AbstractQuestExtension[] extensionsArr;
	private List<float> dataHeightList = new List<float>();
	private ListPopupWindow popup;
	private bool isStopDraw, isOptimize;

	public void SetTargetObject(Quests target)
	{
		targetObject = target;
		isOptimize = target.optimize;
	}

	private void OnGUI()
	{
		if (targetObject == null)
		{
			Close();
			return;
		}

		if (delayCounter == -1) isOptimize = targetObject.optimize;
		extensionsArr = targetObject.GetComponents<AbstractQuestExtension>();
		EditorGUILayout.BeginHorizontal();
		var icon = Resources.Load<Texture>("Lens");
		var options = new GUIStyle();
		options.fixedHeight = 23;
		options.fixedWidth = 23;
		options.alignment = TextAnchor.MiddleCenter;
		GUILayout.Box(icon, options);
		searchText = EditorGUILayout.TextField(searchText);
		if (GUILayout.Button("Clear", GUILayout.Width(70))) ClearSearch();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(20);
		var scrollPos = EditorGUILayout.BeginScrollView(targetObject.scrollPos);
		targetObject.scrollPos = scrollPos;
		isStopDraw = false;
		EditorGUILayout.BeginVertical(verticalOption);

		if (targetObject.questDatasList != null)
		{
			for (int i = 0; i < targetObject.questDatasList.Count; i++)
			{
				if (targetObject.questWindow == null)
				{
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndScrollView();
					Close();
					return;
				}

				if (CanShowWithSearchText(targetObject.questDatasList[i]))
				{
					ShowHeader(i);
					ShowBody(i);
					GUILayout.Space(20);
				}
				if (isStopDraw) break;
			}
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
		if (delayCounter == 0) OnDelayCounterFinished();
		else if (delayCounter > 0) delayCounter--;
		if (delayFrameCounter == 0) OnFrameFinished();
		else delayFrameCounter--;
	}

	private void OnDelayCounterFinished()
	{
		if (!targetObject.optimize) isOptimize = true;
		delayCounter = -1;
	}

	private void ShowHeader(int index)
	{
		var data = targetObject.questDatasList[index];
		Rect rect = EditorGUILayout.BeginHorizontal(horizontalOption);
		if (rect.height > 0 && searchText == "") data.rect = rect;
		var content = new GUIContent { text = index.ToString(), tooltip = $"{data.questBlock.name} {data.questBlock.GetName().tooltip}" };
		GUILayout.Label(content, EditorStyles.largeLabel);
		var str = "";
		if (data.blockPart.header == str) str = GUILayout.TextField(data.questBlock.GetName().text);
		else str = GUILayout.TextField(data.blockPart.header);
		if (str != data.questBlock.GetName().text && str != "") data.blockPart.header = str;

		if (GUILayout.Button("Delete Block"))
		{
			RemoveQuestDataFromList(data);
			ClearDataHeightList(4);
		}

		if (GUILayout.Button("Test"))
		{
			targetObject.startTag = data.blockPart.tag;
			var sm = FindObjectOfType<AbstractSavingManager>();
			if (sm) sm.dontSave = true;
			EditorApplication.isPlaying = true;
		}

		var block = data.questBlock.GetBlockPart(data.id);
		if (block != null) block.color = EditorGUILayout.ColorField(block.color, GUILayout.Width(40));
		foreach (var item in extensionsArr) item.GetHeaderGUIExtension(data);
		EditorGUILayout.EndHorizontal();

		if (targetObject.CheckRectClicked(rect))
		{
			ClearSearch();
			clickedRectSearch = rect;
			rewindToRectSearch = data.rect;
		}
	}

	private void ShowBody(int index)
	{
		if (index > targetObject.questDatasList.Count - 1) return;
		var data = targetObject.questDatasList[index];
		var rect = EditorGUILayout.BeginVertical(GUILayout.Height(5));
		if (!CheckOptimizationConditions(rect))
		{
			targetObject.GetGUI(data);
			EditorGUILayout.BeginHorizontal(horizontalOption);
			int insertIndex = -2;
			if (targetObject.insertBlockIndex == 1) insertIndex = index - 1;
			if (targetObject.insertBlockIndex == 2) insertIndex = index;
			if (targetObject.insertBlockIndex == 3) insertIndex = -2;

			if (targetObject.insertBlockIndex <= 3)
			{
				var newBlockIndex = ShowStepAddFunction("Add new quest block", index.ToString(), "Создать новый блок");
				if (newBlockIndex >= 0)
				{
					if (newBlockIndex < targetObject.questModulesList.Count)
						targetObject.AddNewDataToList(targetObject.questDatasList, newBlockIndex, insertIndex);
					else InsertTemplate(newBlockIndex - targetObject.questModulesList.Count, insertIndex);
				}

				var linkedBlockIndex = ShowStepAddFunction("Add new linked quest block", index.ToString(), "Создать блок и сделать на него ссылку");
				if (linkedBlockIndex >= 0)
				{
					if (linkedBlockIndex < targetObject.questModulesList.Count)
					{
						var questData = targetObject.AddNewDataToList(targetObject.questDatasList, linkedBlockIndex, insertIndex);
						data.blockPart.startQuestsWithTagsList.Add(questData.blockPart.tag);
					}
					else
					{
						var list = InsertTemplate(linkedBlockIndex - targetObject.questModulesList.Count, insertIndex);
						data.blockPart.startQuestsWithTagsList.Add(list[0].blockPart.tag);
					}
				}

				var linkToThisBlockIndex = ShowStepAddFunction("Add new block link to this", index.ToString(), "Создать блок и добавить в него ссылку на этот блок");
				if (linkToThisBlockIndex >= 0)
				{
					if (linkToThisBlockIndex < targetObject.questModulesList.Count)
					{
						var questData = targetObject.AddNewDataToList(targetObject.questDatasList, linkToThisBlockIndex, insertIndex);
						questData.blockPart.startQuestsWithTagsList.Add(data.blockPart.tag);
					}
					else
					{
						var list = InsertTemplate(linkToThisBlockIndex - targetObject.questModulesList.Count, insertIndex);
						list[^1].blockPart.startQuestsWithTagsList.Add(data.blockPart.tag);
					}
				}
			}
			else if (targetObject.insertBlockIndex == 4) InsertInChain(index);
			else if (targetObject.insertBlockIndex == 5) ReplaceByAnother(index);

			string[] arr = new string[] { "BlockInsertStrategy", "InsertBefore", "InsertAfter",
			"InsertEnd", "InsertInChain", "ReplaceByAnother" };
			ShowButtonWithPopup(arr[targetObject.insertBlockIndex], arr[0], arr, targetObject.insertBlockIndex, GUILayout.MinWidth(100));

			if (popup != null && popup.key == arr[0] && popup.selectedIndex != targetObject.insertBlockIndex && rect.height > 0)
			{
				targetObject.insertBlockIndex = popup.selectedIndex;
				popup = null;
				ClearDataHeightList(4);
			}

			if (GUILayout.Button("ViewList"))
			{
				ViewListQuests window = (ViewListQuests)EditorWindow.GetWindow(typeof(ViewListQuests));
				window.Show();
				window.SetTargetObject(targetObject, index);
			}

			GUIContent content = new GUIContent { image = Resources.Load<Texture>("up-arrow"), tooltip = "Move block up" };
			GUIContent content1 = new GUIContent { image = Resources.Load<Texture>("down-arrow"), tooltip = "Move block down" };
			if (GUILayout.Button(content, GUILayout.Width(23), GUILayout.Height(20))) MoveStepBlock(index, -1);
			if (GUILayout.Button(content1, GUILayout.Width(23), GUILayout.Height(20))) MoveStepBlock(index, 1);
			EditorGUILayout.EndHorizontal();
			if (dataHeightList.Count == 0 && index > 0)
			{
				EditorGUILayout.EndVertical();
				return;
			}
			if (rect.height > 0 && dataHeightList.Count - 1 < index) dataHeightList.Add(rect.height);
			if (rect.height > 0 && dataHeightList[index] != rect.height) dataHeightList[index] = rect.height;
		}
		else
		{
			EditorGUILayout.LabelField("", GUILayout.MinHeight(dataHeightList[index]));
		}
		EditorGUILayout.EndVertical();
	}

	private bool CheckOptimizationConditions(Rect rect)
	{
		if (!isOptimize || rect.y == 0) return false;
		if (dataHeightList.Count < targetObject.questDatasList.Count) return false;
		Event e = Event.current;
		var mousePosY = e.mousePosition.y;
		if (rect.y > mousePosY + 500 || rect.y < mousePosY - 500) return true;
		return false;
	}

	private void ShowFooter()
	{
		if (targetObject.questModulesList == null) return;
		var index = ShowStepAddFunction("Add New Quest Block", "footer");

		if (index >= 0)
		{
			if (index < targetObject.questModulesList.Count)
				targetObject.AddNewDataToList(targetObject.questDatasList, index);
			else InsertTemplate(index - targetObject.questModulesList.Count);
		}
	}

	private int ShowStepAddFunction(string lable, string keyAdd, string tooltip = "")
	{
		List<GUIContent> nameList = new List<GUIContent>();
		GUIContent content = new GUIContent { text = lable, tooltip = tooltip };
		nameList.Add(content);

		foreach (var item in targetObject.questModulesList)
		{
			nameList.Add(item.GetName());
		}

		foreach (var item in targetObject.questTemplatesList)
		{
			nameList.Add(new GUIContent { text = item.name });
		}

		var namesArr = nameList.Select(a => a.text).ToArray();
		var rect = EditorGUILayout.BeginHorizontal(horizontalOption);
		var defaultIndex = 0;
		var index = 0;
		string key = lable + keyAdd;
		GUILayoutOption layoutOption;
		layoutOption = GUILayout.MinWidth(100);
		ShowButtonWithPopup(lable, key, namesArr, defaultIndex, layoutOption);

		if (popup != null && popup.key == key && popup.selectedIndex != defaultIndex && rect.height > 0)
		{
			index = popup.selectedIndex;
			popup = null;
			ClearDataHeightList(4);
		}

		EditorGUILayout.EndHorizontal();
		index--;
		return index;
	}

	private void MoveStepBlock(int index, int direction)
	{
		var prevItem = (index - 1) >= 0 ? targetObject.questDatasList[index - 1] : null;
		var item = targetObject.questDatasList[index];
		var nextItem = (index + 1) <= targetObject.questDatasList.Count - 1 ? targetObject.questDatasList[index + 1] : null;

		if (direction < 0 && prevItem != null)
		{
			targetObject.questDatasList[index - 1] = item;
			targetObject.questDatasList[index] = prevItem;
		}

		if (direction > 0 && nextItem != null)
		{
			targetObject.questDatasList[index + 1] = item;
			targetObject.questDatasList[index] = nextItem;
		}
	}

	private List<QuestData> InsertTemplate(int templateIndex, int insertIndex = -2)
	{
		var template = targetObject.questTemplatesList[templateIndex];
		List<QuestData> questDatas = new List<QuestData>();
		QuestData questData = null;

		foreach (var item in template.questDatasList)
		{
			if (questData != null) insertIndex = targetObject.questDatasList.FindIndex(a => a == questData);
			int moduleIndex = targetObject.questModulesList.FindIndex(a => a == item.questBlock);
			questData = targetObject.AddNewDataToList(targetObject.questDatasList, moduleIndex, insertIndex);
			var i = questData.questBlock.GetDataList().FindIndex(a => a == item.blockPart);
			questData.questBlock.SetCopyIndex(i, ref questData.blockPart);
			questData.blockPart.header = item.blockPart.header;
			questDatas.Add(questData);
		}

		targetObject.MatchTags(template.questDatasList, questDatas);
		return questDatas;
	}

	private bool CanShowWithSearchText(QuestData questData)
	{
		if (searchText == "") return true;
		if (questData.blockPart.header.ToLower().Contains(searchText.ToLower())) return true;
		if (questData.blockPart.GetTexts().Any(a => a.ToLower().Contains(searchText.ToLower()))) return true;
		if (questData.blockPart.tag.Contains(searchText)) return true;
		if (questData.blockPart.GetSetDataTags(null).Any(a => a.Contains(searchText))) return true;
		if (questData.blockPart.startQuestsWithTagsList.Any(a => a.Contains(searchText))) return true;
		return false;
	}

	private void ClearSearch()
	{
		searchText = "";
		GUIUtility.keyboardControl = 0;
	}

	private void OnFrameFinished()
	{
		if (clickedRectSearch.size != Vector2.zero) targetObject.RewindToRect(clickedRectSearch.yMax, rewindToRectSearch.yMax);
		clickedRectSearch = rewindToRectSearch = new Rect();
	}

	private void RemoveQuestDataFromList(QuestData data)
	{
		var list = targetObject.questDatasList.Where(a => a.blockPart.startQuestsWithTagsList.Where(b => b == data.blockPart.tag).Any()).ToList();
		foreach (var item in list) item.blockPart.startQuestsWithTagsList.Remove(data.blockPart.tag);
		var list1 = targetObject.questDatasList.Where(a => a.blockPart.GetSetDataTags(null).Where(b => b == data.blockPart.tag).Any()).ToList();
		foreach (var item in list1) item.blockPart.GetSetDataTags(null).Remove(data.blockPart.tag);
		targetObject.RemoveQuestDataFromList(targetObject.questDatasList, data);
	}

	private void InsertInChain(int index)
	{
		var newBlockIndex = ShowStepAddFunction("Add new quest block", index.ToString(), "Создать новый блок");
		var questData = targetObject.questDatasList[index];
		List<string> list = new List<string> { "ChooseChainToInsert" };
		List<(string, string)> chainList = new List<(string, string)>();

		foreach (var item in targetObject.questDatasList)
		{
			if (item.blockPart.startQuestsWithTagsList.Contains(questData.blockPart.tag))
				chainList.Add((item.blockPart.tag, questData.blockPart.tag));
			if (item.blockPart.GetSetDataTags(null).Contains(questData.blockPart.tag))
				chainList.Add((item.blockPart.tag, questData.blockPart.tag));
		}

		foreach (var item in questData.blockPart.startQuestsWithTagsList) chainList.Add((questData.blockPart.tag, item));
		foreach (var item in questData.blockPart.GetSetDataTags(null)) chainList.Add((questData.blockPart.tag, item));
		foreach (var item in chainList) list.Add($"between {item.Item1} and {item.Item2}");
		var localChainInsertIndex = questIndexChoosenForInsert == index ? chainInsertIndex : 0;
		ShowButtonWithPopup(list[localChainInsertIndex], "ChooseChainToInsert" + index.ToString(), list.ToArray(), localChainInsertIndex, GUILayout.ExpandWidth(true));

		if (popup != null && popup.key == "ChooseChainToInsert" + index.ToString() && popup.selectedIndex != localChainInsertIndex)
		{
			localChainInsertIndex = popup.selectedIndex;
			popup = null;
			ClearDataHeightList(4);
		}

		var insertIndex = -2;
		QuestData firstQuestData = new QuestData(), secondQuestData = new QuestData();

		if (localChainInsertIndex > 0)
		{
			questIndexChoosenForInsert = index;
			chainInsertIndex = localChainInsertIndex;
			firstQuestData = targetObject.GetQuestDataByTag(chainList[chainInsertIndex - 1].Item1);
			secondQuestData = targetObject.GetQuestDataByTag(chainList[chainInsertIndex - 1].Item2);
			if (firstQuestData == questData) insertIndex = index;
			if (secondQuestData == questData) insertIndex = index - 1;
		}

		if (newBlockIndex >= 0)
		{
			if (localChainInsertIndex == 0)
			{
				Debug.LogError("Choose chain to insert");
				return;
			}

			if (newBlockIndex < targetObject.questModulesList.Count)
			{
				var newQuestData = targetObject.AddNewDataToList(targetObject.questDatasList, newBlockIndex, insertIndex);

				if (firstQuestData == questData)
				{
					var tagIndex = questData.blockPart.startQuestsWithTagsList.FindIndex(a => a == secondQuestData.blockPart.tag);
					if (tagIndex > -1) questData.blockPart.startQuestsWithTagsList[tagIndex] = newQuestData.blockPart.tag;
					else
					{
						tagIndex = questData.blockPart.GetSetDataTags(null).FindIndex(a => a == secondQuestData.blockPart.tag);
						if (tagIndex > -1) questData.blockPart.GetSetDataTags(null)[tagIndex] = newQuestData.blockPart.tag;
					}

					newQuestData.blockPart.startQuestsWithTagsList.Add(secondQuestData.blockPart.tag);
				}

				if (secondQuestData == questData)
				{
					var tagIndex = firstQuestData.blockPart.startQuestsWithTagsList.FindIndex(a => a == questData.blockPart.tag);
					if (tagIndex > -1) firstQuestData.blockPart.startQuestsWithTagsList[tagIndex] = newQuestData.blockPart.tag;
					else
					{
						tagIndex = firstQuestData.blockPart.GetSetDataTags(null).FindIndex(a => a == questData.blockPart.tag);
						if (tagIndex > -1) firstQuestData.blockPart.GetSetDataTags(null)[tagIndex] = newQuestData.blockPart.tag;
					}

					newQuestData.blockPart.startQuestsWithTagsList.Add(secondQuestData.blockPart.tag);
				}
			}
			else
			{
				var template = InsertTemplate(newBlockIndex - targetObject.questModulesList.Count, insertIndex);

				if (firstQuestData == questData)
				{
					var tagIndex = questData.blockPart.startQuestsWithTagsList.FindIndex(a => a == secondQuestData.blockPart.tag);
					if (tagIndex > -1) questData.blockPart.startQuestsWithTagsList[tagIndex] = template[0].blockPart.tag;
					else
					{
						tagIndex = questData.blockPart.GetSetDataTags(null).FindIndex(a => a == secondQuestData.blockPart.tag);
						if (tagIndex > -1) questData.blockPart.GetSetDataTags(null)[tagIndex] = template[0].blockPart.tag;
					}

					template[^1].blockPart.startQuestsWithTagsList.Add(secondQuestData.blockPart.tag);
				}

				if (secondQuestData == questData)
				{
					var tagIndex = firstQuestData.blockPart.startQuestsWithTagsList.FindIndex(a => a == questData.blockPart.tag);
					if (tagIndex > -1) firstQuestData.blockPart.startQuestsWithTagsList[tagIndex] = template[0].blockPart.tag;
					else
					{
						tagIndex = firstQuestData.blockPart.GetSetDataTags(null).FindIndex(a => a == questData.blockPart.tag);
						if (tagIndex > -1) firstQuestData.blockPart.GetSetDataTags(null)[tagIndex] = template[0].blockPart.tag;
					}

					template[^1].blockPart.startQuestsWithTagsList.Add(secondQuestData.blockPart.tag);
				}
			}

			chainInsertIndex = 0;
		}
	}

	private void ReplaceByAnother(int index)
	{
		var newBlockIndex = ShowStepAddFunction("Add new quest block", index.ToString(), "Создать новый блок");
		var insertIndex = index;
		var questData = targetObject.questDatasList[index];

		if (newBlockIndex >= 0)
		{
			List<QuestData> startQuestsWithTagsLinkThisList = targetObject.questDatasList
				.FindAll(a => a.blockPart.startQuestsWithTagsList
				.Contains(questData.blockPart.tag));
			List<QuestData> dataTagsLinkThisList = targetObject.questDatasList
				.FindAll(a => a.blockPart.GetSetDataTags(null)
				.Contains(questData.blockPart.tag));

			if (newBlockIndex < targetObject.questModulesList.Count)
			{
				var newQuestData = targetObject.AddNewDataToList(targetObject.questDatasList, newBlockIndex, insertIndex);

				foreach (var item in startQuestsWithTagsLinkThisList)
				{
					var tagIndex = item.blockPart.startQuestsWithTagsList.FindIndex(a => a == questData.blockPart.tag);
					item.blockPart.startQuestsWithTagsList[tagIndex] = newQuestData.blockPart.tag;
				}

				foreach (var item in dataTagsLinkThisList)
				{
					var tagIndex = item.blockPart.GetSetDataTags(null).FindIndex(a => a == questData.blockPart.tag);
					item.blockPart.GetSetDataTags(null)[tagIndex] = newQuestData.blockPart.tag;
				}

				newQuestData.blockPart.startQuestsWithTagsList = questData.blockPart.startQuestsWithTagsList;
				var dataTags = questData.blockPart.GetSetDataTags(null);
				newQuestData.blockPart.GetSetDataTags(dataTags);
				if (!newQuestData.blockPart.GetSetDataTags(null).Any() && dataTags.Any())
					newQuestData.blockPart.startQuestsWithTagsList.AddRange(dataTags);
			}
			else
			{
				var template = InsertTemplate(newBlockIndex - targetObject.questModulesList.Count, insertIndex);

				foreach (var item in startQuestsWithTagsLinkThisList)
				{
					var tagIndex = item.blockPart.startQuestsWithTagsList.FindIndex(a => a == questData.blockPart.tag);
					item.blockPart.startQuestsWithTagsList[tagIndex] = template[0].blockPart.tag;
				}

				foreach (var item in dataTagsLinkThisList)
				{
					var tagIndex = item.blockPart.GetSetDataTags(null).FindIndex(a => a == questData.blockPart.tag);
					item.blockPart.GetSetDataTags(null)[tagIndex] = template[0].blockPart.tag;
				}

				template[^1].blockPart.startQuestsWithTagsList = questData.blockPart.startQuestsWithTagsList;
				template[^1].blockPart.GetSetDataTags(questData.blockPart.GetSetDataTags(null));
			}

			RemoveQuestDataFromList(questData);
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

	private void ClearDataHeightList(int delayCount)
	{
		dataHeightList.Clear();
		isOptimize = false;
		delayCounter = delayCount;
		isStopDraw = true;
	}

	private void ShowButtonWithPopup(string buttonName, string key, string[] arr, int defaultIndex, GUILayoutOption layoutOption)
	{
		var style = new GUIStyle(EditorStyles.miniButton);
		style.alignment = TextAnchor.MiddleLeft;
		style.fixedHeight = 20;

		if (GUILayout.Button(buttonName, style, layoutOption))
		{
			Event e = Event.current;
			var popupRect = new Rect(e.mousePosition, Vector2.one);
			popup = new ListPopupWindow(key, defaultIndex, arr);
			PopupWindow.Show(popupRect, popup);
		}
	}
}
