public struct StorageChangedResources: IBusEvent
{
	public ResourceType type;
	public float count;
	public AbstractStorage storageSender;
}
