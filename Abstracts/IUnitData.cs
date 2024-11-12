public interface IUnitData
{
	public int GetHealth();
	public float GetMoveSpeed();
	public object GetValueByTag(string tag, System.Type returnType);
	public T GetValueByTag<T>(string tag);
}
