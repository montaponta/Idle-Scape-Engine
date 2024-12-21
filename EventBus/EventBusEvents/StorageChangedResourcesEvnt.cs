public struct StorageChangedResourcesEvnt: IBusEvent
{
	public ResourceType type;
	public float count;
	public AbstractStorage storageSender;
}
