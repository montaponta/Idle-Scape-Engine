using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UtilsInspectorFunctions))]
public class UtilsInspectorFunctionsEditor : Editor
{
    bool overrideStrings, overrideStrings1;

    public override void OnInspectorGUI()
    {
        UtilsInspectorFunctions inspectorFunctions = (UtilsInspectorFunctions)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Извлечь деревья из Террейна")) inspectorFunctions.GetTreesFromTerrain();
        if (GUILayout.Button("Удалить объекты деревьев с карты")) inspectorFunctions.DestroyTrees();
        //if (GUILayout.Button("Добавить параметры имя и описание в списки AdditionalParams всех SO")) inspectorFunctions.AddNameParameterToAllSOAdditionalParams();
        //GUILayout.BeginHorizontal();
        //overrideStrings = GUILayout.Toggle(overrideStrings, "Перезаписать");
        //if (GUILayout.Button("Добавить параметры имя и описание в I2")) inspectorFunctions.AddNameParametersToI2Asset(overrideStrings);
        //GUILayout.EndHorizontal();
        //GUILayout.BeginHorizontal();
        //overrideStrings1 = GUILayout.Toggle(overrideStrings1, "Перезаписать");
        //if (GUILayout.Button("Добавить ключи продуктов магазина в I2")) inspectorFunctions.AddShopKeysToI2Asset(overrideStrings1);
        //GUILayout.EndHorizontal();
        //if (GUILayout.Button("Отключить коллайдеры в папках")) inspectorFunctions.TurnOffCollidersInFolders();
        
	}
}
