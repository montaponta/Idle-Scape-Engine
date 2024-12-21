using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools : MainRefs
{
	public bool isDebugActive, turnOffAllQuests;
	public List<SpawnUnitsData> spawnUnitsDataList;
	public List<DebugCraftItemData> craftItemsList;
	public List<ResourceToFillStorage> resourcesToFillStorage;
	public List<CollectablesItemCount> uniqueResourcesStorageList;
	public List<ContainerState> containerStatesList;
	public List<DebugInventory> inventoryList;
	public List<ResourceTypeID> resourceTypeIDsList;

	protected InventorySavingData InventorySavingData => GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>(SavingDataType.Inventory);

	public virtual void SetDebugData()
	{
		foreach (var item in craftItemsList)
		{
			if (item.isEnable) item.craftItem.level = item.level;
		}

		StartCoroutine(DelayStorageFill());

		foreach (var item in spawnUnitsDataList)
		{
			var pair = ("", new UnitData { id = item.id });
			GetRef<AbstractCharacterFactory>().CreateCharacters(item.count, pair, out var list);
		}

		foreach (var item in containerStatesList)
		{
			item.container.SetHackedState(item.isHacked);
		}

		foreach (var item in inventoryList)
		{
			if (item.isEnable)
				InventorySavingData.AddInventory(item.inventoryType, item.collectables);
		}

		foreach (var item in resourceTypeIDsList)
		{
			if (item.isEnable)
				InventorySavingData.AddResourceTypeID(item.resourceType, item.id);
		}
	}

	protected virtual IEnumerator DelayStorageFill()
	{
		yield return new WaitForSeconds(0.1f);

		foreach (var item in resourcesToFillStorage)
		{
			if (item.isEnable)
			{
				foreach (var collectables in item.resourceTypesList)
				{
					item.storage.ReceiveResource(collectables.resourceType, collectables.count, false);
				}
			}
		}

		yield return null;
	}

	[Serializable]
	public class ContainerState
	{
		public AbstractContainer container;
		public bool isHacked;
	}

	[Serializable]
	public class DebugInventory
	{
		public ResourceType inventoryType;
		public CollectablesItemCount collectables;
		public bool isEnable;
	}

	[Serializable]
	public class ResourceTypeID
	{
		public ResourceType resourceType;
		public string id;
		public bool isEnable;
	}

	[Serializable]
	public class DebugCraftItemData
	{
		public AbstractCraftItem craftItem;
		public int level = 1;
		public bool isEnable;
	}

	[Serializable]
	public class ResourceToFillStorage
	{
		public AbstractStorage storage;
		public List<CollectablesItemCount> resourceTypesList;
		public bool isEnable;
	}

	[Serializable]
	public class SpawnUnitsData
	{
		public string id;
		public int count;
	}
}