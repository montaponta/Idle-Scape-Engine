using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TagLinksPopupWindow : PopupWindowContent
{
    private QuestWindow questWindow;
    private QuestData questData;
    private List<QuestData> tagLinksDatasList = new();
    private Vector2 scroll;

    public TagLinksPopupWindow(QuestWindow questWindow, QuestData data)
    {
        this.questWindow = questWindow;
        this.questData = data;
        tagLinksDatasList = FindLinks(data.blockPart.tag);
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(250, 70);
    }

    public override void OnGUI(Rect rect)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var data in tagLinksDatasList)
        {
            var str = "";
            if (data.blockPart.header == str) str = data.questBlock.GetName().text;
            else str = data.blockPart.header;
            str += $" - {data.blockPart.tag}";

            if (GUILayout.Button(str))
            {
                var quests = questWindow.GetMainScript();
                quests.RewindToRect(questData.rect.position.y, quests.GetQuestDataByTag(data.blockPart.tag).rect.position.y);
                editorWindow.Close();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private List<QuestData> FindLinks(string tag)
    {
        List<QuestData> links = new();

        foreach (var item in questWindow.GetMainScript().questDatasList)
        {
            if (item.blockPart.startQuestsWithTagsList.Exists(a => a == tag)) links.Add(item);
            if (item.blockPart.GetSetDataTags(null).Exists(a => a == tag)) links.Add(item);
        }

        return links;
    }
}
