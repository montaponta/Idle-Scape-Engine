using System.Collections.Generic;

public interface IScriptableObjectData
{
	public float GetDistanceToStop();
	public object GetValueByTag(string tag, System.Type returnType);
	public List<AdditionalParameters> GetAdditionalParameters();
    public List<NeedResource> GetNeedResourceList();
    public List<NeedOtherResource> GetNeedOtherResourceList();
    public List<NeedInventoryResource> GetNeedInventoryResourceList();
    public List<ResourceTypeID> GetNeedResourceTypeIDsList();
    public List<ProduceResource> GetProduceResourceList();
}
