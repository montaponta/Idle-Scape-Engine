using System.Collections.Generic;
using UnityEngine;

public class AbstractUI : MainRefs
{
    public static AbstractUI shared;
    public Transform panelsFolder;
    public Transform iconGridFolder;
    public GameObject craftPanel;
    public IconGrid iconGrid;

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

    public string GetLocalizedString(string key, string defaultValue)
    {
        if (I2.Loc.LocalizationManager.GetTranslation(key) != null)
            return I2.Loc.LocalizationManager.GetTranslation(key);
        else return defaultValue;
    }

    public (string name, string description) GetLocalizedNameDescription(IScriptableObjectData data)
    {
        var name = (string)data.GetValueByTag("Name", typeof(string));
        var keyName = name + "Name";
        var description = (string)data.GetValueByTag("Description", typeof(string));
        var keyDescription = name + "Description";
        return (GetLocalizedString(keyName, name), GetLocalizedString(keyDescription, description));
    }

    public void CreateOpenResourceProducerPanel(AbstractResourceProducer producer)
    {
        var go = Instantiate(SharedObjects.openResourcePanelPrefab, panelsFolder);
        go.GetComponent<AbstractPanel>().Init(new object[] { producer });
    }

    public void CreateOpenContainerPanel(AbstractContainer container)
    {
        var go = Instantiate(SharedObjects.openContainerResourcePanelPrefab, panelsFolder);
        go.GetComponent<AbstractPanel>().Init(new object[] { container });
        container.OnPlayerTapObject();
    }

    public void CreateNeedResourcePanel(IObjectObservable observable, List<CollectablesItemCount> list)
    {
        foreach (var item in list)
        {
            var panel = iconGrid.CreateIcon<ResourceCountPanel>(SharedObjects.resourceCountCraftPanel, observable.GetObjectObservableTransform(), SharedObjects.panelContainerPrefab);
            panel.resourceType = item.resourceType;
            panel.icon.sprite = SharedObjects.GetResourceSprite(item.resourceType);
            panel.addStringToEnd = $"/{item.count}";
            panel.updatePosition = false;
            panel.Init(observable);
        }
    }
}
