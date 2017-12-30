using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableFactory : MonoBehaviour
{
    private static PoolableFactory poolableFactory;
    public static PoolableFactory Instance 
    {
        get
        {
            if (!poolableFactory)
            {
                poolableFactory = new GameObject("PoolableFactory").AddComponent<PoolableFactory>(); ;
            }
            return poolableFactory;
        }
    }

    private Dictionary<string, Queue<IPoolable>> freePoolables = new Dictionary<string, Queue<IPoolable>>();
    public IPoolable Create<T>(string type, Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour, IPoolable
    {
        IPoolable poolable;
        if (freePoolables.ContainsKey(type) && freePoolables[type].Count > 0)
        {
            poolable = freePoolables[type].Dequeue();
            poolable.Instantiate(position, rotation);
            return poolable;
        }
        else
        {
            GameObject go = (GameObject)Instantiate(Resources.Load(type), position, rotation, parent);
            poolable = go.AddComponent<T>() as IPoolable;
            poolable.SetResourceBase(type);
            return poolable;
        }
    }

    public void Reclaim(IPoolable poolable)
    {
        string type = poolable.GetResourceBase();
        if (!freePoolables.ContainsKey(type)) freePoolables.Add(type, new Queue<IPoolable>());
        freePoolables[type].Enqueue(poolable);
    }
}
