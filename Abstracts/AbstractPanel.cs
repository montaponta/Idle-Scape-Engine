public abstract class AbstractPanel : MainRefs
{
	public UnsubscribingDelegate OnEnableDelegate = new UnsubscribingDelegate();
	public UnsubscribingDelegate OnDisableDelegate = new UnsubscribingDelegate();
	public UnsubscribingDelegate OnDestroyDelegate = new UnsubscribingDelegate();

	public abstract void Init(object[] arr);
	public virtual void Disable() { }
	public virtual void Destroy() { }

	private void OnEnable()
	{
		OnEnableDelegate.Invoke();
	}

	private void OnDisable()
	{
		OnDisableDelegate.Invoke();
	}

	private void OnDestroy()
	{
		OnDestroyDelegate.Invoke();
	}
}
