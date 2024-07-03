using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IDGameObjectDataAttribute))]
public class IDGameObjectDataAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var list = GameObject.FindObjectOfType<SharedObjects>().iDGameObjectDatasList;
        List<string> namesList = new List<string>();
        namesList.Add("None");

        foreach (var item in list)
        {
            namesList.Add(item.id);
        }

        int selectedIndex = 0;
        var arr = namesList.ToArray();
        selectedIndex = namesList.FindIndex(a => a == property.stringValue);
        selectedIndex = EditorGUI.Popup(position, selectedIndex, arr);
        if (selectedIndex < 0) selectedIndex = 0;
        property.stringValue = namesList[selectedIndex];
        EditorGUI.EndProperty();
    }
}
