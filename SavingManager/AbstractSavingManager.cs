using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class AbstractSavingManager : MainRefs
{
	public static AbstractSavingManager shared;
	public bool dontSave;
	protected DateTime lastSaveDate;
	[HideInInspector] public bool isSavingDataLoadComplete;
	public Timer playtimeTimer = new Timer(TimerMode.counterFixedUpdate, false, true);
	public Dictionary<SavingDataType, AbstractSavingData> savingDataPairs = new Dictionary<SavingDataType, AbstractSavingData>();
	protected int sceneIndex;

	public static bool DontSave { get => shared.dontSave; set => shared.dontSave = value; }
	public static bool IsSavingDataLoadComplete => shared.isSavingDataLoadComplete;
	public static Timer PlaytimeTimer => shared.playtimeTimer;
	public string customSavingDataPath => GetCustomSavingDataPath();

	protected Main main => GetRef<Main>();


	protected virtual void Awake()
	{
		shared = this;


		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<AbstractSavingManager>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	public virtual void Init()
	{
		AddSavingDatasToList();
	}

	public virtual AbstractSavingData GetSavingData(SavingDataType type) { return savingDataPairs[type]; }

	public virtual T GetSavingData<T>(SavingDataType type) where T : AbstractSavingData
	{
		return (T)savingDataPairs[type];
	}

	public virtual AbstractSavingData GetSavingData<T>()
	{
		var savingData = savingDataPairs.Where(a => a.Value is T).FirstOrDefault().Value;
		return savingData;
	}

	protected virtual void LoadES3Data<T>(T data, string path = "") where T : AbstractSavingData
	{
		if (path == "")
		{
			if (ES3.KeyExists(data.ToString()))
				ES3.LoadInto(data.ToString(), data);
		}
		else
		{
			if (ES3.KeyExists(data.ToString(), path))
				ES3.LoadInto(data.ToString(), path, data);
		}

		data.LoadData();
	}

	protected abstract void AddSavingDatasToList();
	public virtual void LoadData() { isSavingDataLoadComplete = true; }
	public virtual void SaveData(bool collectParams = true) { lastSaveDate = DateTime.UtcNow; }

	public virtual void SaveAllData()
	{
		var b = dontSave;
		dontSave = false;
		ES3.Save("sceneIndex", sceneIndex);
		if (!savingDataPairs.Any()) AddSavingDatasToList();

		foreach (var item in savingDataPairs.Values)
		{
			item.SaveData(true);
		}

		dontSave = b;
		print("Saved");
	}

	public virtual void SaveDataToPath(string key)
	{
		foreach (var item in savingDataPairs.Values)
		{
			item.SaveDataToPath(true, key);
		}

		ES3.Save("sceneIndex", sceneIndex, $"SaveFile_{key}.es3");
	}

	public void DestroyData()
	{
		ES3.DeleteFile();

		for (int i = 0; i < 100; i++)
		{
			ES3.DeleteFile($"ResourceProducers_{i}.es3");
		}


#if UNITY_EDITOR
		if (EditorApplication.isPlaying) dontSave = true;
#endif
		print("Saved Data Destroyed");
	}

	public void ResetSavingData()
	{
		foreach (var item in savingDataPairs.Values)
		{
			item.ResetData();
		}
		dontSave = true;
	}

	public virtual void ReturnResourcesFromBackpackes(List<CollectablesItemCount> list)
	{
		foreach (var item in list)
		{
			var storages = GetRef<CollectAndCraftFunctions>().GetStoragesWithThisResource(item.resourceType);
			var remain = item.count;

			foreach (var storage in storages)
			{
				var freeSpace = storage.GetStorageFreeSpace(item.resourceType, StorageReserveType.none);
				var canTake = Mathf.Clamp(remain, 0, freeSpace);
				storage.ReceiveResource(item.resourceType, canTake);
				remain -= canTake;
				if (Mathf.Approximately(remain, 0)) remain = 0;
				if (remain == 0) break;
			}
		}
	}

	public virtual bool IsAllAdsDisabled()
	{
		//return shopSavingData.isAllAdsDisabled;
		return false;
	}

	public static string GetCustomSavingDataPath()
	{
		return "CustomSavingData.es3";
	}

	protected virtual void OnApplicationFocus(bool focus)
	{
		if (SceneManager.GetActiveScene().name == "Loading Scene") return;
		//if (!focus) SaveData();
	}

	protected virtual void OnApplicationQuit()
	{
		if (SceneManager.GetActiveScene().name == "Loading Scene") return;
		if ((DateTime.UtcNow - lastSaveDate).TotalSeconds > 0.5f) SaveData();
		//GetRef<QuestsManager>().ApplicationQuit();
	}
}
