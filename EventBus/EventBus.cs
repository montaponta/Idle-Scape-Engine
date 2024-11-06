using System;
using System.Collections.Generic;
using System.Linq;

public static class EventBus
{
	private static Dictionary<Type, List<(int priority, object Target, Action<IBusEvent> action)>> events = new();

	public static void Subscribe<T>(Action<T> callback, int priority = 0) where T : IBusEvent
	{
		var type = typeof(T);
		if (!events.ContainsKey(type)) events.Add(type, new());
		events[type].Add((priority, callback.Target, Convert<T>(callback)));
		events[type] = events[type].OrderByDescending(a => a.priority).ToList();
	}

	public static Action<IBusEvent> Convert<T>(Action<T> myActionT)
	{
		if (myActionT == null) return null;
		return new Action<IBusEvent>(a => myActionT((T)a));
	}

	public static void Unsubscribe<T>(Action<T> callback) where T : IBusEvent
	{
		var type = typeof(T);
		if (events.ContainsKey(type))
		{
			var list = events[type];
			var item = list.Find(a => a.Target == callback.Target);
			events[type].Remove(item);
		}
	}

	public static void Publish<T>(T data) where T : IBusEvent
	{
		var type = typeof(T);
		if (events.ContainsKey(type))
		{
			foreach (var item in events[type])
			{
				item.action?.Invoke(data);
			}
		}
	}
}
