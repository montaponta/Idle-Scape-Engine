public interface IResourceReciever
{
    public void ReceiveResource(ResourceType resourceType, float count, bool reduceInCollectingResource = true);
}
