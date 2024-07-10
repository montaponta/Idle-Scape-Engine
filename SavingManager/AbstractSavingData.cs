using UnityEngine;

public abstract class AbstractSavingData
{
    protected AbstractSavingManager savingManager;
    private Coroutine coroutine;

    public virtual void Init(AbstractSavingManager savingManager)
    {
        this.savingManager = savingManager;
    }

    public virtual void SaveData(bool collectParams, bool isSave = true)
    {
        if (savingManager.dontSave) return;

        if (!savingManager.GetRef<Main>().debugTools.isDebugActive)
        {
            if (coroutine != null) savingManager.StopCoroutine(coroutine);

            coroutine = savingManager.GetRef<Main>().Invoke(() =>
            {
                SaveDataObject();
                coroutine = null;
            }, 0.5f);
        }
        else if (isSave) SaveDataObject();
    }

    public virtual void SaveDataToPath(bool collectParams, string key)
    {
        SaveData(collectParams, false);
        SaveDataObject(key);
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

    protected virtual void SaveDataObject(string key)
    {
        //Just dublicate it in override method (except ResourceProducerSavingData)
        ES3.Save(ToString(), this, $"SaveFile_{key}.es3");
    }
}
