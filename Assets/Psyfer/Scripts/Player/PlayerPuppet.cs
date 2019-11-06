using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPuppet : MonoBehaviour
{
    public GameObject arm1;
    public GameObject arm2;
    public GameObject body;
    public GameObject head;

    private Player player;
    private Quaternion arm1Rot, arm2Rot, bodyRot, headRot;

    private Vector3 previousPos;
    private Vector3 movementDelta = Vector3.zero;
    private float movementAmount = 0;
    private bool holding = false;

    private void Start()
    {
        previousPos = transform.position;
    }

    private void Update()
    {
        bodyRot = Quaternion.Euler(0, 180, 0);
        headRot = Quaternion.Euler(0, 0, 0);

        movementDelta = transform.position - previousPos;
        previousPos = transform.position;
        float mag = movementDelta.magnitude;

        if (holding)
        {
            arm1Rot = Quaternion.Euler(-90, 0, 0);
            arm2Rot = Quaternion.Euler(-90, 0, 0);
        }
        else if (!player.grounded)
        {
            arm1Rot = Quaternion.Euler(Mathf.Lerp(0, 90, mag), 0, 0);
            arm2Rot = Quaternion.Euler(Mathf.Lerp(0, 90, mag), 0, 0);
            bodyRot = Quaternion.Euler(-20, 180, 0);
            headRot = Quaternion.Euler(30, 0, 0);
        }
        else
        {
            if (mag > 0)
            {
                movementAmount += mag;
                arm1Rot = Quaternion.Euler(Mathf.Clamp01(mag * 8) * Mathf.Sin(movementAmount) * -30, 0, 0);
                arm2Rot = Quaternion.Euler(Mathf.Clamp01(mag * 8) * Mathf.Sin(movementAmount) * 30, 0, 0);
            }
            else
            {
                arm1Rot = Quaternion.identity;
                arm2Rot = Quaternion.identity;
            }
        }
        ApplyVisualChanges();
    }

    public void SetOwner(Player player)
    {
        this.player = player;
    }

    public void SetHolding(bool state)
    {
        holding = state;
    }

    private void ApplyVisualChanges()
    {
        arm1.transform.localRotation = Quaternion.Lerp(arm1.transform.localRotation, arm1Rot, Time.deltaTime * 20);
        arm2.transform.localRotation = Quaternion.Lerp(arm2.transform.localRotation, arm2Rot, Time.deltaTime * 20);
        body.transform.localRotation = Quaternion.Lerp(body.transform.localRotation, bodyRot, Time.deltaTime * 10);
        head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, headRot, Time.deltaTime * 20);
    }
}
