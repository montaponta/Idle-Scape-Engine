using System.Collections.Generic;
using UnityEngine;

public class AbstractUI : MainRefs
{
    public static AbstractUI shared;
    public Transform panelsFolder,iconGridFolder, questsPanelsFolder;
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
        var name = (string)data.GetValueByTag("Name", typeof(string));
        var keyName = name + "Name";
        var description = (string)data.GetValueByTag("Description", typeof(string));
        var keyDescription = name + "Description";
        return (GetLocalizedString(keyName, name), GetLocalizedString(keyDescription, description));
    }

    public virtual void CreateOpenResourceProducerPanel(AbstractResourceProducer producer)
    {
        sharedObjects.InstantiatePrefabType<AbstractPanel>(PrefabType.openResourceProducerPanel, panelsFolder)
            .Init(new object[] { producer });
    }

    public virtual void CreateOpenContainerPanel(AbstractContainer container)
    {
        sharedObjects.InstantiatePrefabType<AbstractPanel>(PrefabType.openContainerResourcePanel, panelsFolder)
            .Init(new object[] { container });
        container.OnPlayerTapObject();
    }

    public virtual void CreateNeedResourcePanel(IObjectObservable observable, List<CollectablesItemCount> list)
    {
        foreach (var item in list)
        {
            var panel = CreateIcon<ResourceCountPanel>(PrefabType.needResourcePanel, observable.GetObjectObservableTransform());
            panel.resourceType = item.resourceType;
            panel.icon.sprite = sharedObjects.GetResourceSprite(item.resourceType);
            panel.addStringToEnd = $"/{item.count}";
            panel.updatePosition = false;
            panel.Init(observable);
        }
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
