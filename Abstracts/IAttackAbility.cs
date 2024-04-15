using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackAbility
{
    public AbstractUnit GetUnitToAttack();
	public bool CheckUnitIsEnemy(AbstractUnit unit);
}
