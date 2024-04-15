public abstract class AbstractUnitAbility
{
	protected AbilityType abilityType;
	protected AbstractUnit unit;

	public AbstractUnitAbility(AbstractUnit unit)
	{
		this.unit = unit;
		SetAbilityType();
	}

	protected abstract void SetAbilityType();
	public virtual AbilityType GetAbilityType() { return abilityType; }
	public virtual void Update() { }
	public virtual void FixedUpdate() { }
	public virtual void SetAnimationPhase(int value) { }
	public virtual AbstractUnit GetUnit() { return  unit; }
}
