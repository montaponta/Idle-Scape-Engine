using System.Collections.Generic;

public abstract class AbstractCharacterFactory : MainRefs
{
	public abstract void CreateCharacters(int count, (string unitID, UnitData unitData) data, out List<AbstractUnit> newUnitsList);
}
