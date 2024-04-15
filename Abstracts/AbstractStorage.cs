using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractStorage : AbstractCraftItem
{
	public List<CollectablesItemCount> resourceTypeList;
	private Dictionary<ResourceType, float> reservedResourceFillPair = new Dictionary<ResourceType, float>();
	private Dictionary<Transform, List<CollectablesItemCount>> reservedResourceTakePair = new Dictionary<Transform, List<CollectablesItemCount>>();


	public List<CollectablesItemCount> GetItemsList()
	{
		return GetCollectedList();
	}

	public virtual bool CanStorageKeepThisResource(ResourceType resourceType)
	{
		var v = resourceTypeList.Find(a => a.resourceType == resourceType);
		if (v != null) return true;
		return false;
	}

	public virtual float GetStorageFreeSpace(ResourceType resourceType, StorageReserveType reserveType)
	{
		if (!IsEnable()) return 0;
		var capacity = GetStorageCapacity(resourceType);
		if (capacity == 0) return 0;
		var v = GetCollectedList().Find(a => a.resourceType == resourceType);
		var count = v != null ? v.count : 0;
		float reserved = 0;
		if (reserveType == StorageReserveType.fill)
			reserved = reservedResourceFillPair.ContainsKey(resourceType) ? reservedResourceFillPair[resourceType] : 0;

		if (reserveType == StorageReserveType.take)
		{
			float sum = 0;
			foreach (var item in reservedResourceTakePair)
			{
				var collectables = item.Value.Find(a => a.resourceType == resourceType);
				if (collectables != null) sum += collectables.count;
			}

			reserved = sum;
		}

		return Mathf.Clamp(capacity - count - reserved, 0, capacity);
	}

	public virtual float GetStorageFillingSpace(ResourceType resourceType, StorageReserveType reserveType)
	{
		var v = GetCollectedList().Find(a => a.resourceType == resourceType);
		var count = v != null ? v.count : 0;
		float reserved = 0;
		if (reserveType == StorageReserveType.fill)
			reserved = reservedResourceFillPair.ContainsKey(resourceType) ? reservedResourceFillPair[resourceType] : 0;

		if (reserveType == StorageReserveType.take)
		{
			float sum = 0;
			foreach (var item in reservedResourceTakePair)
			{
				var collectables = item.Value.Find(a => a.resourceType == resourceType);
				if (collectables != null) sum += collectables.count;
			}

			reserved = sum;
		}

		return Mathf.Clamp(count - reserved, 0, count);
	}

    public virtual void UpdateVisualStorageResourceCount(ResourceType resourceType, float exist) { }

    public virtual void TakeStorageResource(ResourceType resourceType, float count)
    {
        SpendResource(resourceType, count);
        var exist = GetAlreadyCollectedResource(resourceType);
        object[] arr = new object[2];
        arr[0] = new CollectablesItemCount { resourceType = resourceType, count = exist };
        arr[1] = this;
        OnObjectObservableChanged?.Invoke(arr);
        UpdateVisualStorageResourceCount(resourceType, exist);
    }

    public virtual float GetStorageCapacity(ResourceType type)
	{
		var collectables = resourceTypeList.Find(a => a.resourceType == type);
		if (collectables == null) return 0;
		var capacity = collectables.count;
		return capacity * level;
	}

	public virtual void ReserveResource(ResourceType resourceType, float count, StorageReserveType reserveType, Transform target)
	{
		if (reserveType == StorageReserveType.fill)
		{
			if (reservedResourceFillPair.ContainsKey(resourceType))
				reservedResourceFillPair[resourceType] += count;
			else reservedResourceFillPair.Add(resourceType, count);
		}

		if (reserveType == StorageReserveType.take)
		{
			CollectablesItemCount newCollectables = new CollectablesItemCount { resourceType = resourceType, count = count };

			if (!reservedResourceTakePair.ContainsKey(target))
			{
				reservedResourceTakePair.Add(target, new List<CollectablesItemCount> { newCollectables });
			}
			else
			{
				var list = reservedResourceTakePair[target];
				var collectables = list.Find(a => a.resourceType == resourceType);
				if (collectables == null) list.Add(newCollectables);
				else collectables.count += count;
			}
		}
	}

	public virtual void ReleaseReservedResources(ResourceType resourceType, float count, StorageReserveType reserveType, Transform target)
	{
		if (reserveType == StorageReserveType.fill)
		{
			if (reservedResourceFillPair.ContainsKey(resourceType))
				reservedResourceFillPair[resourceType] = Mathf.Clamp(reservedResourceFillPair[resourceType] - count, 0, reservedResourceFillPair[resourceType]);
		}

		if (reserveType == StorageReserveType.take)
		{
			if (reservedResourceTakePair.ContainsKey(target))
			{
				var list = reservedResourceTakePair[target];
				var collectables = list.Find(a => a.resourceType == resourceType);
				collectables.count = Mathf.Clamp(collectables.count - count, 0, collectables.count);
				if (Mathf.Approximately(collectables.count, 0)) collectables.count = 0;
			}
		}
	}

	public override void ReceiveResource(ResourceType resourceType, float count, bool reduceInCollectingResource = true)
	{
        base.ReceiveResource(resourceType, count, reduceInCollectingResource);
        var exist = GetAlreadyCollectedResource(resourceType);
        object[] arr = new object[2];
        arr[0] = new CollectablesItemCount { resourceType = resourceType, count = exist };
        arr[1] = this;
        OnObjectObservableChanged?.Invoke(arr);
        UpdateVisualStorageResourceCount(resourceType, exist);
    }

	protected override void OnAssemblingCompleteProcedure()
	{
		var price = GetCraftPrices1(level, SOData.GetIPricesFromNeedResourceList());
		var collected = GetCollectedList();

		foreach (var item in price)
		{
			var collectables = collected.Find(a => a.resourceType == item.resourceType);
			collectables.count -= item.count;
			if (Mathf.Approximately(collectables.count, 0)) collectables.count = 0;
			collectables.count = Mathf.Clamp(0, collectables.count, collectables.count);
			OnResourceCountChanged?.Invoke(item.resourceType, collectables.count);
			object[] arr = new object[2];
			arr[0] = new CollectablesItemCount { resourceType = item.resourceType, count = collectables.count };
			arr[1] = this;
			OnObjectObservableChanged?.Invoke(arr);
		}
	}

	public virtual float HowMuchResourcesCanBeGiven(ResourceType type, Transform target)
	{
		if (target == null || !reservedResourceTakePair.ContainsKey(target))
			return GetStorageFillingSpace(type, StorageReserveType.take);
		else
		{
			var list = reservedResourceTakePair[target];
			var collectables = list.Find(a => a.resourceType == type);
			if (collectables != null && collectables.count > 0) return collectables.count;
			else return GetStorageFillingSpace(type, StorageReserveType.take);
		}
	}

	public virtual Transform GetTargetFromAbility(ResourceCollectorAbility ability)
	{
		Transform target = null;
		if(ability == null) return target;
		if (ability.craftItem) target = ability.craftItem.transform;
		else if (ability.collectTaskItem) target = ability.collectTaskItem;
		else target = ability.GetUnit().transform;
		return target;
	}

	protected override void OnReceiveResourceProcedure(ResourceType resourceType, float count) { }
}

public enum StorageReserveType
{
	fill, take, none
}