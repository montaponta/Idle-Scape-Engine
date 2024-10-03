using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitActionPermissionHandler))]
public class UnitActionPermissionHandlerEditor : Editor
{
    private int foldoutIndex;
    private UnitActionPermissionHandler targetObject;

    public override void OnInspectorGUI()
    {
        targetObject = (UnitActionPermissionHandler)target;
        targetObject.unitsPerFrame = EditorGUILayout.IntField("Units Per Frame", targetObject.unitsPerFrame);

        for (int i = 0; i < targetObject.permissions.Count; i++)
        {
            var permission = targetObject.permissions[i];
            var b = EditorGUILayout.Foldout(foldoutIndex == i, permission.type.ToString());
            if (b) foldoutIndex = i;

            if (b)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                permission.type = (UnitActionType)EditorGUILayout.EnumPopup("For actionType", permission.type);
                if (GUILayout.Button("-"))
                {
                    targetObject.permissions.Remove(permission);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                for (int i1 = 0; i1 < permission.permissionsList.Count; i1++)
                {
                    EditorGUILayout.BeginHorizontal();
                    permission.permissionsList[i1] = (UnitActionType)EditorGUILayout.EnumPopup("Allow Type", permission.permissionsList[i1]);

                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        permission.permissionsList.Add(UnitActionType.idler);
                        break;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        if (permission.permissionsList.Count > 1)
                            permission.permissionsList.RemoveAt(i1);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
        }

        if (GUILayout.Button("+"))
        {
            var permission = new UnitActionPermissionHandler.UnitActionPermission();
            permission.permissionsList.Add(UnitActionType.idler);
            targetObject.permissions.Add(permission);
        }

        if (GUI.changed) EditorUtility.SetDirty(target);
    }
}
