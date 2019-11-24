using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    public UnityEvent onDown;
    public UnityEvent onUp;
    public GameObject glowComponent;
    private List<Rigidbody> weightObjs = new List<Rigidbody>();
    public float minimumWeightToActivate = 1;
    private Material mat;
    private float totalCurrentWeight = 0;
    private bool activated = false;

    private void Start()
    {
        mat = glowComponent.GetComponent<Renderer>().material;
    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigid = other.GetComponent<Rigidbody>();
        if(rigid && rigid.gameObject.layer == LayerMask.NameToLayer("dynamic"))
        {
            other.SendMessage("OnPressurePlateDown", SendMessageOptions.DontRequireReceiver);
            weightObjs.Add(rigid);
        }

        totalCurrentWeight = CalculateTotalMass();

        if (!activated && totalCurrentWeight >= minimumWeightToActivate)
        {
            activated = true;
            onDown.Invoke();
        }

        mat.SetFloat("_shine", totalCurrentWeight / minimumWeightToActivate);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rigid = other.GetComponent<Rigidbody>();
        if (rigid && rigid.gameObject.layer == LayerMask.NameToLayer("dynamic"))
        {
            other.SendMessage("OnPressurePlateUp", SendMessageOptions.DontRequireReceiver);
            weightObjs.Remove(rigid);
        }

        float totalCurrentWeight = CalculateTotalMass();

        if (activated && totalCurrentWeight < minimumWeightToActivate)
        {
            activated = false;
            onUp.Invoke();
        }

        mat.SetFloat("_shine", totalCurrentWeight / minimumWeightToActivate);
    }

    private float CalculateTotalMass()
    {
        float totalCurrentWeight = 0;
        if (weightObjs.Count > 0)
        foreach (Rigidbody r in weightObjs)
        {
            totalCurrentWeight += r.mass;
        }
        return totalCurrentWeight;
    }
}
