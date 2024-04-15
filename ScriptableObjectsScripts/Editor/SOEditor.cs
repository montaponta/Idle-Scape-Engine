using UnityEditor;
using UnityEngine;

public class SOEditor { }

[CustomEditor(typeof(ResourceProducerSO))]
public class ResourceProducerSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ResourceProducerSO container = (ResourceProducerSO)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Create OpenTime"))
        {
            UtilsInspectorFunctions.AddOpenTimeParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
        if (GUILayout.Button("Create Name и Description"))
        {
            UtilsInspectorFunctions.AddNameDescriptionParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
	}
}

[CustomEditor(typeof(CraftItemSO))]
public class CraftItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
		CraftItemSO container = (CraftItemSO)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Create OpenTime"))
        {
            UtilsInspectorFunctions.AddOpenTimeParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
        if (GUILayout.Button("Create Name и Description"))
        {
            UtilsInspectorFunctions.AddNameDescriptionParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
	}
}

[CustomEditor(typeof(RequiredResourcesSO))]
public class RequiredResourcesSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RequiredResourcesSO container = (RequiredResourcesSO)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Добавить OpenTime"))
        {
            UtilsInspectorFunctions.AddOpenTimeParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
        if (GUILayout.Button("Добавить Name и Description"))
        {
            UtilsInspectorFunctions.AddNameDescriptionParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
    }
}
