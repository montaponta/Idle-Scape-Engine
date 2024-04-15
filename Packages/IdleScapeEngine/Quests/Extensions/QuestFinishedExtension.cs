using UnityEditor;
using UnityEngine;

public class QuestFinishedExtension : AbstractQuestExtension
{
	public bool isEnable;

	public override void GetHeaderGUIExtension(QuestData data)
	{
		if (!isEnable) return;
		string str = "X";
		GUIStyle style = new GUIStyle();
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
		style.fixedWidth = 20;

		if (EditorApplication.isPlaying
			&& data.questBlock.GetRef<QuestsManager>().GetQuestState(data.blockPart.tag, data.questBlock.questSystemID).isComplete)
		{
			str = "V";
			style.normal.textColor = Color.green;
		}

		GUILayout.Button(str, style);
	}
}
