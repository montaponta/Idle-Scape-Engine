using System;
using UnityEngine;

public abstract class AbstractTransport : QueueUnitPlace, IIDExtention
{
    public TrasportTargetType targetType;
    public string id;
    public Transform unitsFolder;
    public Action<AbstractTransport> OnTransportReachedDestination;
    [NonSerialized] public Vector3 defaultPos, defaultRot, targetPos, targetRot;
    [NonSerialized] public float stateIndex;
    protected int seatsBusyCount;
    protected Main main => GetRef<Main>();
    protected AbstractSavingManager savingManager => GetRef<AbstractSavingManager>();

    public virtual void Init() { }

    public virtual void OnTransportEnter(AbstractUnit unit)
    {
        seatsBusyCount++;
        int index = 0;
        var tr = GetComponent<AbstractCraftItem>().GetCurrentLevelTransform();
        Transform pointsFolder = main.GetTransformChild<Transform>(tr, "PointsFolder");

        foreach (Transform item in pointsFolder)
        {
            if (item.childCount > 0) continue;
            unit.agent.enabled = false;
            unit.transform.SetParent(item);
            unit.transform.localPosition = Vector3.zero;
            unit.transform.localRotation = Quaternion.identity;
            SetUnitScale(unit, index);
            index++;
            break;
        }
    }

    public virtual void OnTransportExit(AbstractUnit unit, bool isExitAllUnits = false)
    {
        unit.transform.SetParent(unitsFolder);
        unit.agent.enabled = true;
        var pos = main.GetAgentCirclePosition(unit.transform, 3, unit.transform, 5);
        unit.agent.Warp(pos);
        unit.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);

        main.StartCoroutine(main.ActionCoroutine(() =>
        {
            RemoveUnit(unit);
            seatsBusyCount--;
            SaveTransportState();
        }, 0.02f));
    }

    public abstract void SaveTransportState();

    public virtual void ExitTransportAllUnits()
    {
        foreach (var unit in unitsList)
        {
            OnTransportExit(unit, true);
        }
    }

    public override bool CanEnter(AbstractUnit unit)
    {
        if (unitsList.Contains(unit)) return true;
        if (unitsList.Count >= queueLength) return false;
        return base.CanEnter(unit);
    }

    public virtual void MoveToTarget(Vector3 point)
    {
        targetPos = point;
    }

    protected virtual void SetUnitScale(AbstractUnit collector, int placeIndex) { }

    public virtual int GetFreeSpace()
    {
        return queueLength - unitsList.Count;
    }

    public override void OnQueuePlaceReached(AbstractUnit unit)
    {
        StartCoroutine(main.ActionCoroutine(() => { OnTransportEnter(unit); }, () => unit.agent.isStopped));
    }

    public virtual void TurnOnRevers(bool b) { }
    public virtual bool IsSaveAndLoadPosition() { return true; }
    public virtual int GetBusySeatsCount() { return seatsBusyCount; }

    public void SetID(string id)
    {
        this.id = id;
    }

    public string GetID()
    {
        return id;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public UnityEngine.Object GetObject()
    {
        return this;
    }
}
