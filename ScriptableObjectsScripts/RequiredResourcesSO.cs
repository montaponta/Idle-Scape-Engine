using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RequiredResourceData", menuName = "ScriptableObjects/RequiredResourceData")]
public class RequiredResourcesSO : ScriptableObject, IRequiredResourcesSOData
{
    public List<NeedResource> itemsList;
    public List<NeedOtherResource> needOtherResourceList;
    public List<NeedInventoryResource> needInventoryList;
    public List<ResourceTypeID> needResourceTypeIDsList;
    public List<AdditionalParameters> additionalParamsList = new List<AdditionalParameters>();
    public float distanceToStop = 1;
    public int unitsCount = 10;

    public void SetDefaults()
    {
        itemsList = new List<NeedResource>();
    }

    public List<NeedResource> GetNeedResourceList()
    {
        return itemsList;
    }

    public float GetStopingDistance()
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

    public List<NeedOtherResource> GetNeedOtherResourceList()
    {
        return needOtherResourceList;
    }

    public void SetNeedResourceList(List<NeedResource> list)
    {
        itemsList = new List<NeedResource>(list);
    }

    public List<AdditionalParameters> GetAdditionalParameters()
    {
        return additionalParamsList;
    }

    public float GetDistanceToStop()
    {
        return distanceToStop;
    }

    public List<NeedInventoryResource> GetNeedInventoryResourceList()
    {
        return needInventoryList;
    }

    public List<ResourceTypeID> GetNeedResourceTypeIDsList()
    {
        return needResourceTypeIDsList;
    }

	public int GetUnitsCount()
	{
		return unitsCount;
	}
}

