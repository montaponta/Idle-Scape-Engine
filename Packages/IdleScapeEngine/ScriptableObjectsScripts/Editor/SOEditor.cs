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
        if (GUILayout.Button("Create Name � Description"))
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
        if (GUILayout.Button("Create Name � Description"))
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
        if (GUILayout.Button("�������� OpenTime"))
        {
            UtilsInspectorFunctions.AddOpenTimeParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
        if (GUILayout.Button("�������� Name � Description"))
        {
            UtilsInspectorFunctions.AddNameDescriptionParamsToSO(container);
            EditorUtility.SetDirty(container);
        }
    }
}
