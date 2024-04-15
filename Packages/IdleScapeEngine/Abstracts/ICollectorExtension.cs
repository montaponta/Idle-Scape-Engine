public interface ICollectorExtension
{
    public void SetCollectingAnimation(CollectAnimationType openTypeAnim);
    public void SetBusy(bool b);
    public void StartStopOpeningLootContainer(AbstractContainer lootContainer, bool b);
    public void StopCollectingAnimations();
    public void SetColectorActionType(UnitActionType type);
}
