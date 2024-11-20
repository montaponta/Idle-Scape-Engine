public abstract class AbstractPanel : MainRefs
{
	public UnsubscribingDelegate OnEnableDelegate = new UnsubscribingDelegate();
	public UnsubscribingDelegate OnDisableDelegate = new UnsubscribingDelegate();
	public UnsubscribingDelegate OnDestroyDelegate = new UnsubscribingDelegate();

	public abstract void Init(object[] arr);
	public virtual void Disable() { }
	public virtual void Destroy() { }

	protected virtual void OnEnable()
	{
		OnEnableDelegate.Invoke();
	}

	protected virtual void OnDisable()
	{
		OnDisableDelegate.Invoke();
	}

	protected virtual void OnDestroy()
	{
		OnDestroyDelegate.Invoke();
	}
}
