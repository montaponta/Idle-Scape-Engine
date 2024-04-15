using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractCraftItem : MainRefs, IIDExtention, ISODataHandler, IObjectObservable, IResourceReciever
{
    public CraftItemSO SOData;
    public int level;
    public string id;
    private List<CollectablesItemCount> collectedList = new List<CollectablesItemCount>();
    public Action OnAssemblingStart, OnAssemblingComplete;
    public Action<AbstractCraftItem> OnAssemblingStartSendItem, OnAssemblingCompleteSendItem;
    public Action<ResourceType, float> OnResourceCountChanged, OnReceiveResource;
    protected Dictionary<ResourceType, float> inProcessCollectingResourcePair = new Dictionary<ResourceType, float>();
    [HideInInspector] public bool isInImproveProgress;
    public Action<AbstractCraftItem> OnCraftItemClick;
    [HideInInspector] public Timer craftTimer = new Timer(TimerMode.counterFixedUpdate, false);
    private GameObject ghostItem;
    public UnsubscribingDelegate OnAssemblingCompleteUnsubscribe = new UnsubscribingDelegate();
    public Action<object[]> OnObjectObservableChanged;

    protected override void Start()
    {
        SetRefs();
        SetItemLevel(level);
        craftTimer.OnTimerReached = AssemblingComplete;
    }

    protected virtual void FixedUpdate()
    {
        craftTimer.TimerUpdate();
    }

    protected void ImproveLevel()
    {
        level++;
        SetItemLevel(level);
    }

    protected virtual void SetItemLevel(int level)
    {
        foreach (Transform item in transform)
        {
            if (!item.name.Contains("Level")) continue;
            item.gameObject.SetActive(false);
        }

        Transform tr = transform.Find("Level " + level.ToString());
        if (tr) tr.gameObject.SetActive(true);
    }

    public virtual bool IsEnable()
    {
        if (level > 0) return true;
        return false;
    }

    public bool IsIStorageItem()
    {
        return this is IToolItem;
    }

    public void CreateGhost(GameObject prefab)
    {
        if (!prefab || level > 0) return;
        ghostItem = Instantiate(prefab);
        ghostItem.transform.position = transform.position;
    }

    public virtual void ReceiveResource(ResourceType resourceType, float count, bool reduceInCollectingResource = true)
    {
        if (count > 0) Destroy(ghostItem);
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

        if (reduceInCollectingResource && inProcessCollectingResourcePair.ContainsKey(resourceType))
        {
            inProcessCollectingResourcePair[resourceType] -= count;
            if (inProcessCollectingResourcePair[resourceType] < 0)
                inProcessCollectingResourcePair[resourceType] = 0;
        }

        OnReceiveResourceProcedure(resourceType, count);
        OnReceiveResource?.Invoke(resourceType, count);
        OnResourceCountChanged?.Invoke(resource.resourceType, resource.count);
    }

    public void StartAssembling(float time, int count)
    {
        isInImproveProgress = true;
        craftTimer.StartTimer(time, count);
        OnAssemblingStartProcedure();
        OnAssemblingStart?.Invoke();
        OnAssemblingStartSendItem?.Invoke(this);
    }

    public void ChangeAssemblingTime(float time)
    {
        craftTimer.StartTimer(time);
    }

    public void IncreaseAssemblingCount()
    {
        craftTimer.count++;
    }

    private void AssemblingComplete()
    {
        ImproveLevel();
        isInImproveProgress = false;
        ClearInventoryRequaredResources();
        OnAssemblingCompleteProcedure();
        OnAssemblingComplete?.Invoke();
        OnAssemblingCompleteSendItem?.Invoke(this);
        OnAssemblingCompleteUnsubscribe.Invoke();
    }

    protected virtual void OnAssemblingStartProcedure() { }

    protected virtual void OnAssemblingCompleteProcedure()
    {
        if (collectedList.Count == 0) return;
        var v = collectedList[0].resourceType;
        collectedList.Clear();
        OnResourceCountChanged?.Invoke(v, 0);
    }

    private void ClearInventoryRequaredResources()
    {
        var prices = GetCraftPrices(level, SOData.GetIPricesFromNeedInventoryResourceList(), true);
        var isd = GetRef<AbstractSavingManager>().GetSavingData<InventorySavingData>();

        foreach (var pr in prices)
        {
            isd.RemoveInventory((ResourceType)pr.Item2.GetOtherParameters()[0], pr.Item1);
        }
    }

    protected virtual void OnReceiveResourceProcedure(ResourceType resourceType, float count)
    {
        object[] arr = new object[2];
        arr[0] = new CollectablesItemCount { resourceType = resourceType, count = GetAlreadyCollectedResource(resourceType) };
        arr[1] = this;
        OnObjectObservableChanged?.Invoke(arr);
    }

    public float GetInProcessCollectingResourceCount(ResourceType resourceType)
    {
        if (inProcessCollectingResourcePair.ContainsKey(resourceType))
        {
            return inProcessCollectingResourcePair[resourceType];
        }

        return 0;
    }

    public void AddInProcessCollectingResourceCount(ResourceType resourceType, float count)
    {
        if (inProcessCollectingResourcePair.ContainsKey(resourceType))
        {
            inProcessCollectingResourcePair[resourceType] += count;
        }
        else
        {
            inProcessCollectingResourcePair.Add(resourceType, count);
        }
    }

    public virtual void RemoveInProcessCollectingResourceCount(ResourceType resourceType, float count)
    {
        if (!inProcessCollectingResourcePair.ContainsKey(resourceType)) return;
        inProcessCollectingResourcePair[resourceType] -= count;
        if (inProcessCollectingResourcePair[resourceType] < 0) inProcessCollectingResourcePair[resourceType] = 0;
    }

    public virtual float GetAlreadyCollectedResource(ResourceType type)
    {
        var v = collectedList.Find(a => a.resourceType == type);
        if (v == null) return 0;
        return v.count;
    }

    public virtual void OnItemClick()
    {
        OnCraftItemClick?.Invoke(this);
    }

    public virtual void CollectorReachedItem(AbstractUnit unit) { }

    public virtual void SpendResource(ResourceType type, float count)
    {
        var v = collectedList.Find(a => a.resourceType == type);
        //if (v == null || v.count < count) Debug.LogError("Storage resource count < requared count");
        if (v == null) return;
        v.count -= count;
        if (Mathf.Approximately(v.count, 0)) v.count = 0;
        if (v.count < 0) v.count = 0;
        OnResourceCountChanged?.Invoke(v.resourceType, v.count);
    }

    public virtual List<CollectablesItemCount> GetCollectedList()
    {
        return collectedList;
    }

    public void SetCollectedList(List<CollectablesItemCount> list)
    {
        collectedList = list;
    }

    public int GetLevelsCount()
    {
        int lvl = 0;

        foreach (Transform item in transform)
        {
            if (!item.name.Contains("Level")) continue;
            var arr = item.name.Split(new char[] { ' ' });
            lvl = int.Parse(arr[1]);
        }

        return lvl;
    }

    public Transform GetCurrentLevelTransform()
    {
        foreach (Transform item in transform)
        {
            if (!item.name.Contains("Level")) continue;
            var arr = item.name.Split(new char[] { ' ' });
            var lvl = int.Parse(arr[1]);
            if (lvl == level) return item;
        }

        return null;
    }

    public void SetID(string id)
    {
        this.id = id;
    }

    public string GetID()
    {
        return id;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public UnityEngine.Object GetObject()
    {
        return this;
    }

    public IScriptableObjectData GetSOData()
    {
        return SOData;
    }

    public virtual List<(CollectablesItemCount, ICraftItemPrices)> GetCraftPrices(int forLevel, List<ICraftItemPrices> list, bool isFullPricesList = false)
    {
        List<(CollectablesItemCount, ICraftItemPrices)> pricesList = new List<(CollectablesItemCount, ICraftItemPrices)>();
        float coef = 1;

        foreach (var item in list)
        {
            var basicPrice = item.CollectablesItemCount().count;
            var price = forLevel > item.FromLevel() ? basicPrice : 0;

            for (int i = 0; i < forLevel; i++)
            {
                if (i > 0) coef = SOData.craftParams.coef;

                if (SOData.craftParams.coefOverridesList.Any())
                {
                    var overrideCoef = SOData.craftParams.coefOverridesList.Find(a => a.forLevel == i + 1);
                    if (overrideCoef != null) coef = overrideCoef.coef;
                }

                price *= coef;
            }

            price *= GetRef<CollectAndCraftFunctions>().priceMultiplier;
            price = Mathf.Ceil(price);

            pricesList.Add((new CollectablesItemCount
            {
                resourceType = item.CollectablesItemCount().resourceType,
                count = price,
            }, item));
        }

        if (!isFullPricesList) pricesList = pricesList.FindAll(a => a.Item1.resourceType != ResourceType.none && a.Item1.count != 0);
        return pricesList;
    }

    public virtual List<CollectablesItemCount> GetCraftPrices1(int forLevel, List<ICraftItemPrices> list, bool isFullPricesList = false)
    {
        var prices = GetCraftPrices(forLevel, list, isFullPricesList).Select(a => a.Item1).ToList();
        return prices;
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