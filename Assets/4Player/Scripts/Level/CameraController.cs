using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 trackedRegion;
    private Player[] players;
    private Vector3 startPosition;
    private Vector3 moveDirection = Vector3.zero;
    private Bounds trackingBox;
    private float size = 1;
	// Use this for initialization
	private void Start ()
    {
        players = FindObjectsOfType<Player>();
        trackingBox.center = transform.position + transform.forward * 30;
        trackingBox.extents = trackedRegion * size;
    }
	
	// Update is called once per frame
	private void Update ()
    {
        moveDirection = Vector3.zero;
        foreach (Player p in players)
        {
            if(!trackingBox.Contains(p.transform.position))
            {
                moveDirection += (p.transform.position - trackingBox.ClosestPoint(p.transform.position)) * 2;
            }
        }
        trackingBox.center += moveDirection * Time.deltaTime;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, trackingBox.center - transform.forward * 30, Time.smoothDeltaTime * 30);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(trackingBox.center, trackingBox.size);
    }
}
