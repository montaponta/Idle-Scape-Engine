public abstract class AbstractPanel : MainRefs
{
    public UnsubscribingDelegate OnEnableDelegate = new UnsubscribingDelegate();
    public UnsubscribingDelegate OnDisableDelegate = new UnsubscribingDelegate();
    public UnsubscribingDelegate OnDestroyDelegate = new UnsubscribingDelegate();

    public abstract void Init(object[] arr);

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
