using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueUnitPlace : MainRefs
{
    public ResourceType type;
    protected Dictionary<int, AbstractUnit> unitsPairs = new Dictionary<int, AbstractUnit>();
    public int queueLength = 1;

    public virtual bool CanEnter(AbstractUnit unit)
    {
        if (!unitsPairs.ContainsValue(unit)) unitsPairs.Add(unitsPairs.Count, unit);

        for (int i = 0; i < queueLength; i++)
        {
            AbstractUnit value = null;
            if (unitsPairs.TryGetValue(i, out value) && value == unit) return true;
        }

        return false;
    }

    public virtual void RemoveUnit(AbstractUnit unit)
    {
        var pair = unitsPairs.First(a => a.Value == unit);
        unitsPairs.Remove(pair.Key);
        var arr = unitsPairs.ToArray();
        unitsPairs.Clear();

        for (int i = 0; i < arr.Length; i++)
        {
            unitsPairs.Add(i, arr[i].Value);
        }
    }

    public virtual float GetDistance(AbstractUnit unit)
    {
        return Vector3.Distance(transform.position, unit.transform.position);
    }

    public virtual bool IsOccupiedPlace()
    {
        return unitsPairs.Count >= GetQueueLength();
    }

    public virtual int GetEmptyPlaces()
    {
        return GetQueueLength() - unitsPairs.Count;
    }

    public virtual int GetQueueLength()
    {
        return queueLength;
    }

    public virtual void SetUnitStateIndex(AbstractUnit unit, int stateIndex) { }
    public virtual void OnQueuePlaceReached(AbstractUnit unit) { }

    public Dictionary<int, AbstractUnit> GetUnitsPairs()
    {
        return unitsPairs;
    }

    public virtual Transform GetTargetPoint(AbstractUnit unit) { return transform; }
    public virtual Transform GetLastTargetPoint() { return transform; }
    public virtual Transform GetLastFreeTargetPoint() { return transform; }

    public virtual int GetMyQueueIndex(AbstractUnit unit)
    {
        return unitsPairs.First(a => a.Value == unit).Key;
    }

    public virtual bool IsEnable() { return queueLength > 0; }
}
