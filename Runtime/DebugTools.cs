using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools : MainRefs
{
	public bool isDebugActive, turnOffAllQuests;
	public int spawnUnitsCount;
	public List<DebugCraftItemData> craftItemsList;
	public List<ResourceToFillStorage> resourcesToFillStorage;
	public List<CollectablesItemCount> uniqueResourcesStorageList;
	public List<ContainerState> containerStatesList;
	public List<DebugInventory> inventoryList;
	public List<ResourceTypeID> resourceTypeIDsList;

	public void SetDebugData()
	{
		SetRefs();

		foreach (var item in craftItemsList)
		{
			if (item.isEnable) item.craftItem.level = item.level;
		}

		StartCoroutine(DelayStorageFill());
        GetRef<AbstractSavingManager>().CreateCharacters(spawnUnitsCount, new KeyValuePair<string, UnitData>(), out var list);

		foreach (var item in containerStatesList)
		{
			item.container.SetHackedState(item.isHacked);
		}

		foreach (var item in inventoryList)
		{
			if (item.isEnable)
                GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>().AddInventory(item.inventoryType, item.collectables);
		}

		foreach (var item in resourceTypeIDsList)
		{
			if (item.isEnable)
                GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>().AddResourceTypeID(item.resourceType, item.id);
		}
	}

	private IEnumerator DelayStorageFill()
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
}