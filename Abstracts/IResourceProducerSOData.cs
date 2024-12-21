using System.Collections.Generic;

public interface IResourceProducerSOData : IScriptableObjectData
{
	public List<ProduceResource> GetProduceResourceList();
	public float GetPriority();
}
