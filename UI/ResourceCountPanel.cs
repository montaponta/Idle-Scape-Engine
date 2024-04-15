using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCountPanel : MainRefs, IObjectObserver
{
	public ResourceType resourceType, resourceType1;
	public ObservableType observableType;
	public bool hideIfAbsent, updatePosition;
	public TMP_Text countText;
	public Image icon;
	private Dictionary<IObjectObservable, float> observablePairs = new Dictionary<IObjectObservable, float>();
	public string addStringToEnd, addStringToBegining;

	public void Init(IObjectObservable observable)
	{
		SetRefs();
		observablePairs.Add(observable, 0);
		observable.AddObjectObserver(this);
		observable.GetObjectObservableState(this, new object[] { resourceType, resourceType1 });
		icon.sprite = resourceType1 == ResourceType.none ? SharedObjects.GetResourceSprite(resourceType) : SharedObjects.GetResourceSprite(resourceType1);
		Update();
	}

	public void Init(List<IObjectObservable> observables)
	{
		SetRefs();
		observables.ForEach(a => observablePairs.Add(a, 0));
		observables.ForEach(a => a.AddObjectObserver(this));
		observables.ForEach(a => a.GetObjectObservableState(this, new object[] { resourceType, resourceType1 }));
		icon.sprite = resourceType1 == ResourceType.none ? SharedObjects.GetResourceSprite(resourceType) : SharedObjects.GetResourceSprite(resourceType1);
		Update();
	}

	private void Update()
	{
		if (updatePosition) ChangePositionPanel();
	}

	private void ChangePositionPanel()
	{
		var pos = Camera.main.WorldToScreenPoint(observablePairs.First().Key.GetObjectObservableTransform().position);
		transform.position = pos + Vector3.up * 100 * GetRef<AbstractUI>().transform.localScale.y;
	}

	public void OnObjectObservableChanged(object[] data)
	{
		var collectables = (CollectablesItemCount)data[0];
		var observable = (IObjectObservable)data[1];
		if (resourceType1 == ResourceType.none && collectables.resourceType != resourceType) return;
		if (resourceType1 != ResourceType.none && collectables.resourceType != resourceType1) return;
		float fill = Mathf.Floor(collectables.count);
		observablePairs[observable] = fill;
		var sum = observablePairs.Values.Sum();
		countText.text = $"{addStringToBegining}{sum}{addStringToEnd}";
		if (hideIfAbsent) gameObject.SetActive(fill > 0);
	}

	public void UnsubscribeActions()
	{
		observablePairs.Keys.ToList().ForEach(a => a.RemoveObjectObserver(this));
		observablePairs.Clear();
	}

	private void OnDestroy()
	{
		UnsubscribeActions();
	}
}
