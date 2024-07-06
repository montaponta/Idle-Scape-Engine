using System.Collections.Generic;
using System.Linq;

public class CraftItemsSavingData : AbstractSavingData
{
    public Dictionary<string, CraftItemData> pairs = new Dictionary<string, CraftItemData>();

    public override bool IsDataEmpty()
    {
        return pairs.Any();
    }

    public override void LoadData()
    {
        savingManager.GetRef<CollectAndCraftFunctions>().OnItemCrafted += SaveCraftItemState;
    }

    public override void ResetData(int flag = 0)
    {
        pairs.Clear();
    }

    public override void SaveData(bool collectParams)
    {
        if (savingManager.dontSave) return;
        if (collectParams)
        {
            foreach (var item in savingManager.GetRef<CollectAndCraftFunctions>().craftItemsList)
            {
                AddDataToPair(item);
            }
        }

        SaveDataObject();
    }

    public void SaveCraftItemState(AbstractCraftItem craftItem)
    {
        AddDataToPair(craftItem);
        SaveData(false);
    }

    private void AddDataToPair(AbstractCraftItem craftItem)
    {
        CraftItemData craftItemData = new CraftItemData();
        craftItemData.collectedList = craftItem.GetCollectedList();
        craftItemData.id = craftItem.GetID();
        craftItemData.isInImproveProgress = craftItem.isInImproveProgress;
        craftItemData.level = craftItem.level;
        craftItemData.name = craftItem.name;
        craftItemData.remainSeconds = craftItem.craftTimer.GetRemainSeconds();

        if (pairs.ContainsKey(craftItem.id)) pairs[craftItem.id] = craftItemData;
        else pairs.Add(craftItem.id, craftItemData);
    }

    public CraftItemData GetCraftItemState(AbstractCraftItem craftItem)
    {
        if (!pairs.ContainsKey(craftItem.id)) return null;
        var data = pairs[craftItem.id];
        craftItem.SetCollectedList(data.collectedList);
        craftItem.isInImproveProgress = data.isInImproveProgress;
        craftItem.level = data.level;
        return data;
    }

    protected override void SaveDataObject()
    {
        ES3.Save(ToString(), this);
    }

    public class CraftItemData
    {
        public int level;
        public List<CollectablesItemCount> collectedList = new List<CollectablesItemCount>();
        public bool isInImproveProgress;
        public string id;
        public string name;
        public float remainSeconds;
    }
}
