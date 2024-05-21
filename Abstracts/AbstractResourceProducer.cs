using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractResourceProducer : MainRefs, IIDExtention, ISODataHandler, IObjectObservable
{
	public ResourceProducerSO SOData;
	public CollectAnimationType animationType;
	public Dictionary<ResourceType, float> resourceGiven = new Dictionary<ResourceType, float>();
	public Dictionary<ResourceCollectorAbility, (ResourceType, float)> reservedResourcePairs = new Dictionary<ResourceCollectorAbility, (ResourceType, float)>();
	public bool isEnable;
	public string id;
	public Timer recoverTimer = new Timer(TimerMode.counterFixedUpdate);
	protected (ResourceCollectorAbility resourceCollectorAbility, CollectablesItemCount collectables) magnetizeData;
	[NonSerialized] public List<ResourceCollectorAbility> reservedCollectorsList = new List<ResourceCollectorAbility>();
	public Action<object[]> OnObjectObservableChanged;
    public Coroutine getResourceCoroutine;

    protected override void Start()
	{
		base.Start();
		if (isEnable) SetEnable(isEnable);
	}

	protected virtual void FixedUpdate()
	{
		recoverTimer.TimerUpdate();
		if (magnetizeData.resourceCollectorAbility != null) MagnetizeResource(magnetizeData.resourceCollectorAbility, magnetizeData.collectables);
	}

	public virtual void SetEnable(bool b)
	{
		isEnable = b;
	}

	protected virtual ProduceResource GetCurrentItem(ResourceType requiredResourceType)
	{
		var v = SOData.itemsList.Find(a => a.produceResourceType == requiredResourceType);
		if (v == null) return null;
		ProduceResource produceResource = (ProduceResource)v.Clone();
		return produceResource;
	}

	public virtual bool CanProduce(ResourceType requiredResourceType, ResourceCollectorAbility ability)
	{
		if (!isEnable) return false;
		var remain = GetRemainResourceCount(requiredResourceType);
		if (remain == 0) return false;
		if (reservedCollectorsList.Contains(ability)) return true;
		else if (remain - reservedResourcePairs.Values.Where(a => a.Item1 == requiredResourceType).Sum(a => a.Item2) <= 0) return false;
		else return reservedCollectorsList.Count < SOData.collectorsCount;
	}

	public virtual void GetResource(ResourceType requiredResourceType, float needCount, ResourceCollectorAbility resourceCollectorAbility = null)
	{
		var produceResource = GetCurrentItem(requiredResourceType);
		getResourceCoroutine = StartCoroutine(GetResourceCoroutine(produceResource, needCount, resourceCollectorAbility));
	}

	protected virtual IEnumerator GetResourceCoroutine(ProduceResource produceResource, float needCount, ResourceCollectorAbility resourceCollectorAbility)
	{
		yield return new WaitForSeconds(produceResource.produceTime);
		getResourceCoroutine = null;
		float canGive;
		needCount = Mathf.Clamp(needCount, 0, produceResource.produceCount);
		var type = produceResource.produceResourceType;
		if (!resourceGiven.ContainsKey(type)) resourceGiven.Add(type, 0);

		if (produceResource.maxProduceCount - (resourceGiven[type] + needCount) > 0)
			canGive = needCount;
		else canGive = produceResource.maxProduceCount - resourceGiven[type];

		resourceGiven[type] += canGive;
		var remain = produceResource.maxProduceCount - resourceGiven[type];
		produceResource.sourceGO = gameObject;
		GetResourceProcedure(produceResource, canGive, resourceCollectorAbility);
		resourceCollectorAbility.RecieveResource(canGive, remain, produceResource);

		if (remain <= 0)
		{
			float f = 0;

			foreach (var item in SOData.itemsList)
			{
				f = GetRemainResourceCount(item.produceResourceType);
				if (f > 0) break;
			}

			if (f == 0)
			{
				isEnable = false;
				ResourceIsEmpty();
			}
		}
	}

	public virtual void ResourceIsEmpty()
	{
		gameObject.SetActive(false);
		SetEnable(false);
	}

	protected virtual void GetResourceProcedure(ProduceResource produceResource, float canGive, ResourceCollectorAbility resourceCollectorAbility) { }

	public virtual float GetResourceProduceCount(ResourceType type)
	{
		var v = SOData.itemsList.Find(a => a.produceResourceType == type);
		if (v != null) return v.produceCount;
		return 0;
	}

	public virtual float GetRemainResourceCount(ResourceType type)
	{
		var produceResource = GetCurrentItem(type);
		if (produceResource == null) return 0;
		if (!resourceGiven.ContainsKey(type)) resourceGiven.Add(type, 0);
		return produceResource.maxProduceCount - resourceGiven[type];
	}

	public virtual bool IsResourceProducerEmpty()
	{
		foreach (var item in SOData.itemsList)
		{
			if (GetRemainResourceCount(item.produceResourceType) > 0) return false;
		}

		return true;
	}

	public virtual void StartRecover(float time) { }
	protected virtual void RecoverResource() { }
	public virtual bool IsResourceMagnetic() { return false; }
	public virtual void MagnetizeResource(ResourceCollectorAbility resourceCollectorAbility, CollectablesItemCount collectables)
	{
		if (magnetizeData.resourceCollectorAbility == null)
		{
			magnetizeData.resourceCollectorAbility = resourceCollectorAbility;
			magnetizeData.collectables = collectables;
			return;
		}

		var pos = resourceCollectorAbility.GetUnit().transform.position + Vector3.up;
		transform.position = Vector3.MoveTowards(transform.position, pos, 0.4f);

		if (transform.position == pos)
		{
			GetResource(collectables.resourceType, collectables.count, resourceCollectorAbility);
			magnetizeData.resourceCollectorAbility = null;
			magnetizeData.collectables = null;
		}
	}

	public virtual void AddCollectorToReservedList(ResourceCollectorAbility ability, ResourceType type, float count)
	{
		if (!reservedCollectorsList.Contains(ability)) reservedCollectorsList.Add(ability);
		reservedResourcePairs.Add(ability, (type, count));
	}

	public virtual void RemoveCollectorFromReservedList(ResourceCollectorAbility ability)
	{
		reservedCollectorsList.Remove(ability);
		reservedResourcePairs.Remove(ability);
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

	public IScriptableObjectData GetSOData()
	{
		return SOData;
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
		arr[0] = new CollectablesItemCount { resourceType = type, count = GetRemainResourceCount(type) };
		arr[1] = this;
		observer.OnObjectObservableChanged(arr);
	}

	public Transform GetObjectObservableTransform()
	{
		return transform;
	}
}