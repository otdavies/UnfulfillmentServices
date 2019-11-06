using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoolable : MonoBehaviour, IPoolable
{
    private Player player;
    private new Rigidbody rigidbody;
    private Transform thisTrans;

    private string resourceBase;

    public void Awake()
    {
        player = GetComponentInChildren<Player>();
        rigidbody = player.GetComponent<Rigidbody>();
        thisTrans = transform;
    }

    public void Destroy()
    {
        PoolableFactory.Instance.Reclaim(this);
        gameObject.SetActive(false);

    }

    public void Instantiate(Vector3 position, Quaternion rotation)
    {
        ResetFirstOrderChildrenState();
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

    private void ResetFirstOrderChildrenState()
    {
        var children = thisTrans.GetComponentsInChildren<Transform>();
        foreach (var c in children)
        {
            if (c.parent == thisTrans)
            {
                c.localPosition = Vector3.zero;
                c.localRotation = Quaternion.identity;
            }
        }
    }
}
