using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ViewTreeQuests : EditorWindow, IGeneralFunctionalWindow
{
    GUILayoutOption horizontalOption = GUILayout.ExpandWidth(true);
    GUILayoutOption verticalOption = GUILayout.ExpandHeight(true);
    private Quests targetObject;
    private int index, questDatasTreeIndex;
    private Vector2 scrollPos;
    private List<List<QuestData>> questDatasTree = new();
    private string[] branchNamesArr;
    private string branchTag;
    private Queue<Action> actionsQueue = new();

    public void SetTargetObject(Quests quests)
    {
        targetObject = quests;
    }

    public void SetParameters(AbstractQuestBlockPart blockPart, object obj)
    {
        index = targetObject.questDatasList.FindIndex(a => a.id == blockPart.id);
        string launchTag = obj != null ? (string)obj : blockPart.tag;
        List<QuestData> list = new();
        if (!questDatasTree.Any()) questDatasTree.Add(list);
        FindFlowDown(launchTag, list);
        if (actionsQueue.Any()) StartQueueAction();
        questDatasTree.ForEach(a => FindFlowUp(launchTag, a));
        if (actionsQueue.Any()) StartQueueAction();
        branchNamesArr = new string[questDatasTree.Count];

        for (int i = 0; i < questDatasTree.Count; i++)
        {
            branchNamesArr[i] = $"branch_{i}";
        }
    }

    private void FindFlowDown(string launchTag, List<QuestData> branchQuestDatasList)
    {
        foreach (var item in targetObject.questDatasList)
        {
            var v = item.blockPart.header;

            if (item.blockPart.tag == launchTag
                && !branchQuestDatasList.Contains(item))
            {
                branchQuestDatasList.Add(item);
                FindFlowDownNextStepTag(item.blockPart.startQuestsWithTagsList, branchQuestDatasList);
                var dataTags = item.blockPart.GetSetDataTags(null);

                if (dataTags != null)
                {
                    FindFlowDownNextStepTag(dataTags, branchQuestDatasList);
                }

                break;
            }
            //else LogLoop(launchTag, item.blockPart.tag);
        }
    }

    private void FindFlowDownNextStepTag(List<string> searchingList, List<QuestData> branchQuestDatasList)
    {
        for (int i = 0; i < searchingList.Count; i++)
        {
            var nextStepTag = searchingList[i];

            if (i == 0) FindFlowDown(nextStepTag, branchQuestDatasList);
            else if (!branchQuestDatasList.Exists(a => a.blockPart.tag == nextStepTag))
            {
                actionsQueue.Enqueue(() =>
                {
                    List<QuestData> newBranchList = new(branchQuestDatasList);
                    newBranchList.Remove(newBranchList[^1]);
                    questDatasTree.Add(newBranchList);
                    FindFlowDown(nextStepTag, newBranchList);
                    if (actionsQueue.Any()) StartQueueAction();
                });
            }
            //else LogLoop(nextStepTag, questData.blockPart.tag);
        }
    }

    private void FindFlowUp(string launchTag, List<QuestData> branchQuestDatasList)
    {
        foreach (var item in targetObject.questDatasList)
        {
            var v = item.blockPart.header;
            var tagExist = item.blockPart.startQuestsWithTagsList.Exists(a => a == launchTag);
            if (tagExist) FindFlowUpNextStepTag(launchTag, item, branchQuestDatasList);
            var tagExistInDataTags = item.blockPart.GetSetDataTags(null).Exists(a => a == launchTag);

            if (tagExistInDataTags)
            {
                actionsQueue.Enqueue(() =>
                {
                    List<QuestData> newBranchList = new(branchQuestDatasList);
                    newBranchList.Remove(newBranchList[^2]);
                    questDatasTree.Add(newBranchList);
                    FindFlowUpNextStepTag(launchTag, item, newBranchList);
                    if (actionsQueue.Any()) StartQueueAction();
                });

            }
        }
    }

    private void FindFlowUpNextStepTag(string launchTag, QuestData questData, List<QuestData> branchQuestDatasList)
    {
        if (!branchQuestDatasList.Contains(questData))
        {
            branchQuestDatasList.Insert(0, questData);
            FindFlowUp(questData.blockPart.tag, branchQuestDatasList);
        }
        else LogLoop(launchTag, questData.blockPart.tag);
    }

    private void StartQueueAction()
    {
        actionsQueue.Dequeue().Invoke();
    }

    private void LogLoop(string launchTag, string targetTag)
    {
        Debug.Log($"{launchTag} loop on {targetTag}");
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

        EditorGUILayout.BeginHorizontal();
        var branchStr = questDatasTree.Count == 1 ? "branch" : "branches";
        EditorGUILayout.LabelField($"Select branch: ({questDatasTree.Count} {branchStr})");
        questDatasTreeIndex = EditorGUILayout.Popup(questDatasTreeIndex, branchNamesArr/*, GUILayout.Width(250)*/);
        EditorGUILayout.EndHorizontal();

        var currentBranch = questDatasTree[questDatasTreeIndex];
        var rect = EditorGUILayout.BeginVertical(verticalOption);

        for (int i = 0; i < currentBranch.Count; i++)
        {
            if (targetObject.questWindow == null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                Close();
                return;
            }

            ShowHeader(currentBranch[i]);
            ShowBody(currentBranch[i]);
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
