using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySavingData : AbstractSavingData, IObjectObservable
{
	public List<(ResourceType inventoryType, CollectablesItemCount collectables)> inventoryList = new List<(ResourceType inventoryType, CollectablesItemCount collectables)>();
	public List<(ResourceType type, string id)> resourceTypeIDsList = new List<(ResourceType type, string id)>();
	public Action<object[]> OnObjectObservableChanged;

	public override bool IsDataEmpty()
	{
		return !inventoryList.Any() && !resourceTypeIDsList.Any();
	}

	public override void ResetData(int flag = 0)
	{
		inventoryList.Clear();
		resourceTypeIDsList.Clear();
	}

	public void AddInventory(ResourceType inventoryType, CollectablesItemCount collectables)
	{
		var item = inventoryList.Find(a => a.inventoryType == inventoryType && a.collectables.resourceType == collectables.resourceType).collectables;

		if (item == null)
		{
			inventoryList.Add((inventoryType, collectables));
			item = collectables;
		}
		else item.count += collectables.count;
		item.count = Mathf.Round(item.count);
		item.count = Mathf.Clamp(item.count, 0, item.count);

		object[] arr = new object[]
		{
			item, 
			this
		};

		OnObjectObservableChanged?.Invoke(arr);
		SaveData(false);
	}

	public void ChangeInventory(ResourceType inventoryType, CollectablesItemCount collectables)
	{
		var item = inventoryList.Find(a => a.inventoryType == inventoryType && a.collectables.resourceType == collectables.resourceType).collectables;

		if (item == null)
		{
			inventoryList.Add((inventoryType, collectables));
			item = collectables;
		}
		else item.count = collectables.count;

		object[] arr = new object[]
		{
			item,
			this
		};

		OnObjectObservableChanged?.Invoke(arr);
		SaveData(false);
	}

	public void RemoveInventory(ResourceType inventoryType, CollectablesItemCount collectables)
	{
		var item = inventoryList.Find(a => a.inventoryType == inventoryType && a.collectables.resourceType == collectables.resourceType).collectables;
		if (item == null) return;
		else item.count -= collectables.count;
		item.count = Mathf.Round(item.count);
		item.count = Mathf.Clamp(item.count, 0, item.count);

		object[] arr = new object[]
		{
			item,
			this
		};

		OnObjectObservableChanged?.Invoke(arr);
		SaveData(false);
	}

	public float GetInventoryCount(ResourceType inventoryType, ResourceType type)
	{
		var item = inventoryList.Find(a => a.inventoryType == inventoryType && a.collectables.resourceType == type).collectables;
		if (item == null) return 0;
		return item.count;
	}

	public List<CollectablesItemCount> GetListByInventoryType(ResourceType inventoryType)
	{
		var list = inventoryList.FindAll(a => a.inventoryType == inventoryType).Select(b => b.collectables).ToList();
		return list;
	}

	public List<string> GetResourceTypeIDs(ResourceType type)
	{
		return resourceTypeIDsList.Where(a => a.type == type).Select(b => b.id).ToList();
	}

	public bool CheckResourceTypeID(ResourceType type, string id)
	{
		var list = resourceTypeIDsList.Where(a => a.type == type).Select(b => b.id).ToList();
		if (!list.Any()) return false;
		return list.Contains(id);
	}

	public void AddResourceTypeID(ResourceType type, string id)
	{
		resourceTypeIDsList.Add((type, id));
		SaveData(false);
	}

	public void RemoveResourceTypeID(string id)
	{
		var v = resourceTypeIDsList.Find(a => a.id == id);
		resourceTypeIDsList.Remove(v);
		SaveData(false);
	}

	public void RemoveResourceTypeID(ResourceType type, string id)
	{
		var v = resourceTypeIDsList.Find(a => a.type == type && a.id == id);
		resourceTypeIDsList.Remove(v);
		SaveData(false);
	}

	protected override void SaveDataObject()
	{
		ES3.Save(ToString(), this);
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
		var inventoryType = (ResourceType)data[0];
		var type = (ResourceType)data[1];
		var collectables = new CollectablesItemCount { resourceType = type, count = GetInventoryCount(inventoryType, type) };
		observer.OnObjectObservableChanged(new object[] { collectables, this });
	}

	public Transform GetObjectObservableTransform()
	{
		throw new NotImplementedException();
	}
}
