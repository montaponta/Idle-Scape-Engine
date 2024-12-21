using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceCollectorAbility : AbstractUnitAbility, IObjectObservable
{
	public AbstractResourceProducer resourceProducer;
	public CollectablesItemCount requiredItemCount;
	public List<CollectablesItemCount> backpackList = new List<CollectablesItemCount>();
	public Dictionary<ResourceType, GameObject> toolsGOPairs = new Dictionary<ResourceType, GameObject>();
	public AbstractCraftItem craftItem;
	public Transform collectTaskItem;
	public (AbstractStorage storage, ResourceType resourceType, float count) reservedCollectables;
	protected int animPhaseValue = -1;
	public Action<object[]> OnObjectObservableChanged;
	protected Coroutine waitAnimPhaseValueCoroutine;

	public ResourceCollectorAbility(AbstractUnit unit) : base(unit)
	{
		var arr = unit.transform.GetComponentsInChildren<ResourceTypeHelper>(true)
			.Where(a => a.enumType == ResourceType.tool);

		foreach (var item in arr)
		{
			toolsGOPairs.Add(item.resourceType, item.gameObject);
		}
	}

	protected override void SetAbilityType()
	{
		abilityType = AbilityType.collect;
	}

	public override void SetAnimationPhase(int value)
	{
		if (value == 0) TakeResource();
		if (value > 0) animPhaseValue = value;
	}

	public virtual void TakeResource()
	{
		var type = unit.GetRef<CollectAndCraftFunctions>().CheckResourceProducerNeedConditions(resourceProducer.GetSOData()).needType;
		var count = type != ResourceType.none ? unit.GetRef<CollectAndCraftFunctions>().GetCraftItem(type).GetComponent<IToolItem>().HowMuchToolCanTake() : requiredItemCount.count;
		count = Mathf.Clamp(count, 0, requiredItemCount.count);
		resourceProducer.GetResource(requiredItemCount.resourceType, count, this);
	}

	public virtual void RecieveResource(float count, float remainCount, ProduceResource produceResource)
	{
		if (requiredItemCount == null) return;
		requiredItemCount.count -= count;
		if (Mathf.Approximately(requiredItemCount.count, 0)) requiredItemCount.count = 0;
		var backpackResource = backpackList.Find(a => a.resourceType == produceResource.produceResourceType);

		if (backpackResource != null) backpackResource.count += count;
		else
		{
			backpackResource = new CollectablesItemCount { resourceType = produceResource.produceResourceType, count = count };
			backpackList.Add(backpackResource);
		}

		if (requiredItemCount.count == 0 || remainCount == 0)
		{
			if (resourceProducer.animationType == CollectAnimationType.none)
				animPhaseValue = 1;

			unit.StartCoroutine(unit.GetRef<Main>().ActionCoroutine(() =>
			{
				animPhaseValue = -1;
				ResetWaitAnimPhaseCoroutine();
				OnResourceProducerEmpty();
			}, () => animPhaseValue > 0));
		}
		else if (requiredItemCount.count > 0)
		{
			if (resourceProducer.animationType == CollectAnimationType.none)
			{
				animPhaseValue = -1;
				TakeResource();
			}
			else
			{
				waitAnimPhaseValueCoroutine = unit.StartCoroutine(unit.GetRef<Main>().ActionCoroutine(() =>
				 {
					 animPhaseValue = -1;
					 waitAnimPhaseValueCoroutine = null;
					 unit.PlayAnimation(resourceProducer.animationType.ToString(), true);
				 }, () => animPhaseValue == 1));
			}
		}
		else animPhaseValue = -1;

		object[] arr = new object[2];
		arr[0] = new CollectablesItemCount { resourceType = backpackResource.resourceType, count = backpackResource.count };
		arr[1] = this;
		OnObjectObservableChanged?.Invoke(arr);
	}

	protected virtual void ResetWaitAnimPhaseCoroutine()
	{
		if (waitAnimPhaseValueCoroutine != null)
			unit.StopCoroutine(waitAnimPhaseValueCoroutine);
		waitAnimPhaseValueCoroutine = null;
	}

	public virtual void OnResourceProducerEmpty()
	{
		if (reservedCollectables.storage)
		{
			if (requiredItemCount.count > 0)
			{
				reservedCollectables.storage.ReleaseReservedResources(requiredItemCount.resourceType, requiredItemCount.count, StorageReserveType.fill, null);
				requiredItemCount.count = 0;
			}

			unit.SetActionTypeForced(UnitActionType.readyToCollectSomething);
		}
		else if (collectTaskItem) unit.SetActionTypeForced(UnitActionType.readyForCollectingTask);
		else if (backpackList.Any()) unit.SetActionTypeForced(UnitActionType.readyForUnloadBackpack);
		else unit.SetActionTypeForced(UnitActionType.idler);
	}

	public virtual float GetBackpackCapacity(ResourceType resourceType)
	{
		var backpack = unit.GetRef<CollectAndCraftFunctions>().GetCraftItem(ResourceType.backpack);
		var baseCapacity = (float)backpack.GetSOData().GetValueByTag<float>("capacity");
		return baseCapacity * backpack.level;
	}

	public virtual float GetBackpackFreeSpace(ResourceType resourceType)
	{
		float busySpace = 0;

		foreach (var item in backpackList)
		{
			busySpace += item.count;
		}

		var resourceCapacity = GetBackpackCapacity(resourceType);
		var approx = Mathf.Approximately(resourceCapacity - busySpace, 0) ? 0 : resourceCapacity - busySpace;
		var freeSpace = Mathf.Clamp(approx, 0, resourceCapacity);
		return freeSpace;
	}

	public virtual void UnloadBackpack()
	{
		foreach (var item in backpackList)
		{
			reservedCollectables.storage.ReleaseReservedResources(item.resourceType, item.count, StorageReserveType.fill, null);
			reservedCollectables.storage.ReceiveResource(item.resourceType, item.count);
		}

		ClearBackpack();
		requiredItemCount = null;
		reservedCollectables = (null, ResourceType.none, 0);
	}

	public virtual void ClearBackpack()
	{
		var types = backpackList.Select(a => a.resourceType).ToArray();
		backpackList.Clear();

		foreach (var item in types)
		{
			object[] arr = new object[2];
			arr[0] = new CollectablesItemCount { resourceType = item, count = 0 };
			arr[1] = this;
			OnObjectObservableChanged?.Invoke(arr);
		}
	}

	public virtual void ShowTool(bool b)
	{
		foreach (var item in toolsGOPairs.Values) item.SetActive(false);
		if (!b || resourceProducer == null) return;
		var list = resourceProducer.GetSOData().GetNeedResourceList();
		if (!list.Any()) return;
		var needType = list[0].collectablesItemCount.resourceType;
		if (needType != ResourceType.none) toolsGOPairs[needType].SetActive(b);
	}

	public virtual float GetCraftTime(float baseTime)
	{
		return baseTime;
	}

	public void AddObjectObserver(IObjectObserver observer)
	{
		OnObjectObservableChanged += observer.OnObjectObservableChanged;
	}

	public void RemoveObjectObserver(IObjectObserver observer)
	{
		OnObjectObservableChanged -= observer.OnObjectObservableChanged;
	}

	public void GetObjectObservableState(IObjectObserver observer, object[] data)
	{
		var type = (ResourceType)data[0];
		object[] arr = new object[2];
		arr[0] = new CollectablesItemCount { resourceType = type, count = backpackList.Where(a => a.resourceType == type).FirstOrDefault().count };
		arr[1] = this;
		observer.OnObjectObservableChanged(arr);
	}

	public Transform GetObjectObservableTransform()
	{
		return unit.transform;
	}
}
