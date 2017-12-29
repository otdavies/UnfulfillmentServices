using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Vector2 extent = Vector3.one;
    public Vector3 forceDirection = Vector3.forward;
    public float strength = 10;

    private List<Rigidbody> rigids = new List<Rigidbody>();
    private BoxCollider triggerZone;

    void Start()
    {
        triggerZone = gameObject.AddComponent<BoxCollider>();
        triggerZone.size = new Vector3(extent.x, 0.1f, extent.y);
        triggerZone.isTrigger = true;

        forceDirection = transform.TransformDirection(forceDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        rigids.Add(other.attachedRigidbody);
    }

    private void OnTriggerExit(Collider other)
    {
        rigids.Remove(other.attachedRigidbody);
    }
	
	void FixedUpdate ()
    {
		for(int i = 0; i < rigids.Count; i++)
        {
            rigids[i].velocity = forceDirection * strength;
        }
	}

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        Vector3 h, v;
        h = (extent.x * 0.5f * transform.right);
        v = (extent.y * 0.5f * transform.forward);

        Vector3 ll, ul, lr, ur;
        ll = transform.position - h - v;
        ul = transform.position + h - v;
        lr = transform.position - h + v;
        ur = transform.position + h + v;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(ll, ul);
        Gizmos.DrawLine(ul, ur);
        Gizmos.DrawLine(ur, lr);
        Gizmos.DrawLine(lr, ll);

        Gizmos.color = Color.red;
        if(!Application.isPlaying)
            Gizmos.DrawRay(transform.position, transform.TransformDirection(forceDirection) * 3);
        else
            Gizmos.DrawRay(transform.position, forceDirection * 3);
    }
}
