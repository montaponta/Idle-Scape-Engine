public class GeneralSavingData : AbstractSavingData
{
	public bool isSoundOn = true;
	public bool isMusicOn = true;

	public override void ResetData(int flag = 0) { }

	public override void SaveData(bool collectParams)
	{
		if (savingManager.dontSave) return;
		SaveDataObject();
	}

	protected override void SaveDataObject()
	{
		ES3.Save(ToString(), this);
	}
}
