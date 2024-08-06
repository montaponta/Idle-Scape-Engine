using System.Collections.Generic;
using UnityEngine;

public class QueueUnitPlace : MainRefs
{
    public ResourceType type;
    protected List<AbstractUnit> unitsList = new List<AbstractUnit>();
    public int queueLength = 1;

    public virtual bool CanEnter(AbstractUnit unit)
    {
        var exist = unitsList.Contains(unit);
        if (!exist) unitsList.Add(unit);

        for (int i = 0; i < queueLength; i++)
        {
            if (unitsList[i] == unit) return true;
        }

        return false;
    }

    public virtual void RemoveUnit(AbstractUnit unit)
    {
        unitsList.Remove(unit);
    }

    public virtual float GetDistance(AbstractUnit unit)
    {
        return Vector3.Distance(transform.position, unit.transform.position);
    }

    public virtual bool IsOccupiedPlace()
    {
        return unitsList.Count >= GetQueueLength();
    }

    public virtual int GetEmptyPlaces()
    {
        return GetQueueLength() - unitsList.Count;
    }

    public virtual int GetQueueLength()
    {
        return queueLength;
    }

    public virtual void SetUnitStateIndex(AbstractUnit unit, int stateIndex) { }
    public virtual void OnQueuePlaceReached(AbstractUnit unit) { }

    public List<AbstractUnit> GetUnitsList()
    {
        return unitsList;
    }

    public virtual Transform GetTargetPoint(AbstractUnit unit) { return transform; }
    public virtual Transform GetLastTargetPoint() { return transform; }
    public virtual Transform GetLastFreeTargetPoint() { return transform; }

    public virtual int GetMyQueueIndex(AbstractUnit unit)
    {
        return unitsList.FindIndex(a => a == unit);
    }

    public virtual bool IsEnable() { return queueLength > 0; }
}
