using System;
using System.Collections.Generic;
using UnityEngine;

public class DataStructures { }

[Serializable]
public class ResourceTypeSpriteData
{
	public ResourceType resourceType;
	public Sprite sprite;
}

[Serializable]
public class ResourceGOPrefab
{
	public ResourceType resourceType;
	public GameObject gameObject;
}

[Serializable]
public class NeedResource : ICraftItemPrices
{
	public CollectablesItemCount collectablesItemCount;
	public int fromLevel;
	public int needResourceLevel;
	[HideInInspector] public Rect rect;

	public NeedResource()
	{
		collectablesItemCount = new CollectablesItemCount();
		rect = new Rect();
	}

	public CollectablesItemCount CollectablesItemCount()
	{
		return collectablesItemCount;
	}

	public int FromLevel()
	{
		return fromLevel;
	}

	public object[] GetOtherParameters()
	{
		return new object[] { needResourceLevel };
	}
}

[Serializable]
public class AdditionalParameters
{
	public string paramName;
	public string value;
}

[Serializable]
public class ProduceResource : ICloneable
{
	public NeedResource needResource;
	public ResourceType produceResourceType;
	public float produceCount;
	public float maxProduceCount;
	public float produceTime;
	public List<AdditionalParameters> additionalParametersList;
	[NonSerialized] public GameObject sourceGO;

	public object Clone()
	{
		var produceResource = new ProduceResource();
		produceResource.needResource = needResource;
		produceResource.produceResourceType = produceResourceType;
		produceResource.produceCount = produceCount;
		produceResource.maxProduceCount = maxProduceCount;
		produceResource.produceTime = produceTime;
		produceResource.additionalParametersList = new List<AdditionalParameters>(additionalParametersList);
		return produceResource;
	}

    public object GetValueByTag(string tag, System.Type returnType)
    {
        var v = additionalParametersList.Find(a => a.paramName == tag);

        if (v != null)
        {
            if (returnType == typeof(float))
            {
                System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");
                return float.Parse(v.value, cultureInfo);
            }

            if (returnType == typeof(string)) return v.value;
        }

        return null;
    }

    public T GetValueByTag<T>(string tag)
    {
        var v = (T)GetValueByTag(tag, typeof(T));
        return v != null ? v : default(T);
    }
}

[Serializable]
public class NeedOtherResource
{
	public int forCraftLevel;
	public ResourceType needResource;
	public int needResourceLevel;
}

[Serializable]
public class CollectablesItemCount
{
	public ResourceType resourceType;
	public float count;
}

[Serializable]
public class CraftParameters
{
	public float coef;
	public List<CoefOverride> coefOverridesList;
	public float craftTime;
	public float levelIncrement;

	public CraftParameters()
	{
		coef = 1;
		craftTime = 1;
		levelIncrement = 0;
	}

	[Serializable]
	public class CoefOverride
	{
		public int forLevel;
		public float coef;
	}
}

public class CraftData
{
	public AbstractCraftItem craftItem;
	public List<AbstractUnit> unitsList = new List<AbstractUnit>();
	public List<AbstractUnit> readyUnitsList = new List<AbstractUnit>();
	public List<CollectablesItemCount> collectables = new List<CollectablesItemCount>();
}

[Serializable]
public class ExchangeParameters
{
	public List<FromType> fromTypes;
	public ResourceType toType;
	public float exchangeTime;

	[Serializable]
	public class FromType
	{
		public ResourceType type;
		public float needCountToUnit;
	}
}

public class CollectData
{
	public Transform target;
	public List<AbstractUnit> unitList = new List<AbstractUnit>();
	public List<CollectablesItemCount> collectables = new List<CollectablesItemCount>();
}

[Serializable]
public class IDGameObjectData
{
	public string id;
	public GameObject gameObject;
}

[Serializable]
public class NeedInventoryResource : ICraftItemPrices
{
	public int fromLevel;
	public ResourceType inventoryType;
	public CollectablesItemCount collectablesItemCount = new CollectablesItemCount();

	public CollectablesItemCount CollectablesItemCount()
	{
		return collectablesItemCount;
	}

	public int FromLevel()
	{
		return fromLevel;
	}

	public object[] GetOtherParameters()
	{
		return new object[] { inventoryType };
	}
}

[Serializable]
public class GameObjectsMatch
{
	public GameObject go1, go2;
}

[Serializable]
public class ResourceTypeID
{
    public ResourceType type;
    public string id;
}

[Serializable]
public class LootContainerState
{
    public string id;
    public bool isHacked;
    public bool isEnable;
    public bool isVisible;
    public string name;
    public Vector3Serialized pos, rot;
}

public class OpenContainerData
{
    public AbstractContainer container;
    public List<AbstractUnit> unitList = new List<AbstractUnit>();
    public List<AbstractUnit> readyUnitsList = new List<AbstractUnit>();
}

public class UnitData
{
    public bool isEnable;
    public Vector3Serialized pos, rot;
    public string id;
    public string name;
    public bool isVisible;
    public List<CollectablesItemCount> backpackList;
}
