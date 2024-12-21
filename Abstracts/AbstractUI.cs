using System.Collections.Generic;
using UnityEngine;

public class AbstractUI : MainRefs
{
	public static AbstractUI shared;
	public Transform panelsFolder, iconGridFolder, questsPanelsFolder;
	public IconGrid iconGrid;

	protected SharedObjects sharedObjects => GetRef<SharedObjects>();

	protected virtual void Awake()
	{
		shared = this;

		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<AbstractUI>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
		iconGrid = new IconGrid(this, iconGridFolder);
	}

	protected override void Start()
	{
		base.Start();
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		if (SystemInfo.deviceType == DeviceType.Handheld) Application.targetFrameRate = 60;
	}

	protected virtual void Update()
	{
		iconGrid.Update();
	}

	public virtual void ShowHidePanel(GameObject panel)
	{
		panel.SetActive(!panel.activeSelf);
	}

	public virtual void ClearContainer(Transform container)
	{
		if (container == null) return;
		for (int i = 0; i < container.childCount; i++)
		{
			if (container.GetChild(i).gameObject.tag != "DontDestroy")
				Destroy(container.GetChild(i).gameObject);
		}
	}

	public virtual string GetLocalizedString(string key, string defaultValue)
	{
		if (I2.Loc.LocalizationManager.GetTranslation(key) != null)
			return I2.Loc.LocalizationManager.GetTranslation(key);
		else return defaultValue;
	}

	public virtual (string name, string description) GetLocalizedNameDescription(IScriptableObjectData data)
	{
		var name = data.GetValueByTag<string>("Name");
		var keyName = name + "Name";
		var description = data.GetValueByTag<string>("Description");
		var keyDescription = name + "Description";
		return (GetLocalizedString(keyName, name), GetLocalizedString(keyDescription, description));
	}

	public virtual AbstractPanel CreateOpenResourceProducerPanel(AbstractResourceProducer producer)
	{
		var panel = sharedObjects.InstantiatePrefabType<AbstractPanel>(PrefabType.openResourceProducerPanel, panelsFolder);
		panel.Init(new object[] { producer });
		return panel;
	}

	public virtual AbstractPanel CreateOpenContainerPanel(AbstractContainer container)
	{
		var panel = sharedObjects.InstantiatePrefabType<AbstractPanel>(PrefabType.openContainerResourcePanel, panelsFolder);
		panel.Init(new object[] { container });
		container.OnPlayerTapObject();
		return panel;
	}

	public virtual List<AbstractPanel> CreateNeedResourcePanel(IObjectObservable observable, List<CollectablesItemCount> list)
	{
		List<AbstractPanel> panels = new List<AbstractPanel>();

		foreach (var item in list)
		{
			var panel = CreateIcon<ResourceCountPanel>(PrefabType.needResourcePanel, observable.GetObjectObservableTransform());
			panel.resourceType = item.resourceType;
			panel.icon.sprite = sharedObjects.GetResourceSprite(item.resourceType);
			panel.addStringToEnd = $"/{item.count}";
			panel.updatePosition = false;
			panel.Init(observable);
			panels.Add(panel);
		}
		return panels;
	}

	public virtual T CreateIcon<T>(PrefabType type, Transform target, PrefabType panelContainerType = PrefabType.none)
	{
		if (panelContainerType == PrefabType.none) panelContainerType = PrefabType.panelContainer;
		return iconGrid.CreateIcon<T>(sharedObjects.GetIDGameObjectData(type.ToString()), target, sharedObjects.GetIDGameObjectData(panelContainerType.ToString()));
	}

	public void DestroyIcon(Transform target, GameObject iconGO)
	{
		iconGrid.DestroyIcon(target, iconGO);
	}

	public virtual void DestroyAllIcons(Transform target)
	{
		iconGrid.DestroyAllIcons(target);
	}
}
