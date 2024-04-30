using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Quests))]
public class QuestsEditor : Editor
{
    Quests targetObject;


    private void OnEnable()
    {
        targetObject = (Quests)target;
        targetObject.CheckAllDataBlocks();
        if (!FindObjectOfType<QuestsManager>()) Debug.LogError("QuestManager absent!");
    }


    public override void OnInspectorGUI()
    {
        targetObject.GetQuestModulsList();
        var content = new GUIContent { text = "Quest system ID" };
        content.tooltip = "If there are several quest systems in the project, this ID allows you to give each system uniqueness in order to avoid duplication of quest IDs";
        targetObject.questSystemID = EditorGUILayout.TextField(content, targetObject.questSystemID);

        if (GUILayout.Button("Open quests window"))
        {
            QuestWindow window = (QuestWindow)EditorWindow.GetWindow(typeof(QuestWindow));
            targetObject.questWindow = window;
            window.Show();
            window.SetTargetObject(targetObject);
        }

        targetObject.isQuestSystemActive = EditorGUILayout.Toggle("Is quest system active", targetObject.isQuestSystemActive);

        EditorGUILayout.BeginHorizontal();
        targetObject.startTag = EditorGUILayout.TextField("Start tag", targetObject.startTag);
        if (GUILayout.Button("Reset start tag")) targetObject.startTag = "";
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Open templates window"))
        {
            QuestTemplateWindow window = (QuestTemplateWindow)EditorWindow.GetWindow(typeof(QuestTemplateWindow));
            targetObject.questWindow = window;
            window.Show();
            window.SetTargetObject(targetObject);
        }

        targetObject.optimize = EditorGUILayout.Toggle("Optimize", targetObject.optimize);
        ShowAddExtension();
        ShowExtensions();
        //DrawDefaultInspector();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetObject);
        }
    }

    private void ShowAddExtension()
    {
        var list = new List<string> { "Add extension" };
        if (!CheckComponent<AddQuestDataModulExtension>()) list.Add("AddQuestDataModulExtension");
        if (!CheckComponent<AddTagToAnalyticsExtension>()) list.Add("AddTagToAnalyticsExtension");
        if (!CheckComponent<GroupBlocksDraggingExtension>()) list.Add("GroupBlocksDraggingExtension");
        if (!CheckComponent<QuestFinishedExtension>()) list.Add("QuestFinishedExtension");

        int index = 0;
        index = EditorGUILayout.Popup(index, list.ToArray());

        switch (list[index])
        {
            case "AddQuestDataModulExtension": targetObject.gameObject.AddComponent<AddQuestDataModulExtension>(); break;
            case "AddTagToAnalyticsExtension": targetObject.gameObject.AddComponent<AddTagToAnalyticsExtension>(); break;
            case "GroupBlocksDraggingExtension": targetObject.gameObject.AddComponent<GroupBlocksDraggingExtension>(); break;
            case "QuestFinishedExtension": targetObject.gameObject.AddComponent<QuestFinishedExtension>(); break;
        }
    }

    private bool CheckComponent<T>() where T : Component
    {
        if (targetObject.GetComponent<T>() != null) return true;
        return false;
    }

    private void ShowExtensions()
    {
        var arr = targetObject.GetComponents<AbstractQuestExtension>();

        foreach (var item in arr)
        {
            item.GetExtension(targetObject);
        }
    }
}
