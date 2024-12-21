using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClickablesContainersHandler : MainRefs
{
	public static ClickablesContainersHandler shared;
	[HideInInspector] public List<AbstractContainer> clickablesContainersList = new List<AbstractContainer>();
	public Dictionary<string, AbstractContainer> containersPairs = new Dictionary<string, AbstractContainer>();
	[HideInInspector] public GameObject lootContainerPanel;
	public Action<AbstractContainer> OnContainerOpenedAction;
	[HideInInspector] public List<InteractableAnimationExtension> animationList = new List<InteractableAnimationExtension>();
	Dictionary<AbstractContainer, OpenContainerData> openContainerDatas = new Dictionary<AbstractContainer, OpenContainerData>();
	private InventorySavingData InventorySavingData => GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>(SavingDataType.Inventory);

	private void Awake()
	{
		shared = this;
		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<ClickablesContainersHandler>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	protected override void Start()
	{
		base.Start();
		clickablesContainersList = GetLootContainers();
		containersPairs = clickablesContainersList.ToDictionary(a => a.id);
	}

	public List<AbstractContainer> GetLootContainers()
	{
		var world = GameObject.Find("World");
		var lootContainers = world.GetComponentsInChildren<AbstractContainer>(true).ToList();
		return lootContainers;
	}

	public (ResourceType type, float count, int needLevel, bool canOpen) CheckConditions(IScriptableObjectData SOData)
	{
		List<(ResourceType, float, int, bool)> list = new List<(ResourceType, float, int, bool)>();

		foreach (var item in SOData.GetNeedResourceList())
		{
			if (item.collectablesItemCount.resourceType == ResourceType.none) continue;
			var existResource = GetRef<CollectAndCraftFunctions>().HaveStoragesThisResource(item.collectablesItemCount.resourceType, item.collectablesItemCount.count, null);
			list.Add((item.collectablesItemCount.resourceType, item.collectablesItemCount.count, item.needResourceLevel, existResource));
		}

		if (list.Any())
		{
			var notExist = list.Where(a => !a.Item4);
			if (notExist.Any()) return notExist.First();
			else return list.First();
		}

		return (ResourceType.none, 0, 0, true);
	}

	public (ResourceType type, int needLevel, bool canOpen) CheckOtherConditions(IScriptableObjectData SOData)
	{
		var needList = SOData.GetNeedOtherResourceList();
		List<(ResourceType, int, bool)> list = new List<(ResourceType, int, bool)>();

		foreach (var item in needList)
		{
			var needResource = GetRef<CollectAndCraftFunctions>().GetCraftItem(item.needResource);
			if (needResource == null) continue;
			var existResource = needResource.IsEnable();
			var existLevel = needResource ? needResource.level >= item.needResourceLevel : false;
			list.Add((item.needResource, item.needResourceLevel, existResource && existLevel));
		}

		if (list.Any())
		{
			var notExist = list.Where(a => !a.Item3);
			if (notExist.Any()) return notExist.First();
			else return list.First();
		}

		return (ResourceType.none, 0, true);
	}

	public (ResourceType type, string id, bool canOpen) CheckInventoryConditions(IScriptableObjectData SOData)
	{
		var needList = SOData.GetNeedResourceTypeIDsList();
		List<(ResourceType, string, bool)> list = new List<(ResourceType, string, bool)>();

		foreach (var item in needList)
		{
			bool existResource = InventorySavingData.GetResourceTypeIDs(item.type).Contains(item.id);
			list.Add((item.type, item.id, existResource));
		}

		if (list.Any())
		{
			var notExist = list.Where(a => !a.Item3);
			if (notExist.Any()) return notExist.First();
			else return list.First();
		}
		return (ResourceType.none, "", true);
	}

	public float RecalculateTime(float defaultTime)
	{
		//if (humanCraft == null) humanCraft = (HumanCraft)main.GetCraftItemImprovement(ResourceType.human);
		//var level = humanCraft.collectorCraftData.GetItemLevel(type);
		//var SOData = humanCraft.GetSOData();
		//var maxCount = (float)SOData.GetValueByTag("levelCount", typeof(float));
		//var coeff = defaultTime / maxCount;
		//var time = defaultTime - coeff * level;
		//time = Mathf.Clamp(time, 0.1f, time);
		//return time;

		if (GetRef<AbstractSavingManager>().IsAllAdsDisabled()) defaultTime /= 5;
		return defaultTime;
	}

	public void StartOpenContainer(AbstractContainer lootContainer, AbstractUnit unit)
	{
		var sounds = lootContainer.GetSoundTypes();
		if (sounds != null && sounds.Count > 0)
		{
			StartStopPlayLoopSound(lootContainer, true);
		}
	}

	private void CalculateNeedResources(List<NeedResource> needResourcesList, out NeedResource needResource, out NeedResource need)
	{
		var needResources = needResourcesList.Where(a => a.collectablesItemCount.resourceType != ResourceType.none).ToList();
		needResource = null;
		need = new NeedResource();

		if (needResources.Any())
		{
			var needCraftItems = needResources.Where(a => GetRef<CollectAndCraftFunctions>().GetCraftItem(a.collectablesItemCount.resourceType)).ToList();

			if (needCraftItems.Any())
			{
				need = needCraftItems[0];
				needResource = needCraftItems.Where(a => a.needResourceLevel > GetRef<CollectAndCraftFunctions>().GetCraftItem(a.collectablesItemCount.resourceType).level).FirstOrDefault();
			}

			if (needResource == null)
			{
				var list = needResources.Except(needCraftItems).ToList();

				if (list.Any())
				{
					if (need.collectablesItemCount.resourceType == ResourceType.none) need = list[0];
					needResource = list.Where(a => !GetRef<CollectAndCraftFunctions>().HaveStoragesThisResource(a.collectablesItemCount.resourceType, a.collectablesItemCount.count, null)).FirstOrDefault();
				}

				if (needResource != null) need = needResource;
			}
			else need = needResource;
		}
	}

	public void AddNewOpenContainerTask(AbstractContainer lootContainer)
	{
		var need = lootContainer.GetPrices();

		foreach (var item in need)
		{
			var exist = lootContainer.GetAlreadyCollectedResource(item.resourceType);
			item.count = Mathf.Clamp(item.count - exist, 0, item.count);
		}

		need = need.FindAll(a => a.count > 0);

		if (need.Any())
		{
			GetRef<CollectAndCraftFunctions>().ReserveCollectablesInStorages(need, lootContainer.transform);
			GetRef<CollectAndCraftFunctions>().AddNewCollectTask(lootContainer.transform, need);
			GetRef<AbstractUI>().CreateNeedResourcePanel(lootContainer, need);
		}

		OpenContainerData openContainerData = new OpenContainerData();
		openContainerData.container = lootContainer;
		openContainerDatas.Add(lootContainer, openContainerData);
	}

	public void OnContainerOpened(AbstractContainer lootContainer)
	{
		openContainerDatas.Remove(lootContainer);
		var SOData = lootContainer.GetSOData();
		var sounds = lootContainer.GetSoundTypes();
		if (sounds != null && sounds.Count > 1) GetRef<SoundMusic>().PlaySound(sounds[1]);
		StartStopPlayLoopSound(lootContainer, false);
		OnContainerOpenedAction?.Invoke(lootContainer);
		var list = SOData.GetAdditionalParameters();
		var param = list.Where(a => a.paramName.Contains("FinishMap"));

		if (param.Any())
		{
			InventorySavingData.RemoveResourceTypeID(param.First().value);
		}
	}

	public void RecoverAllResourceProducers()
	{
		var list = clickablesContainersList.FindAll(a => a.resourceProducer != null && a.resourceProducer.recoverTimer.isTimerActive);

		foreach (var item in list)
		{
			item.resourceProducer.recoverTimer.StartTimer(0);
		}
	}

	public void StartStopPlayLoopSound(AbstractContainer lootContainer, bool b)
	{
		var sounds = lootContainer.GetSoundTypes();

		if (sounds != null && sounds.Count != 0)
		{
			if (b) GetRef<SoundMusic>().PlayStopLoopSound(sounds[0], true, this);
			else GetRef<SoundMusic>().PlayStopLoopSound(sounds[0], false, this);
		}
	}

	public void OnPlayerTapObject(AbstractContainer container) { }

	public AbstractContainer CanIOpenAnything()
	{
		if (openContainerDatas.Any())
		{
			var data = openContainerDatas.Values.Where(a => a.unitList.Count < a.container.SOData.unitsCount).FirstOrDefault();
			if (data == null) return null;
			return data.container;
		}
		return null;
	}

	public void AddUnitToOpenTask(AbstractContainer container, AbstractUnit unit)
	{
		openContainerDatas[container].unitList.Add(unit);
		container.OnContainerOpenedAction.AddListener(() => unit.SetActionTypeForced(UnitActionType.idler), true);
	}

	public void AddUnitToReadyList(AbstractContainer container, AbstractUnit unit)
	{
		openContainerDatas[container].readyUnitsList.Add(unit);
	}

	public void RemoveUnitFromTaskLists(AbstractContainer container, AbstractUnit unit)
	{
		if (!openContainerDatas.ContainsKey(container)) return;
		openContainerDatas[container].unitList.Remove(unit);
		openContainerDatas[container].readyUnitsList.Remove(unit);
	}

	public bool CheckAllUnitsReadyToOpen(AbstractContainer container)
	{
		return openContainerDatas[container].readyUnitsList.Count == openContainerDatas[container].unitList.Count;
	}
}
