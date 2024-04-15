using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "ResourceProducerData", menuName = "ScriptableObjects/ResourceProducerData")]
public class ResourceProducerSO : ScriptableObject, IScriptableObjectData
{
	public List<ProduceResource> itemsList;
	public List<AdditionalParameters> additionalParamsList = new List<AdditionalParameters>();
	public float recoveryTime;
	public float priority;
	public int collectorsCount;
	public float distanceToStop = 1;

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
        throw new System.NotImplementedException();
    }

    public List<NeedOtherResource> GetNeedOtherResourceList()
    {
        throw new System.NotImplementedException();
    }

    public List<NeedResource> GetNeedResourceList()
	{
		var list = itemsList.Where(a => a.needResource != null)
			.ToDictionary(a => a.needResource).Keys.ToList();
		return list;
	}

    public List<ResourceTypeID> GetNeedResourceTypeIDsList()
    {
        throw new System.NotImplementedException();
    }

    public List<ProduceResource> GetProduceResourceList()
    {
		return itemsList;
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
}
