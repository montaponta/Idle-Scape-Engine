using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectAndCraftFunctions : MainRefs
{
	public static CollectAndCraftFunctions shared;
	[NonSerialized] public List<AbstractStorage> storagesList;
	[NonSerialized] public List<AbstractCraftItem> craftItemsList;
	[NonSerialized] public List<(ResourceType resourceType, AbstractResourceProducer producer)> activeResourceProducersList = new List<(ResourceType resourceType, AbstractResourceProducer producer)>();
	[NonSerialized] public float priceMultiplier = 1;
	[NonSerialized] public List<AbstractUnit> resourceCollectorsList;
	private int speedMultiplayer = 1;
	public Dictionary<AbstractCraftItem, CraftData> craftDatas = new Dictionary<AbstractCraftItem, CraftData>();
	public Dictionary<Transform, CollectData> collectDatas = new Dictionary<Transform, CollectData>();
	public Action<AbstractCraftItem> OnItemCrafted;
	protected InventorySavingData InventorySavingData => GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>(SavingDataType.Inventory);

	private void Awake()
	{
		shared = this;
		craftItemsList = FindObjectsOfType<AbstractCraftItem>(true).ToList();
		craftItemsList.ForEach(a => a.OnAssemblingCompleteSendItem += (craftItem) => OnItemCrafted?.Invoke(craftItem));
		storagesList = FindObjectsOfType<AbstractStorage>(true).ToList();
		resourceCollectorsList = FindObjectsOfType<AbstractUnit>().ToList();

		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<CollectAndCraftFunctions>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	public (ResourceType needType, float needLevel, bool canOpen) CheckResourceProducerNeedConditions(IResourceProducerSOData SOData)
	{
		List<(ResourceType needType, float needLevel, bool canOpen)> list = new List<(ResourceType, float, bool)>();

		foreach (var item in SOData.GetProduceResourceList())
		{
			var need = item.needResource;
			var needResourceType = need.collectablesItemCount.resourceType;
			if (needResourceType == ResourceType.none) continue;

			if (GetCraftItem(needResourceType).level < need.needResourceLevel)
				return (needResourceType, need.needResourceLevel, false);
			else list.Add((needResourceType, need.needResourceLevel, true));
		}

		if (list.Any()) return (list[0].needType, list[0].needLevel, list[0].canOpen);
		return (ResourceType.none, 0, true);
	}

	public (ResourceType type, float needCountOrLevel, bool canOpen, NeedConditionType needType) CheckCraftItemNeedConditions(AbstractCraftItem craftItem)
	{
		List<(ResourceType, float, bool, NeedConditionType)> list = new List<(ResourceType, float, bool, NeedConditionType needType)>();
		var SOData = craftItem.GetSOData();

		foreach (var item in SOData.GetNeedOtherResourceList())
		{
			if (craftItem.level >= item.forCraftLevel) continue;
			var needCraftItem = GetCraftItem(item.needResource);
			if (needCraftItem == null) continue;
			if (craftItem.level + 1 == item.forCraftLevel
				&& needCraftItem.level < item.needResourceLevel)
				list.Add((item.needResource, item.needResourceLevel, false, NeedConditionType.craftItem));
		}

		var prices = craftItem.GetCraftPrices(craftItem.level + 1, craftItem.GetSOData().GetIPricesFromNeedResourceList());

		foreach (var item in prices)
		{
			var existResource = HaveStoragesThisResource(item.Item1.resourceType, item.Item1.count, craftItem);
			list.Add((item.Item1.resourceType, item.Item1.count, existResource, NeedConditionType.collectableResource));
		}

		prices = craftItem.GetCraftPrices(craftItem.level + 1, craftItem.GetSOData().GetIPricesFromNeedInventoryResourceList(), true);

		foreach (var item in prices)
		{
			var exist = InventorySavingData.GetInventoryCount((ResourceType)item.Item2.GetOtherParameters()[0], item.Item1.resourceType) >= item.Item1.count;
			list.Add((item.Item1.resourceType, item.Item1.count, exist, NeedConditionType.inventory));
		}

		if (list.Any())
		{
			var notExist = list.Where(a => !a.Item3);
			if (notExist.Any()) return notExist.First();
			else return list.First();
		}

		return (ResourceType.none, 0, true, NeedConditionType.none);
	}

	public AbstractCraftItem GetCraftItem(ResourceType type)
	{
		var craftItem = craftItemsList.Find(a => a.GetSOData().GetCraftItemType() == type);
		return craftItem;
	}

	public bool HaveStoragesThisResource(ResourceType resourceType, float count, AbstractCraftItem craftItem)
	{
		var list = GetStoragesWithThisResource(resourceType);
		float exist = 0;
		var tr = craftItem ? craftItem.transform : null;

		foreach (var item in list)
		{
			exist += item.HowMuchResourcesCanBeGiven(resourceType, tr);
		}

		return exist >= count;
	}

	public List<AbstractStorage> GetStoragesWithThisResource(ResourceType resourceType)
	{
		var v = storagesList.Where(a => a.CanStorageKeepThisResource(resourceType));
		if (v.Any()) return v.ToList();
		return null;
	}

	public void AddResourceProducerToList(ResourceType resourceType, AbstractResourceProducer producer)
	{
		activeResourceProducersList.Add((resourceType, producer));
		SortResourceList();
	}

	public void RemoveResourceProducerFromList(ResourceType resourceType, AbstractResourceProducer producer)
	{
		var item = activeResourceProducersList.Find(a => a.producer == producer && a.resourceType == resourceType);
		activeResourceProducersList.Remove(item);
	}

	public void SortResourceList()
	{
		activeResourceProducersList = activeResourceProducersList
			.OrderByDescending(a => a.producer.GetSOData().GetPriority()).ToList();
	}

	public void AddNewCraftTask(AbstractCraftItem craftItem)
	{
		if (craftDatas.ContainsKey(craftItem)) return;
		CraftData craftData = new CraftData();
		craftData.craftItem = craftItem;
		var need = craftItem.GetCraftPrices1(craftItem.level + 1, craftItem.GetSOData().GetIPricesFromNeedResourceList());

		foreach (var item in need)
		{
			var exist = craftItem.GetAlreadyCollectedResource(item.resourceType);
			item.count = Mathf.Clamp(item.count - exist, 0, item.count);
		}

		need = need.FindAll(a => a.count > 0);

		if (need.Count > 0)
		{
			ReserveCollectablesInStorages(need, craftItem.transform);
			AddNewCollectTask(craftItem.transform, need);
		}

		craftData.collectables = need;
		craftItem.isInImproveProgress = true;
		craftDatas.Add(craftItem, craftData);
		craftItem.OnAssemblingCompleteUnsubscribe.AddListener(() => craftDatas.Remove(craftItem), true);
		craftItem.OnItemChosenForAssembly();

	}

	public float GetCraftTime(AbstractCraftItem craftItem)
	{
		var levelIncrement = craftItem.GetSOData().GetCraftParams().levelIncrement * craftItem.level;
		var craftItemBaseTime = craftItem.GetSOData().GetCraftParams().craftTime;
		var craftItemTime = craftItemBaseTime + craftItemBaseTime * levelIncrement;
		var time = (resourceCollectorsList[0].GetUnitAbility<ResourceCollectorAbility>(AbilityType.collect).GetCraftTime(craftItemTime)) / speedMultiplayer;
		return time;
	}

	public AbstractCraftItem CanICraftAnything()
	{
		if (craftDatas.Any())
		{
			var data = craftDatas.Values.FirstOrDefault(a => a.unitsList.Count < a.craftItem.GetSOData().GetUnitsCount());
			if (data == null) return null;
			return data.craftItem;
		}
		return null;
	}

	public CollectablesItemCount WhatResourceToCollectForCraft(AbstractCraftItem craftItem, ResourceCollectorAbility resourceCollectorAbility, bool isTakeTask)
	{
		CollectablesItemCount collectables = new CollectablesItemCount();
		CraftData data = null;
		if (!craftDatas.TryGetValue(craftItem, out data)) return collectables;
		if (!data.collectables.Any()) return collectables;
		collectables.resourceType = data.collectables[0].resourceType;
		var canTake = resourceCollectorAbility.GetBackpackFreeSpace(data.collectables[0].resourceType);
		collectables.count = Mathf.Clamp(data.collectables[0].count, 0, canTake);

		if (isTakeTask)
		{
			data.collectables[0].count -= collectables.count;
			if (Mathf.Approximately(data.collectables[0].count, 0)) data.collectables.RemoveAt(0);
		}

		return collectables;
	}

	public bool CheckAllUnitsReadyToCraft(AbstractCraftItem craftItem)
	{
		return craftDatas[craftItem].readyUnitsList.Count == craftDatas[craftItem].unitsList.Count;
	}

	public void AddUnitToCraftTask(AbstractCraftItem craftItem, AbstractUnit unit, Action OnCraftComplete = null)
	{
		craftDatas[craftItem].unitsList.Add(unit);
		if (OnCraftComplete == null)
			craftItem.OnAssemblingCompleteUnsubscribe.AddListener(() => unit.SetActionTypeForced(UnitActionType.idler), true);
		else craftItem.OnAssemblingCompleteUnsubscribe.AddListener(OnCraftComplete);
	}

	public void AddUnitToCraftReadyList(AbstractCraftItem craftItem, AbstractUnit unit)
	{
		craftDatas[craftItem].readyUnitsList.Add(unit);
	}

	public void RemoveUnitFromCraftTaskLists(AbstractCraftItem craftItem, AbstractUnit unit)
	{
		if (!craftDatas.ContainsKey(craftItem)) return;
		craftDatas[craftItem].unitsList.Remove(unit);
		craftDatas[craftItem].readyUnitsList.Remove(unit);
	}

	public void ReturnResourcesToCollectForCraft(AbstractCraftItem craftItem, CollectablesItemCount collectables)
	{
		craftDatas[craftItem].collectables.Add(new CollectablesItemCount { resourceType = collectables.resourceType, count = collectables.count });
	}

	public void AddNewCollectTask(Transform target, List<CollectablesItemCount> collectables)
	{
		List<CollectablesItemCount> list = new List<CollectablesItemCount>();

		foreach (var item in collectables)
		{
			CollectablesItemCount collectablesItem = new CollectablesItemCount { resourceType = item.resourceType, count = item.count };
			list.Add(collectablesItem);
		}

		CollectData collectData = new CollectData();
		collectData.target = target;
		collectData.collectables = list;
		collectDatas.Add(target, collectData);
	}

	public Transform CanICollectAnything()
	{
		if (collectDatas.Any())
		{
			return collectDatas.First().Key;
		}
		return null;
	}

	public CollectablesItemCount WhatResourceToCollect(Transform target, ResourceCollectorAbility resourceCollectorAbility, bool isTakeTask)
	{
		CollectablesItemCount collectables = new CollectablesItemCount();
		CollectData data = null;
		if (!collectDatas.TryGetValue(target, out data)) return collectables;
		if (!data.collectables.Any()) return collectables;
		collectables.resourceType = data.collectables[0].resourceType;
		var canTake = resourceCollectorAbility.GetBackpackFreeSpace(data.collectables[0].resourceType);
		collectables.count = Mathf.Clamp(data.collectables[0].count, 0, canTake);

		if (isTakeTask)
		{
			data.collectables[0].count -= collectables.count;
			if (Mathf.Approximately(data.collectables[0].count, 0)) data.collectables.RemoveAt(0);
			if (!data.collectables.Exists(a => a.count > 0)) collectDatas.Remove(target);
		}

		return collectables;
	}

	public void ReturnResourcesToCollect(Transform target, CollectablesItemCount collectables)
	{
		if (collectDatas.ContainsKey(target))
			collectDatas[target].collectables.Add(new CollectablesItemCount { resourceType = collectables.resourceType, count = collectables.count });
		else AddNewCollectTask(target, new List<CollectablesItemCount> { collectables });
	}

	public void ReserveCollectablesInStorages(List<CollectablesItemCount> collectables, Transform target)
	{
		foreach (var item in collectables)
		{
			var type = item.resourceType;
			var storages = GetStoragesWithThisResource(type);
			float remainToReserve = item.count;

			foreach (var storage in storages)
			{
				var storageFillingSpace = storage.HowMuchResourcesCanBeGiven(type, target);
				var canReserve = Mathf.Clamp(remainToReserve, 0, storageFillingSpace);
				storage.ReserveResource(type, canReserve, StorageReserveType.take, target);
				remainToReserve -= canReserve;
				if (Mathf.Approximately(remainToReserve, 0)) remainToReserve = 0;
				if (remainToReserve == 0) break;
			}
		}
	}
}