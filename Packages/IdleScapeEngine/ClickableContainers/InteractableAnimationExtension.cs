using UnityEngine;

public class InteractableAnimationExtension : MonoBehaviour
{
	public ResourceAnimationType resourceAnimationType;
	public Vector3 resourceAnimationValue;
	public InteractStrategy interactStrategy;

	public bool CanPlayAnimation()
	{
		var container = GetComponent<AbstractContainer>();

		if (container)
		{
			if (!container.IsHacked() && interactStrategy == InteractStrategy.interactableBeforeEnable) return true;
			if (container.IsHacked() && interactStrategy == InteractStrategy.interactableAfterEnable) return true;
		}

		var producer = GetComponent<AbstractResourceProducer>();

		if (producer)
		{
			if (!producer.isEnable && interactStrategy == InteractStrategy.interactableBeforeEnable) return true;
			if (producer.isEnable && interactStrategy == InteractStrategy.interactableAfterEnable) return true;
		}

		if (interactStrategy == InteractStrategy.interactableAlways) return true;
		return false;
	}
}
