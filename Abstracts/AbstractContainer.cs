using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbstractContainer : MainRefs, IIDExtention, ISODataHandler, IResourceReciever, IObjectObservable
{
    public RequiredResourcesSO SOData;
    public bool isNeedTap, isCreateResourcesPanel;
    public CollectAnimationType openTypeAnim = CollectAnimationType.none;
    public Vector2 spawnForceRangeZ, spawnForceRangeXY;
    public Transform lootSpawnPoint, openPoint;
    public bool isAlreadyHacked;
    [Tooltip("opening, openFinished, looting")]
    public List<SoundType> soundTypes;
    [SerializeField] private List<AbstractContainer> parentContainersList;
    public string id;
    protected bool isHacked, isInHackProcess, isEnable;
    private Timer unlockTimer = new Timer(TimerMode.counterFixedUpdate);
    [HideInInspector] public AbstractResourceProducer resourceProducer;
    public UnsubscribingDelegate OnContainerOpenedAction = new UnsubscribingDelegate();
    public List<AbstractContainer> alsoOpenContainersList;
    private ProgressBarUI bar;
    private List<CollectablesItemCount> collectedList = new List<CollectablesItemCount>();
    public Action<object[]> OnObjectObservableChanged;

    protected override void Start()
    {
        base.Start();
        GetRef<AbstractSavingManager>().GetSavingData<ContainerSavingData>().GetContainerState(this);
        resourceProducer = GetComponent<AbstractResourceProducer>();
        if (!openPoint) openPoint = transform;
        ShowHidePreview(false);
        if (!isHacked && isAlreadyHacked) OnContainerOpened();
    }

    private void FixedUpdate()
    {
        unlockTimer.TimerUpdate();
        if (bar) bar.SetValue(unlockTimer.GetTimeNormalized());
    }

    public virtual void StartOpenContainer(AbstractUnit unit)
    {
        if (!unlockTimer.isTimerActive)
        {
            isInHackProcess = true;
            unlockTimer.OnTimerReached += OnContainerOpened;
            GetRef<AbstractUI>().iconGrid.DestroyAllIcons(transform);
            var sharedObjects = GetRef<SharedObjects>();
            bar = GetRef<AbstractUI>().iconGrid.CreateIcon<ProgressBarUI>(sharedObjects.GetIDGameObjectData("roundBarPrefab"), transform, sharedObjects.GetIDGameObjectData("panelContainerPrefab"));
            var time = (float)SOData.GetValueByTag("OpenTime", typeof(float));
            unlockTimer.StartTimer(time);
        }
        else unlockTimer.count++;

        GetRef<ClickablesContainersHandler>().StartOpenContainer(this, unit);
    }

    public void StopOpeningContainer()
    {
        if (!unlockTimer.isTimerActive) return;
        isInHackProcess = false;
        unlockTimer.CancelTimer();
        unlockTimer.OnTimerReached -= OnContainerOpened;
    }

    public virtual void OnContainerOpened()
    {
        if (resourceProducer) resourceProducer.isEnable = true;
        isInHackProcess = false;
        SetEnableState(true, null);
        SetHackedState(true);
        OnContainerOpenedAction?.Invoke();
        OnContainerOpenedProcedure();
        GetRef<ClickablesContainersHandler>().OnContainerOpened(this);
        ShowHidePreview(false);
        GetRef<AbstractUI>().iconGrid.DestroyIcon(transform, bar.gameObject);
        //var waypoint = GetComponentInChildren<Waypoint_Indicator>();
        //if (waypoint) Destroy(waypoint.gameObject);
        GetRef<AbstractSavingManager>().GetSavingData<ContainerSavingData>().SaveContainerState(this);

        foreach (var item in alsoOpenContainersList)
        {
            if (!item.IsHacked()) item.OnContainerOpened();
        }
    }

    protected virtual void OnContainerOpenedProcedure() { }

    public virtual void OnPlayerTapObject()
    {
        GetRef<ClickablesContainersHandler>().OnPlayerTapObject(this);
    }

    public void CreateOpenContainerTask()
    {
        isInHackProcess = true;
        GetRef<ClickablesContainersHandler>().AddNewOpenContainerTask(this);
    }

    public List<ProduceResource> GetLootItems()
    {
        return resourceProducer.SOData.itemsList;
    }

    public bool IsHacked()
    {
        return isHacked;
    }

    public virtual void SetHackedState(bool state)
    {
        isHacked = state;
    }

    public bool IsContainerEnable()
    {
        return isEnable;
    }

    public virtual void SetEnableState(bool state, AbstractContainer childContainer)
    {
        isEnable = state;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public virtual bool IsVisible()
    {
        return transform.parent.gameObject.activeSelf;
    }

    public virtual void SetVisible(bool b)
    {
        transform.parent.gameObject.SetActive(b);
    }

    public CollectAnimationType GetOpenTypeAnim()
    {
        return openTypeAnim;
    }

    public Timer GetUnlockTimer()
    {
        return unlockTimer;
    }

    public Transform GetOpenPoint()
    {
        return openPoint;
    }

    public bool IsNeedTap()
    {
        return isNeedTap;
    }

    public bool IsInHackProcess()
    {
        return isInHackProcess;
    }

    public List<SoundType> GetSoundTypes()
    {
        return soundTypes;
    }

    public void IncreaseOpenUnitsCount()
    {
        unlockTimer.count++;
    }

    public virtual IScriptableObjectData GetSOData()
    {
        return SOData;
    }

    public virtual void SetID(string id)
    {
        this.id = id;
    }

    public virtual string GetID()
    {
        return id;
    }

    public virtual GameObject GetGameObject()
    {
        return gameObject;
    }

    public virtual UnityEngine.Object GetObject()
    {
        return this;
    }

    public virtual void ResourceIsEmpty()
    {
        var parentContainers = GetComponentsInParent<AbstractContainer>().ToList();
        parentContainers.Remove(this);
        if (parentContainers.Any()) parentContainers[0].SetEnableState(isEnable, this);
    }

    public void ShowHidePreview(bool b)
    {
        foreach (Transform item in transform.parent)
        {
            if (item.tag == "Preview") item.gameObject.SetActive(b);
        }
    }

    public virtual void ResetContainerState()
    {
        SetEnableState(false, null);
        SetHackedState(false);
        SetVisible(true);
        GetRef<AbstractSavingManager>().GetSavingData<ContainerSavingData>().SaveContainerState(this);
    }

#if UNITY_EDITOR
    public virtual void Configure() { }
#endif

    public virtual void ReceiveResource(ResourceType resourceType, float count, bool reduceInCollectingResource = true)
    {
        var resource = collectedList.Find(a => a.resourceType == resourceType);

        if (resource == null)
        {
            resource = new CollectablesItemCount { resourceType = resourceType, count = count };
            collectedList.Add(resource);
        }
        else
        {
            resource.count += count; /*print(resource.count);*/
        }

        object[] arr = new object[2];
        arr[0] = new CollectablesItemCount { resourceType = resourceType, count = resource.count };
        arr[1] = this;
        OnObjectObservableChanged?.Invoke(arr);
    }

    public virtual float GetAlreadyCollectedResource(ResourceType type)
    {
        var v = collectedList.Find(a => a.resourceType == type);
        if (v == null) return 0;
        return v.count;
    }

    public List<CollectablesItemCount> GetPrices(bool isFullPricesList = false)
    {
        List<CollectablesItemCount> pricesList = new List<CollectablesItemCount>();

        foreach (var item in SOData.itemsList)
        {
            CollectablesItemCount collectables = new CollectablesItemCount();
            collectables.resourceType = item.collectablesItemCount.resourceType;
            collectables.count = item.collectablesItemCount.count;
            pricesList.Add(collectables);
        }

        if (!isFullPricesList) pricesList = pricesList.FindAll(a => a.resourceType != ResourceType.none && a.count != 0);
        return pricesList;
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
        var type = (ResourceType)data[0];
        object[] arr = new object[2];
        arr[0] = new CollectablesItemCount { resourceType = type, count = GetAlreadyCollectedResource(type) };
        arr[1] = this;
        observer.OnObjectObservableChanged(arr);
    }

    public Transform GetObjectObservableTransform()
    {
        return transform;
    }
}
