using System;
using System.Linq;
using UnityEngine;

public class PoolableGameObjectProvider : MonoBehaviour
{
    [SerializeField] private PoolObjectInfo[] poolObjectInfos;

    public Component GetPoolableGameObject(string objectName)
        => poolObjectInfos.FirstOrDefault(e => e.ObjectName == objectName).Component;
    
    [Serializable]
    public struct PoolObjectInfo
    {
        public string ObjectName;
        public Component Component;
    }
}