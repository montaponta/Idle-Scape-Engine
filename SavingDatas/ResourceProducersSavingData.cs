using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceProducersSavingData : AbstractSavingData
{
    public List<ResourceProducerState> resourceProducersList = new List<ResourceProducerState>();

    public override bool IsDataEmpty()
    {
        return resourceProducersList.Any();
    }

    public override void LoadData()
    {
        var producers = Object.FindObjectsOfType<AbstractResourceProducer>().ToList();
        var temp = producers.Where(a => a.GetComponent<IIDExtention>() != null && a.GetComponent<IIDExtention>().GetID() == "0").ToList();
        producers = producers.Except(temp).ToList();

        foreach (var item in resourceProducersList)
        {
            var v = producers.Find(a => a.GetComponent<IIDExtention>() != null && a.GetComponent<IIDExtention>().GetID() == item.id);

            if (item.id != "0" && v != null && item.producerType == ResourceProducerType.onScene)
            {
                v.isEnable = item.isEnable;
                v.resourceGiven = item.resourceGiven;
                v.gameObject.SetActive(item.isVisible);
                if (item.isRecoverTimerActive) v.StartRecover(item.recoverSeconds);
            }
            else if (item.producerType != ResourceProducerType.onScene)
            {
                var arr = GameObject.FindGameObjectsWithTag("ResourceProducerContainer");
                var container = arr.FirstOrDefault(a => a.transform.GetComponentInParent<IIDExtention>().GetID() == item.producerContainerId);
                GameObject prefab = null;
                GameObject go = null;

                if (item.producerType == ResourceProducerType.loot)
                {
                    if (savingManager.GetRef<ObjectPoolSystem>() == null)
                    {
                        prefab = savingManager.GetRef<SharedObjects>().GetLootPrefab(item.resourcesList[0].resourceType);
                        go = Object.Instantiate(prefab);
                    }
                    else
                    {
                        var pool = savingManager.GetRef<ObjectPoolSystem>();
                        go = pool.GetPoolableObject<AbstractResourceProducer>(item.resourcesList[0].resourceType.ToString()).gameObject;
                    }
                }


                go.GetComponent<AbstractResourceProducer>().isEnable = item.isEnable;

                if (container)
                {
                    go.transform.SetParent(container.transform);
                    go.transform.localPosition = Vector3.zero;
                }
                else
                {
                    go.transform.position = ExtensionClasses.ConvertToVector3(item.pos);
                    go.transform.eulerAngles = ExtensionClasses.ConvertToVector3(item.rot);
                }

                if (go.GetComponent<IResourceProducerExtention>() != null)
                {
                    go.GetComponent<IResourceProducerExtention>().SetResourceList(item.resourcesList);
                }
            }
        }
    }

    public override void ResetData(int flag = 0)
    {
        resourceProducersList.Clear();
        SaveData(false);
    }

    public override void SaveData(bool collectParams, bool isSave = true)
    {
        if (savingManager.dontSave) return;

        if (collectParams)
        {
            resourceProducersList.Clear();
            var producers = Object.FindObjectsOfType<AbstractResourceProducer>(true).ToList();
            var temp = producers.Where(a => a.GetComponent<IIDExtention>() != null && a.GetComponent<IIDExtention>().GetID() == "0").ToList();
            producers = producers.Except(temp).ToList();

            foreach (var item in producers)
            {
                var id = "0";
                if (item.GetComponent<IIDExtention>() != null) id = item.GetComponent<IIDExtention>().GetID();
                if (id == "") Debug.LogError(item.name + " id = null");
                ResourceProducerState resourceProducerState = new ResourceProducerState();
                resourceProducerState.id = id;
                resourceProducerState.isEnable = item.isEnable;
                resourceProducerState.isVisible = item.gameObject.activeSelf;
                resourceProducerState.resourceGiven = item.resourceGiven;
                resourceProducerState.name = item.name;
                resourceProducerState.pos = ExtensionClasses.ConvertToVector3Serialized(item.transform.position);
                resourceProducerState.rot = ExtensionClasses.ConvertToVector3Serialized(item.transform.eulerAngles);
                resourceProducerState.isRecoverTimerActive = item.recoverTimer.isTimerActive;
                resourceProducerState.recoverSeconds = item.recoverTimer.GetRemainSeconds();

                ResourceProducerType type = ResourceProducerType.onScene;
                var extention = item.GetComponent<IResourceProducerExtention>();

                if (extention != null)
                {
                    if (extention.GetProducerType() == ResourceProducerType.none) Debug.LogError(item.name + " = ResourceProducerType.none");
                    type = extention.GetProducerType();
                    if (type == ResourceProducerType.loot && !item.gameObject.activeSelf) continue;
                    resourceProducerState.resourcesList = item.GetComponent<IResourceProducerExtention>().GetResourceList();

                    if (item.transform.parent)
                    {
                        var v = item.transform.parent.GetComponentInParent<IIDExtention>();
                        if (v != null) resourceProducerState.producerContainerId = v.GetID();
                    }
                }

                resourceProducerState.producerType = type;
                resourceProducersList.Add(resourceProducerState);
            }
        }

        if (isSave) SaveDataObject();
    }

    protected override void SaveDataObject()
    {
        ES3.Save(ToString(), this, $"ResourceProducers_{savingManager.GetRef<Main>().GetSceneIndex()}.es3");
    }

    protected override void SaveDataObject(string key)
    {
        ES3.Save(ToString(), this, $"ResourceProducers_{savingManager.GetRef<Main>().GetSceneIndex()}_{key}.es3");
    }

    public class ResourceProducerState
    {
        public string id;
        public bool isEnable;
        public bool isVisible;
        public Dictionary<ResourceType, float> resourceGiven = new Dictionary<ResourceType, float>();
        public Vector3Serialized pos = new Vector3Serialized();
        public Vector3Serialized rot = new Vector3Serialized();
        public ResourceProducerType producerType;
        public List<CollectablesItemCount> resourcesList = new List<CollectablesItemCount>();
        public string producerContainerId;
        public string name;
        public bool isRecoverTimerActive;
        public float recoverSeconds;
    }
}
