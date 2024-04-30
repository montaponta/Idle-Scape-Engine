using System.Collections.Generic;
using System.Linq;

public class EngineUtilsFunctions
{

//    public void AddShopKeysToI2Asset(bool overrideStrings = true)
//    {
//        var shopIDs = FindObjectOfType<ShopIDs>();

//        foreach (var item in shopIDs.shopIDsList)
//        {
//            //if (item.type != ShopProductType.airdrops && item.type != ShopProductType.skins) continue;
//            //AddTagStrToI2Asset(item.name, item.name, overrideStrings);
//        }

//        LocalizationManager.Sources[0].Editor_SetDirty();
//#if UNITY_EDITOR
//        AssetDatabase.SaveAssets();
//#endif
//    }

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
}
