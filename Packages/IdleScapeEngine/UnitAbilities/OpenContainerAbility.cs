public class OpenContainerAbility : AbstractUnitAbility
{
    public AbstractContainer container;

    public OpenContainerAbility(AbstractUnit unit) : base(unit)
    {
    }

    protected override void SetAbilityType()
    {
        abilityType = AbilityType.openContainer;
    }
}
