using UnityEngine;

public abstract class AbstractInteractObject : MainRefs
{
	public int priority;

	public GameObject GetGameObject()
	{
		return gameObject;
	}

	public int GetPriority()
	{
		return priority;
	}

	public abstract void Interact(AbstractUnit unit);
	public virtual bool IsInteractable() { return enabled; }
	public virtual void FinishInteraction() { }
	public virtual (bool checkCondition, bool checkOtherCondition, bool checkInventoryCondition) GetCheckConditions() { return (false, true, true); }
	public virtual string GetSpecialInteractionIcon() { return null; }
}

public enum InteractStrategy
{
	none, interactableAfterEnable, interactableBeforeEnable, interactableAlways
}
