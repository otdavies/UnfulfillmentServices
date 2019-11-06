using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public bool renderDebug = true;
    public Vector2 extent = Vector3.one;
    public Vector3 forceDirection = Vector3.forward;
    public float strength = 10;
    private BoxCollider triggerZone;

    void Start()
    {
        triggerZone = gameObject.AddComponent<BoxCollider>();
        triggerZone.size = new Vector3(extent.x, 0.1f, extent.y);
        triggerZone.isTrigger = true;

        forceDirection = transform.TransformDirection(forceDirection);
    }

    Rigidbody body;
    private void OnTriggerStay(Collider other)
    {
        body = other.attachedRigidbody;
        if(body != null) body.velocity = forceDirection * strength;
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        if (!renderDebug) return;

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
