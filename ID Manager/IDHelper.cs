using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDHelper : MonoBehaviour, IIDExtention
{
    public string id;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public string GetID()
    {
        return id;
    }

    public UnityEngine.Object GetObject()
    {
        return this;
    }

    public void SetID(string id)
    {
        this.id = id;
    }
}
