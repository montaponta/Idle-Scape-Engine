using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCountPanel : AbstractPanel, IObjectObserver
{
	public ResourceType resourceType, resourceType1;
	public ObservableType observableType;
	public bool hideIfAbsent, updatePosition;
	public TMP_Text countText;
	public Image icon;
	protected Dictionary<IObjectObservable, float> observablePairs = new Dictionary<IObjectObservable, float>();
	public string addStringToEnd, addStringToBegining;
	protected SharedObjects sharedObjects => GetRef<SharedObjects>();

	public virtual void Init(IObjectObservable observable)
	{
		observablePairs.Add(observable, 0);
		observable.AddObjectObserver(this);
		observable.GetObjectObservableState(this, new object[] { resourceType, resourceType1 });
		icon.sprite = resourceType1 == ResourceType.none ? sharedObjects.GetResourceSprite(resourceType) : sharedObjects.GetResourceSprite(resourceType1);
		Update();
	}

	public virtual void Init(List<IObjectObservable> observables)
	{
		observables.ForEach(a => observablePairs.Add(a, 0));
		observables.ForEach(a => a.AddObjectObserver(this));
		observables.ForEach(a => a.GetObjectObservableState(this, new object[] { resourceType, resourceType1 }));
		icon.sprite = resourceType1 == ResourceType.none ? sharedObjects.GetResourceSprite(resourceType) : sharedObjects.GetResourceSprite(resourceType1);
		Update();
	}

	public override void Init(object[] arr)
	{
		if (arr.Length < 2)
		{
			var observable = (IObjectObservable)arr[0];
			Init(observable);
		}
		else
		{
			var observables = arr.Cast<IObjectObservable>().ToList();
			Init(observables);
		}
	}

	protected virtual void Update()
	{
		if (updatePosition) ChangePositionPanel();
	}

	protected virtual void ChangePositionPanel()
	{
		var pos = Camera.main.WorldToScreenPoint(observablePairs.First().Key.GetObjectObservableTransform().position);
		transform.position = pos + Vector3.up * 100 * GetRef<AbstractUI>().transform.localScale.y;
	}

	public virtual void OnObjectObservableChanged(object[] data)
	{
		var collectables = (CollectablesItemCount)data[0];
		var observable = (IObjectObservable)data[1];
		if (resourceType1 == ResourceType.none && collectables.resourceType != resourceType) return;
		if (resourceType1 != ResourceType.none && collectables.resourceType != resourceType1) return;
		float fill = collectables.count;
		observablePairs[observable] = fill;
		var sum = observablePairs.Values.Sum();
		countText.text = $"{addStringToBegining}{sum}{addStringToEnd}";
		if (hideIfAbsent) gameObject.SetActive(fill > 0);
	}

	public virtual void UnsubscribeActions()
	{
		observablePairs.Keys.ToList().ForEach(a => a.RemoveObjectObserver(this));
		observablePairs.Clear();
	}

	protected override void OnDestroy()
	{
		UnsubscribeActions();
	}
}
