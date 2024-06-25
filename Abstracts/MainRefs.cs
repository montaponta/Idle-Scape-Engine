using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainRefs : MonoBehaviour
{
    private Dictionary<Type, MainRefs> systemsPairs = new Dictionary<Type, MainRefs>();

    protected virtual void Start()
    {
        StartCoroutine(PostStartCoroutine());
        StartCoroutine(DelayStartCoroutine());
    }

    public virtual void OnInitializeFinished() { }
    public virtual void DelayStart() { }
    // PostStart выполняется не сразу после старта а после одного или двух апдейтов
    public virtual void PostStart() { }
    private IEnumerator DelayStartCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        DelayStart();
    }

    private IEnumerator PostStartCoroutine()
    {
        yield return new WaitForEndOfFrame();
        PostStart();
    }

    public T GetRef<T>() where T : MainRefs
    {
        if (systemsPairs.TryGetValue(typeof(T), out var pair))
            return (T)systemsPairs[typeof(T)];
        else
        {
            var reference = FindObjectOfType<T>(true);
            systemsPairs.Add(typeof(T), reference);
            return reference;
        }
    }
}

