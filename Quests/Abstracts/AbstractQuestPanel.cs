using System.Collections.Generic;
using System.Linq;

public abstract class AbstractQuestPanel : MainRefs, IQuestUnsubscribe
{
	public int priority;
	protected AbstractQuestBlock questBlock;
	protected AbstractQuestBlockPart blockPart;

	public virtual void Init(AbstractQuestBlock questBlock, AbstractQuestBlockPart blockPart, object obj)
	{
		this.questBlock = questBlock;
		this.blockPart = blockPart;
		SortByPriority();
	}

	protected virtual void SortByPriority()
	{
		List<AbstractQuestPanel> list = transform.parent.GetComponentsInChildren<AbstractQuestPanel>().ToList();
		list.Remove(this);
		list = list.OrderByDescending(a => a.priority).ToList();

		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i].priority <= priority)
				transform.SetSiblingIndex(i);
		}
	}

	public virtual void Unsubscribe() { }
}
