using System;
using System.Collections.Generic;
using System.Reflection;

public class UnsubscribingDelegate
{
	private List<Action> unsubscribingList = new List<Action>();
	private Action actions;

	public void AddListener(Action action, bool unsubscribeAfterInvoke = false)
	{
		actions += action;
		if (unsubscribeAfterInvoke) unsubscribingList.Add(action);
	}

	public void RemoveListener(Action action)
	{
		actions -= action;
		unsubscribingList.Remove(action);
	}

	public void Invoke()
	{
		actions?.Invoke();

		foreach (var item in unsubscribingList)
		{
			actions -= item;
		}

		unsubscribingList.Clear();
	}

	public Dictionary<object, MethodInfo> GetListeners()
	{
		Dictionary<object, MethodInfo> pairs = new Dictionary<object, MethodInfo>();

		foreach (var item in actions.GetInvocationList())
		{
			pairs.Add(item.Target, item.Method);
		}

		return pairs;
	}
}
