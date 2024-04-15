using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionPermissionHandler : MonoBehaviour
{
    public List<UnitActionPermission> permissions = new List<UnitActionPermission>();
    private Dictionary<UnitActionType, UnitActionPermission> permissionsPairs = new Dictionary<UnitActionType, UnitActionPermission>();

    [Serializable]
    public class UnitActionPermission
    {
        public UnitActionType type;
        public List<UnitActionType> permissionsList = new List<UnitActionType>();
    }

    private void Start()
    {
        foreach (var item in permissions)
        {
            permissionsPairs.Add(item.type, item);
        }
    }

    public bool CheckPermission(UnitActionType askType, UnitActionType activeType)
    {
        return permissionsPairs[askType].permissionsList.Contains(activeType);
    }
}
