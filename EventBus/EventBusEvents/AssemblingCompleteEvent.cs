public struct AssemblingCompleteEvent : IBusEvent
{
    public AbstractCraftItem craftItem;
    public ResourceType type;
}
