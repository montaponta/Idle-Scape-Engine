using System;
using System.Linq;
using UnityEngine;

public class PoolableGameObjectProvider : MonoBehaviour
{
    [SerializeField] private PoolObjectInfo[] poolObjectInfos;

    public PoolObjectInfo GetPoolableGameObject(string objectName)
        => poolObjectInfos.FirstOrDefault(e => e.ObjectName == objectName);
    
    [Serializable]
    public struct PoolObjectInfo
    {
        public string ObjectName;
        public GameObject GameObject;
    }
}