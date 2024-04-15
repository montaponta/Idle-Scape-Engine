using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedObjects : MainRefs
{
	public static SharedObjects shared;
	public GameObject openResourcePanelPrefab, resourceProducerPanel, craftBarPrefab,
		resourceCountCraftPanel, resourceCountPanelPrefab, chooseRecycleResourceButtonPrefab,
		resourceCountPanelRecycleItemPrefab, panelContainerPrefab, chooseCraftItemButtonPrefab, 
		roundBarPrefab, resourceCountPanelCraftBtnPrefab, openContainerResourcePanelPrefab;
	public List<ResourceTypeSpriteData> resourceTypeSpriteDatasList;
	public List<ResourceGOPrefab> resourceGOPrefabsList;
	public List<IDGameObjectData> iDGameObjectDatasList;

	[Header("Префабы контейнеров")]
    public GameObject showHideContainer;

    private void Awake()
	{
		shared = this;
		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<SharedObjects>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	public Sprite GetResourceSprite(ResourceType type)
	{
		var v = resourceTypeSpriteDatasList.Find(a => a.resourceType == type);

		if (v == null)
		{
			Debug.LogError($"No icon for: {type}");
			return null;
		}

		return v.sprite;
	}

	public GameObject GetLootPrefab(ResourceType type)
	{
		return resourceGOPrefabsList.Find(a => a.resourceType == type).gameObject;
	}

	public IDGameObjectData GetIDGameObjectData(string id)
	{
		var v = iDGameObjectDatasList.Find(a => a.id == id);

		if (v == null)
		{
			Debug.LogError($"No object for: {id}");
			return null;
		}

		return v;
	}

	public string GetRandomIDBasedOnDefaultID(string id)
	{
		var arr = iDGameObjectDatasList.Where(a => a.id.Contains(id)).Select(a => a.id).ToArray();

		if (!arr.Any())
		{
			Debug.LogError($"No object for: {id}");
			return null;
		}

		var index = UnityEngine.Random.Range(0, arr.Count());
		return arr[index];
	}
}
