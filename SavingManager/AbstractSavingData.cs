using UnityEngine;

public abstract class AbstractSavingData
{
	protected AbstractSavingManager savingManager;
	private Coroutine coroutine;

	public virtual void Init(AbstractSavingManager savingManager)
	{
		this.savingManager = savingManager;
	}

	public virtual void SaveData(bool collectParams)
	{
		if (savingManager.dontSave) return;
	
		if (!savingManager.GetRef<Main>().debugTools.isDebugActive)
		{
			if (coroutine != null) savingManager.StopCoroutine(coroutine);

			coroutine = savingManager.StartCoroutine(savingManager.GetRef<Main>().ActionCoroutine(() =>
			{
				SaveDataObject();
				coroutine = null;
			}, 0.5f));
		}
		else SaveDataObject();
	}

	public void SaveDataImmediately()
	{
		if (coroutine != null) savingManager.StopCoroutine(coroutine);
		SaveDataObject();
	}

	public virtual void LoadData() { }
	public virtual bool IsDataEmpty() { return true; }
	public abstract void ResetData(int flag = 0);
	protected abstract void SaveDataObject();
}
