using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContainerSavingData : AbstractSavingData
{
	public Dictionary<string, LootContainerState> pairs = new Dictionary<string, LootContainerState>();

	public override bool IsDataEmpty()
	{
		return pairs.Any();
	}

	public override void ResetData(int flag = 0)
	{
		pairs.Clear();
	}

	public override void LoadData()
	{
		var list = GetLootContainers();

		foreach (var item in list)
		{
			if (item.gameObject.activeSelf) continue;
			if (!pairs.ContainsKey(item.id)) continue;
			item.gameObject.SetActive(pairs[item.id].isVisible);
		}
	}

	public override void SaveData(bool collectParams)
	{
		if (savingManager.dontSave) return;
		if (collectParams)
		{
			var arr = Object.FindObjectsOfType<AbstractContainer>(true);

			foreach (var item in arr)
			{
				AddDataToPair(item);
			}
		}

		base.SaveData(collectParams);
	}

	public void SaveContainerState(AbstractContainer container)
	{
		AddDataToPair(container);
		SaveData(false);
	}

	private void AddDataToPair(AbstractContainer container)
	{
		var state = new LootContainerState();
		state.id = container.GetID();
		state.isEnable = container.IsContainerEnable();
		state.isHacked = container.IsHacked();
		state.isVisible = container.IsVisible();
		state.name = container.transform.parent.name;
		state.pos = ExtensionClasses.ConvertToVector3Serialized(container.transform.parent.position);
		state.rot = ExtensionClasses.ConvertToVector3Serialized(container.transform.parent.eulerAngles);
		if (pairs.ContainsKey(container.id)) pairs[container.id] = state;
		else pairs.Add(container.id, state);
	}

	public void GetContainerState(AbstractContainer container)
	{
		if (!pairs.ContainsKey(container.id)) return;
		var data = pairs[container.id];
		container.SetEnableState(data.isEnable, null);
		container.SetHackedState(data.isHacked);
		container.SetVisible(data.isVisible);
		if (ExtensionClasses.IsVector3SerializedEqualToZero(data.pos)) return;
		container.transform.parent.position = ExtensionClasses.ConvertToVector3(data.pos);
		container.transform.parent.eulerAngles = ExtensionClasses.ConvertToVector3(data.rot);
	}

	public LootContainerState GetContainerStateInfo(string id)
	{
		if (!pairs.ContainsKey(id)) return null;
		var data = pairs[id];
		return data;
	}

	protected override void SaveDataObject()
	{
		ES3.Save(ToString(), this);
	}

	public List<AbstractContainer> GetLootContainers()
	{
		var world = savingManager.GetRef<Main>().GetWorldGO();
		var lootContainers = world.GetComponentsInChildren<AbstractContainer>(true).ToList();
		return lootContainers;
	}
}
