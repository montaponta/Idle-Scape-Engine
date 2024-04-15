public class AgentMovementAbility : AbstractUnitAbility
{
	public AgentMovementAbility(AbstractUnit unit) : base(unit) { }

	protected override void SetAbilityType()
	{
		abilityType = AbilityType.movement;
	}

	public override void Update()
	{
		base.Update();
		unit.animator.SetBool("Run", unit.agent.desiredVelocity.magnitude > 0);
	}
}
