using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPuppet : MonoBehaviour
{
    public GameObject arm1;
    public GameObject arm2;
    private Quaternion arm1Rot, arm2Rot;

    private bool holding = false;
    private Vector3 previousPos;
    private Vector3 movementDelta = Vector3.zero;
    private float movementAmount = 0;

    private void Update()
    {
        if (holding)
        {
            arm1Rot = Quaternion.Euler(-90, 0, 0);
            arm2Rot = Quaternion.Euler(-90, 0, 0);
        }
        else
        {
            if (previousPos != null) movementDelta = transform.position - previousPos;
            previousPos = transform.position;
            float mag = movementDelta.magnitude;

            if (mag > 0)
            {
                movementAmount += mag * 2;
                arm1Rot = Quaternion.Euler(Mathf.Clamp01(mag * 8) * Mathf.Sin(movementAmount) * -30, 0, 0);
                arm2Rot = Quaternion.Euler(Mathf.Clamp01(mag * 8) * Mathf.Sin(movementAmount) * 30, 0, 0);
            }
            else
            {
                arm1Rot = Quaternion.identity;
                arm2Rot = Quaternion.identity;
            }
        }
        ApplyArmChanges();
    }

    public void SetHolding(bool state)
    {
        holding = state;
    }

    private void ApplyArmChanges()
    {
        arm1.transform.localRotation = Quaternion.Lerp(arm1.transform.localRotation, arm1Rot, Time.deltaTime * 20);
        arm2.transform.localRotation = Quaternion.Lerp(arm2.transform.localRotation, arm2Rot, Time.deltaTime * 20);
    }
}
