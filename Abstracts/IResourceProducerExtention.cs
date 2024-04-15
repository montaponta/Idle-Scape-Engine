using System.Collections.Generic;

public interface IResourceProducerExtention
{
	public void SetResourceList(List<CollectablesItemCount> list);
	public List<CollectablesItemCount> GetResourceList();
	public ResourceProducerType GetProducerType();
}
