using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedObjects : MainRefs
{
    public static SharedObjects shared;
    public List<ResourceTypeSpriteData> resourceTypeSpriteDatasList;
    public List<IDGameObjectData> iDGameObjectDatasList;

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

    public GameObject GetIDGameObjectData(string id)
    {
        var v = iDGameObjectDatasList.Find(a => a.id == id);

        if (v == null)
        {
            Debug.LogError($"No object for: {id}");
            return null;
        }

        return v.gameObject;
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

    public T InstantiateUIElement<T>(string id, Transform parent)
    {
        var go = Instantiate(GetIDGameObjectData(id), parent);
        return go.GetComponent<T>();
    }

    public ResourceCountPanel InstantiateResourceCountPanel(string id, Transform parent)
    {
        var go = Instantiate(GetIDGameObjectData(id).gameObject, parent);
        return go.GetComponent<ResourceCountPanel>();
    }

    public ProgressBarUI InstantiateProgressBar(string id, Transform parent)
    {
        var go = Instantiate(GetIDGameObjectData(id).gameObject, parent);
        return go.GetComponent<ProgressBarUI>();
    }

    public GameObject GetLootPrefab(ResourceType resourceType)
    {
        return GetIDGameObjectData($"{resourceType}_Loot");
    }
}
