using System.Collections.Generic;
using UnityEngine;

public class IDManager : MonoBehaviour
{
    
}

public interface IIDExtention
{
    public void SetID(string id);
    public string GetID();
    public GameObject GetGameObject();
    public Object GetObject();
}

public interface IIDCollectionExtension
{
    IEnumerable<IIDExtention> GetCollection();
}
