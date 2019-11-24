using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxDestroyer : MonoBehaviour
{
    private BoxCollider destroyTrigger;

	// Use this for initialization
	void Start ()
    {
        destroyTrigger = GetComponent<BoxCollider> ();
        destroyTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        IPoolable poolable = other.GetComponent<IPoolable>();

        if(poolable != null)
        {
            poolable.Destroy();
        }
    }
}
