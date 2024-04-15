using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

public class AddTagToAnalyticsExtension : AbstractQuestExtension
{
	public bool isEnable;
	public List<ListDataForAnalytics> listDataForAnalyticsList = new List<ListDataForAnalytics>();
	private GUIContent content = new GUIContent { text = "A", tooltip = "Добавить блок в аналитику" };
	[HideInInspector] public int listIndex;
	

#if UNITY_EDITOR
	public override void GetExtension(Quests quests)
	{
		base.GetExtension(quests);
		DrawFoldout("AddTagToAnalyticsExtension");
		
		if (foldout)
		{
			EditorGUILayout.BeginHorizontal();
			var arr = listDataForAnalyticsList.Select(a => a.name).ToArray();
			listIndex = EditorGUILayout.Popup("Current analytics list", listIndex, arr);
			if (GUILayout.Button("Create new Analytics list")) CreateNewAnalyticsList();
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Save all analytics lists")) SaveAllAnalyticsLists();
		}
	}

	public override void GetHeaderGUIExtension(QuestData data)
	{
		if (!isEnable) return;
		GUIStyle style = new GUIStyle(GUI.skin.button);
		var fullTag = $"{data.blockPart.tag}_{data.questBlock.questSystemID}";
		if (listIndex > listDataForAnalyticsList.Count - 1) CreateNewAnalyticsList();
		var exist = listDataForAnalyticsList[listIndex].list.Where(a => a.fullTag == fullTag).Any();
		style.normal.textColor = exist ? Color.green : Color.black;
		style.hover.textColor = exist ? Color.green : Color.black;

		if (GUILayout.Button(content, style, GUILayout.Width(20)))
		{
			if (!exist)
			{
				DataForAnalytics newData = new DataForAnalytics { name = data.blockPart.header, fullTag = fullTag };
				listDataForAnalyticsList[listIndex].list.Add(newData);
			}
			else listDataForAnalyticsList[listIndex].list.Remove(listDataForAnalyticsList[listIndex].list.Find(a => a.fullTag == fullTag));
		}
	}

	private void CreateNewAnalyticsList()
	{
		ListDataForAnalytics listData = new ListDataForAnalytics();
		listDataForAnalyticsList.Add(listData);
		listIndex = listDataForAnalyticsList.Count - 1;
		listData.name = $"List {listIndex}";
	}

	private void SaveAllAnalyticsLists()
	{
		var path = System.IO.Directory.GetCurrentDirectory();
		string directoryName = "Analytics tags lists";
		path += $@"\{directoryName}";
		if (!Directory.Exists(path)) Directory.CreateDirectory(path);

		foreach (var listData in listDataForAnalyticsList)
		{
			var localPath = path + $@"\{listData.name}.csv";
			var fs = File.Create(localPath);
			fs.Close();
			StreamWriter sw = new StreamWriter(localPath);
			sw.WriteLine("Name,FullTag");

			foreach (var item in listData.list)
			{
				sw.WriteLine($"{item.name},{item.fullTag}");
			}

			sw.Close();
		}

		print("Analytics lists saved");
	}
#endif

	[Serializable]
	public class ListDataForAnalytics
	{
		public string name;
		public List<DataForAnalytics> list = new List<DataForAnalytics>();
	}

	[Serializable]
	public class DataForAnalytics
	{
		public string name;
		public string fullTag;
	}
}
