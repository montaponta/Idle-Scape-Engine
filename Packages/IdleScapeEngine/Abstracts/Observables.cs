using UnityEngine;

public class Observables { }

public interface IObjectObservable
{
	void AddObjectObserver(IObjectObserver observer);
	void RemoveObjectObserver(IObjectObserver observer);
	void GetObjectObservableState(IObjectObserver observer, object[] data);
	Transform GetObjectObservableTransform();
}

public interface IObjectObserver
{
	public void OnObjectObservableChanged(object[] data);
}

public enum ObservableType
{
	none, storage, inventory, resourceProducer
}