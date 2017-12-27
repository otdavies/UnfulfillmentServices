using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    public Camera followCamera;

    private Transform thisTransform;

	// Use this for initialization
	void Start ()
    {
        thisTransform = this.transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        thisTransform.position = new Vector3(followCamera.transform.position.x, thisTransform.position.y, followCamera.transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            other.SendMessage("DeathByFalling", SendMessageOptions.DontRequireReceiver);
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("dynamic"))
        {
            Destroy(other.gameObject, 3);
        }
    }
}
