public interface ISODataHandler
{
	public T GetSOData<T>() where T : IScriptableObjectData;
	public IScriptableObjectData GetScriptableObjectData();
}
