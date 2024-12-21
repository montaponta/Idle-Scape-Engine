using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftItemData", menuName = "ScriptableObjects/CraftItemData")]
public class CraftItemSO : ScriptableObject, ICraftItemSOData
{
	public ResourceType craftItem;
	public List<NeedResource> needResourceList;
	public List<NeedOtherResource> needOtherResourceList;
	public List<NeedInventoryResource> needInventoryResourceList;
    public List<ResourceTypeID> needResourceTypeIDsList;
    public List<AdditionalParameters> additionalParamsList = new List<AdditionalParameters>();
	public CraftParameters craftParams;
	public float distanceToStop = 1;
	public int unitsCount = 10;

	public List<AdditionalParameters> GetAdditionalParameters()
	{
		return additionalParamsList;
	}

	public float GetDistanceToStop()
	{
		return distanceToStop;
	}

	public object GetValueByTag(string tag, System.Type returnType)
	{
		var v = additionalParamsList.Find(a => a.paramName == tag);

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

    public List<ICraftItemPrices> GetIPricesFromNeedResourceList()
	{
		return needResourceList.Cast<ICraftItemPrices>().ToList();
	}

	public List<ICraftItemPrices> GetIPricesFromNeedInventoryResourceList()
	{
		return needInventoryResourceList.Cast<ICraftItemPrices>().ToList();
	}

    public List<NeedResource> GetNeedResourceList()
    {
		return needResourceList;
    }

    public List<NeedOtherResource> GetNeedOtherResourceList()
    {
		return needOtherResourceList;
    }

    public List<NeedInventoryResource> GetNeedInventoryResourceList()
    {
		return needInventoryResourceList;
    }

    public List<ResourceTypeID> GetNeedResourceTypeIDsList()
    {
		return needResourceTypeIDsList;
    }

	public ResourceType GetCraftItemType()
	{
		return craftItem;
	}

	public CraftParameters GetCraftParams()
	{
		return craftParams;
	}

	public int GetUnitsCount()
	{
		return unitsCount;
	}
}
