using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolSystem : MainRefs
{
    [SerializeField] private PoolableGameObjectProvider poolableGameObjectProvider;

    public static ObjectPoolSystem shared;

    private Dictionary<string, ObjectPool<GameObject>> pool = new();

    private void Awake()
    {
        shared = this;
        FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
        var arr = FindObjectsOfType<ObjectPoolSystem>();
        if (arr.Length > 1) Debug.LogError($"Singleton {nameof(ObjectPoolSystem)} already exists!!!");
#endif
    }

    public void Reserve(string gameObjectType, int count = 10)
    {
        CheckPool(gameObjectType);

        if (pool[gameObjectType].CountInactive >= count)
            return;

        var reserve = new GameObject[count - pool[gameObjectType].CountInactive];

        for (int i = 0; i < count; i++)
            reserve[i] = pool[gameObjectType].Get();

        for (int i = 0; i < count; i++)
            pool[gameObjectType].Release(reserve[i]);
    }

    public GameObject GetPoolableObject(string gameObjectType)
    {
        CheckPool(gameObjectType);
        return pool[gameObjectType].Get();
    }

    public T GetPoolableObject<T>(string gameObjectType) where T : Component
    {
        CheckPool(gameObjectType);
        return pool[gameObjectType].Get().GetComponent<T>();
    }

    public void GetPoolableObjects(string gameObjectType, int count, out GameObject[] gameObjects)
    {
        CheckPool(gameObjectType);

        gameObjects = new GameObject[count];
        for (var index = 0; index < gameObjects.Length; index++)
            gameObjects[index] = pool[gameObjectType].Get();
    }

    public void GetPoolableObjects<T>(string gameObjectType, int count, out T[] gameObjects) where T : Component
    {
        CheckPool(gameObjectType);

        gameObjects = new T[count];
        for (var index = 0; index < gameObjects.Length; index++)
            gameObjects[index] = pool[gameObjectType].Get().GetComponent<T>();
    }

    public void ReleasePoolableObject(string gameObjectType, GameObject obj)
    {
        if (pool.ContainsKey(gameObjectType))
            pool[gameObjectType].Release(obj);
        else
            Debug.Log($"Attempt to release object '{gameObjectType}' to pool that doesn't exist");
    }

    public void ReleasePoolableObjects(string gameObjectType, IEnumerable<GameObject> objects)
    {
        if (pool.ContainsKey(gameObjectType))
        {
            foreach (var obj in objects)
                pool[gameObjectType].Release(obj);
        }
        else
            Debug.Log($"Attempt to release object '{gameObjectType}' to pool that doesn't exist");
    }

    public void ClearPool(string gameObjectType)
    {
        if (!pool.ContainsKey(gameObjectType))
            return;

        pool[gameObjectType].Clear();
        pool.Remove(gameObjectType);
    }

    private void CheckPool(string gameObjectType)
    {
        if (!pool.ContainsKey(gameObjectType))
            CreatePool(gameObjectType);
    }

    private void CreatePool(string gameObjectType, int count = 10)
    {
        if (pool.ContainsKey(gameObjectType))
            return;

        var poolObjectInfo = poolableGameObjectProvider.GetPoolableGameObject(gameObjectType);
        if (string.IsNullOrEmpty(poolObjectInfo.ObjectName)
            || poolObjectInfo.GameObject == null)
            Debug.LogError($"Can't find object '{gameObjectType}' in poolableGameObjectProvider");
        else
        {
            var objectPool = new ObjectPool<GameObject>(() => CreateGameObject(poolObjectInfo), actionOnRelease: DisableGameObject, defaultCapacity: count);
            pool.Add(gameObjectType, objectPool);
        }
    }

    private GameObject CreateGameObject(PoolableGameObjectProvider.PoolObjectInfo poolObjectInfo)
    {
        var newGameObject = Instantiate(poolObjectInfo.GameObject, transform);
        newGameObject.name = poolObjectInfo.ObjectName;
        return newGameObject;
    }

    private void DisableGameObject(GameObject disableGameObject)
    {
        disableGameObject.gameObject.SetActive(false);
        disableGameObject.transform.SetParent(transform);
    }
}
