public class GeneralSavingData : AbstractSavingData
{
    public bool isSoundOn = true;
    public bool isMusicOn = true;

    public override void ResetData(int flag = 0) { }

    public override void SaveData(bool collectParams, bool isSave = true)
    {
        if (savingManager.dontSave) return;
        if (isSave) SaveDataObject();
    }

    protected override void SaveDataObject()
    {
        ES3.Save(ToString(), this);
    }

    protected override void SaveDataObject(string key)
    {
        ES3.Save(ToString(), this, $"SaveFile_{key}.es3");
    }
}
