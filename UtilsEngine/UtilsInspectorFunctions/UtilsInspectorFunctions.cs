using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using I2.Loc;
using UnityEditor;

public class UtilsInspectorFunctions : MonoBehaviour
{
	public Terrain terrain;
	public Transform treesFolder;
	public List<GameObject> treePrefabsList;
	public List<Transform> collidersFoldersList;

	public void GetTreesFromTerrain()
	{
		DestroyTrees();
		var treeInstancesList = terrain.terrainData.treeInstances;
		int index = 0;

		foreach (var item in treeInstancesList)
		{
			var pos = item.position;
			pos.x *= terrain.terrainData.size.x;
			pos.y *= terrain.terrainData.size.y;
			pos.z *= terrain.terrainData.size.z;
			pos.x += terrain.transform.position.x;
			pos.y += terrain.transform.position.y;
			pos.z += terrain.transform.position.z;
			var rotY = item.rotation * Mathf.Rad2Deg;

			Quaternion quaternion = Quaternion.Euler(0, rotY, 0);
			GameObject go = Instantiate(treePrefabsList[item.prototypeIndex], pos, Quaternion.identity, treesFolder);
			go.transform.localScale = new Vector3(item.heightScale, item.heightScale, item.heightScale);
			go.transform.GetChild(0).localRotation = quaternion;
			go.name = treePrefabsList[item.prototypeIndex].name + "_" + index;
			index++;
		}

		terrain.drawTreesAndFoliage = false;
	}

	public void DestroyTrees()
	{
		List<GameObject> list = new List<GameObject>();

		foreach (Transform item in treesFolder)
		{
			list.Add(item.gameObject);
		}

		foreach (var item in list)
		{
			DestroyImmediate(item);
		}

		terrain.drawTreesAndFoliage = true;
	}

	//public void AddNameParameterToAllSOAdditionalParams()
	//{
	//	var arr = Resources.FindObjectsOfTypeAll<RequiredResourcesSO>();

	//	foreach (var item in arr)
	//	{
	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Name")).Any())
	//		{
	//			print(item.name);
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Name", value = "Write Name In SO" });
	//		}

	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Description")).Any())
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Description", value = "Write Description In SO" });
	//		item.SetDirty();
	//	}

	//	var arr1 = Resources.FindObjectsOfTypeAll<ResourceProducerSO>();

	//	foreach (var item in arr1)
	//	{
	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Name")).Any())
	//		{
	//			print(item.name);
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Name", value = "Write Name In SO" });
	//		}

	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Description")).Any())
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Description", value = "Write Description In SO" });
	//		item.SetDirty();
	//	}

	//	var arr2 = Resources.FindObjectsOfTypeAll<StorageSO>();

	//	foreach (var item in arr2)
	//	{
	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Name")).Any())
	//		{
	//			print(item.name);
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Name", value = "Write Name In SO" });
	//		}

	//		if (!item.additionalParamsList.Where(a => a.paramName.Contains("Description")).Any())
	//			item.additionalParamsList.Add(new AdditionalParameters { paramName = "Description", value = "Write Description In SO" });
	//		item.SetDirty();
	//	}
	//}

//	public void AddNameParametersToI2Asset(bool overrideStrings = true)
//	{
//		List<IScriptableObjectData> list = new List<IScriptableObjectData>();
//		var arr = Resources.FindObjectsOfTypeAll<RequiredResourcesSO>();
//		var arr1 = Resources.FindObjectsOfTypeAll<ResourceProducerSO>();
//		var arr2 = Resources.FindObjectsOfTypeAll<StorageSO>();
//		foreach (var item in arr) list.Add(item);
//		foreach (var item in arr1) list.Add(item);
//		foreach (var item in arr2) list.Add(item);

//		foreach (var item in list)
//		{
//			var v = item.GetAdditionalParameters().Find(a => a.paramName.Contains("Name"));
//			var paramName = "";

//			if (v != null)
//			{
//				if (v.value == "Write Name In SO") continue;
//				paramName = v.value;
//				AddTagStrToI2Asset(v.value + v.paramName, v.value, overrideStrings);
//			}

//			v = item.GetAdditionalParameters().Find(a => a.paramName.Contains("Description"));

//			if (v != null)
//			{
//				if (v.value == "Write Description In SO") continue;
//				AddTagStrToI2Asset(paramName + v.paramName, v.value, overrideStrings);
//			}
//		}

//		LocalizationManager.Sources[0].Editor_SetDirty();
//#if UNITY_EDITOR
//		AssetDatabase.SaveAssets();
//#endif
//	}

	private void AddTagStrToI2Asset(string tag, string str, bool overrideStrings)
	{
		print($"{tag}, {str}");
		LocalizationManager.UpdateSources();
		var Source = LocalizationManager.Sources[0];
		var list = LocalizationManager.GetTermsList();

		if (list.Contains(tag))
		{
			if (overrideStrings) Source.GetTermData(tag).Languages[0] = str;
		}
		else
		{
			var data = Source.AddTerm(tag, eTermType.Text, false);
			data.Languages[0] = str;
		}
	}

//	public void AddShopKeysToI2Asset(bool overrideStrings = true)
//	{
//		var shopIDs = FindObjectOfType<ShopIDs>();

//		foreach (var item in shopIDs.shopIDsList)
//		{
//			//if (item.type != ShopProductType.airdrops && item.type != ShopProductType.skins) continue;
//			//AddTagStrToI2Asset(item.name, item.name, overrideStrings);
//		}

//		LocalizationManager.Sources[0].Editor_SetDirty();
//#if UNITY_EDITOR
//		AssetDatabase.SaveAssets();
//#endif
//	}

	public void TurnOffCollidersInFolders()
	{
		foreach (var item in collidersFoldersList)
		{
			var arr = item.GetComponentsInChildren<Collider>();

			foreach (var collider in arr)
			{
				collider.enabled = false;
				print(collider.name);
			}
		}
	}

	public static void AddNameDescriptionParamsToSO(IScriptableObjectData data)
	{
		var list = data.GetAdditionalParameters();
		if (list == null) list = new List<AdditionalParameters>();

		if (!list.Where(a => a.paramName.Contains("Name")).Any())
			list.Add(new AdditionalParameters { paramName = "Name", value = "Write Name In SO" });

		if (!list.Where(a => a.paramName.Contains("Description")).Any())
			list.Add(new AdditionalParameters { paramName = "Description", value = "Write Description In SO" });
	}

	public static void AddOpenTimeParamsToSO(IScriptableObjectData data)
	{
		var list = data.GetAdditionalParameters();
		if (list == null) list = new List<AdditionalParameters>();

		if (!list.Where(a => a.paramName.Contains("OpenTime")).Any())
			list.Add(new AdditionalParameters { paramName = "OpenTime", value = "1" });
	}

	public static void FinishMap(IScriptableObjectData data)
	{
		var list = data.GetAdditionalParameters();
		if (list == null) list = new List<AdditionalParameters>();

		if (!list.Where(a => a.paramName.Contains("FinishMap")).Any())
			list.Add(new AdditionalParameters { paramName = "FinishMap", value = "" });
	}
}
