using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class IconGrid
{
	private AbstractUI ui;
	public Dictionary<Transform, Transform> iconPairs = new Dictionary<Transform, Transform>();
	private Transform mainFolder;
	private float posYIncrement = 80;

	public IconGrid(AbstractUI ui, Transform mainFolder)
	{
		this.ui = ui;
		this.mainFolder = mainFolder;
	}

	public void Update()
	{
		foreach (var item in iconPairs)
		{
			if (!item.Value) continue;
			UpdatePosition(item);
		}
	}

	private void UpdatePosition(KeyValuePair<Transform, Transform> item)
	{
		var pos = ui.GetRef<Main>().mainCamera.WorldToScreenPoint(item.Key.position);
		pos.y += posYIncrement * ui.transform.localScale.y;
		item.Value.transform.position = pos;
	}

	public T CreateIcon<T>(GameObject iconPrefab, Transform target)
	{
		var go = Object.Instantiate(iconPrefab, mainFolder);
		if (!iconPairs.ContainsKey(target)) iconPairs.Add(target, mainFolder);
		Update();
		return go.GetComponentInChildren<T>();
	}

	public T CreateIcon<T>(GameObject iconPrefab, Transform target, GameObject containerPrefab)
	{
		var go = Object.Instantiate(iconPrefab, mainFolder);

		if (!iconPairs.ContainsKey(target))
		{
			var container = Object.Instantiate(containerPrefab, mainFolder);
			iconPairs.Add(target, container.transform);
		}

		go.transform.SetParent(iconPairs[target]);
		go.transform.localPosition = Vector3.zero;
		Update();
		return go.GetComponentInChildren<T>();
	}

	public void DestroyIcon(Transform target, GameObject iconGO)
	{
		if (iconGO == null) return;
		if (!iconPairs.ContainsKey(target)) return;

		foreach (Transform item in iconPairs[target])
		{
			if (item == iconGO.transform)
			{
				Object.Destroy(item.gameObject);
				break;
			}
		}

		Action action = () => { if (iconPairs[target].childCount == 0) DestroyAllIcons(target); };
		ui.GetRef<Main>().Invoke(action, 0.1f);
	}

	public void DestroyAllIcons(Transform target)
	{
		if (!iconPairs.ContainsKey(target)) return;
		var go = iconPairs[target].gameObject;
		Object.Destroy(go);
		iconPairs.Remove(target);
	}

	public void SetPosYIncrement(float value)
	{
		posYIncrement = value;
	}

	public void RetargetIconContainer(Transform target, Transform newTarget)
	{
		var iconContainer = iconPairs[target];
		iconPairs.Add(newTarget, iconContainer);
		iconPairs.Remove(target);
	}

	public void UntargetIconContainer(Transform target)
	{
		iconPairs.Remove(target);
	}
}
