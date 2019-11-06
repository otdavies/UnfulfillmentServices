using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPoolable : MonoBehaviour, IPoolable
{
    private new Rigidbody rigidbody;
    private Transform thisTrans;
    private string resourceBase;

    public void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        thisTrans = transform;
    }

    public void Destroy()
    {
        PoolableFactory.Instance.Reclaim(this);
        gameObject.SetActive(false);
    }

    public void Instantiate(Vector3 position, Quaternion rotation)
    {
        thisTrans.position = position;
        thisTrans.rotation = rotation;

        if (rigidbody)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(true);
    }

    public string GetResourceBase()
    {
        return resourceBase;
    }

    public void SetResourceBase(string resource)
    {
        resourceBase = resource;
    }
}
