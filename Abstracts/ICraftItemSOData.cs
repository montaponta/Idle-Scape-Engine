using System.Collections.Generic;

public interface ICraftItemSOData : IScriptableObjectData
{
	public ResourceType GetCraftItemType();
	public List<ICraftItemPrices> GetIPricesFromNeedResourceList();
	public List<ICraftItemPrices> GetIPricesFromNeedInventoryResourceList();
	public CraftParameters GetCraftParams();
}
