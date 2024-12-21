using System.Collections.Generic;

public interface IScriptableObjectData
{
	public float GetDistanceToStop();
	public T GetValueByTag<T>(string tag);
	public List<AdditionalParameters> GetAdditionalParameters();
	public List<NeedResource> GetNeedResourceList();
	public List<NeedOtherResource> GetNeedOtherResourceList();
	public List<NeedInventoryResource> GetNeedInventoryResourceList();
	public List<ResourceTypeID> GetNeedResourceTypeIDsList();
	public int GetUnitsCount();
}
