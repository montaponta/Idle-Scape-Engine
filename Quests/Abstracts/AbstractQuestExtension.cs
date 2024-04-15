using UnityEditor;
using UnityEngine;

public abstract class AbstractQuestExtension : MonoBehaviour
{
#if UNITY_EDITOR
	protected GUIStyle foldoutStyle;
	protected bool foldout;

	public virtual void GetExtension(Quests quests)
	{
		foldoutStyle = new GUIStyle(EditorStyles.foldout);
		foldoutStyle.fontStyle = FontStyle.Bold;
	}

	protected virtual void DrawFoldout(string str)
	{
		foldout = EditorGUILayout.Foldout(foldout, str, foldoutStyle);
	}

	public virtual void GetHeaderGUIExtension(QuestData data) { }
#endif
}
