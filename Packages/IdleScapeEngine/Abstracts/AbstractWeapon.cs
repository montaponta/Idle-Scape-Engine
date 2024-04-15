using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractWeapon : MainRefs
{
	public WeaponSO SOData;
	public float explosionForce;
	[NonSerialized] public AbstractUnit unit;
	protected Timer timer = new Timer(TimerMode.counterFixedUpdate, false);


	protected override void Start()
	{
		base.Start();
		if (!unit) Init();
	}

	public void Init()
	{
		SetRefs();
		unit = GetComponentInParent<AbstractUnit>();
		SetDefaultParameters();
	}

	protected virtual void FixedUpdate()
	{
		timer.TimerUpdate();
	}

	protected virtual void SetDefaultParameters() { }
	public virtual void Attack(bool b) { }
	public virtual void SetDamage(AbstractUnit toUnit)
	{
		if (toUnit && unit.isApplyHit)
		{
			toUnit.SetDamage(GetDamageCount(), this);
		}
	}

	public virtual void ApplyHitEffects(AbstractUnit toUnit)
	{
		if (!toUnit || !unit.isApplyHit) return;
		if (toUnit.isEnable) toUnit.animator.SetTrigger("Hit");
		//toUnit.GetComponentsInChildren<ParticleSystem>()
		//			.Where(a => a.name.Contains("BloodSplat")).First().Emit(10);

		if (!toUnit.isEnable)
		{
			//var shooterPos = toUnit.transform.InverseTransformPoint(unit.transform.position);
			//var pos = new Vector3(shooterPos.normalized.x, 1, shooterPos.normalized.z);
			//pos = toUnit.transform.TransformPoint(pos);
			//var ragdollHandler = toUnit.GetComponent<RagdollHandler>();

			//if (ragdollHandler)
			//{
			//	foreach (var item in ragdollHandler.rigidbodiesList)
			//	{
			//		item.AddExplosionForce(explosionForce, pos, 5, 1, ForceMode.Acceleration);
			//	}
			//}
		}
	}

	public virtual float GetShootPause()
	{
		return SOData.weaponShootPause;
	}

	public virtual void SetAnimationPhase(int value)
	{
		unit.isApplyHit = value > 0;
	}

	public virtual float GetDamageCount()
	{
		return SOData.damage;
	}

	public virtual float GetAttackDistance() { return SOData.attackDistance; }

	public virtual float WhenYouAttack() { return 100; }
}
