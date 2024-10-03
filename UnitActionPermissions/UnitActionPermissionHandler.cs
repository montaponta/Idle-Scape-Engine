using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionPermissionHandler : MonoBehaviour
{
    public List<UnitActionPermission> permissions = new();
    public int unitsPerFrame = 100;
    private Dictionary<UnitActionType, UnitActionPermission> permissionsPairs = new();
    private int counter;
    private Dictionary<AbstractUnit, bool> unitsWhoAskedPair = new();

    [Serializable]
    public class UnitActionPermission
    {
        public UnitActionType type;
        public List<UnitActionType> permissionsList = new();
        private Dictionary<UnitActionType, bool> permissionDictionary = new();

        public void FillDictionary()
        {
            for (int i = 0; i < permissionsList.Count; i++)
            {
                permissionDictionary.Add(permissionsList[i], true);
            }
        }

        public bool CheckActionType(UnitActionType type)
        {
            return permissionDictionary.ContainsKey(type);
        }
    }

    private void Start()
    {
        foreach (var item in permissions)
        {
            item.FillDictionary();
            permissionsPairs.Add(item.type, item);
        }
    }

    private void Update()
    {
        if (counter == 0) unitsWhoAskedPair.Clear();
        counter = 0;
    }

    public bool CheckPermission(UnitActionType askType, UnitActionType activeType)
    {
        return permissionsPairs[askType].CheckActionType(activeType);
    }

    public bool CanIAskPermission(AbstractUnit unit)
    {
        if (counter >= unitsPerFrame) return false;
        if (!unitsWhoAskedPair.ContainsKey(unit)) unitsWhoAskedPair.Add(unit, true);
        else return false;
        counter++;
        return true;
    }
}
