using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementDirection { left, right, forward, back}
    public MovementDirection movementDirection;

    public enum ActivateCondition { always, activate, proximity }
    public ActivateCondition activateCondition;

    private enum DestinationCondition { initial, final }
    private DestinationCondition destinationCondition = DestinationCondition.initial;

    public float travelDistance = 1;
    public float travelTime = 1;
    public Collider proximityZone;

    private Transform thisTransform;
    private Rigidbody thisRigid;

    private Vector3 initialPosition;
    private Vector3 moveDirection;
    private float percentCompletion = 0;
    private bool activated = false;

    private void Start ()
    {
        thisTransform = this.transform;
        thisRigid = GetComponent<Rigidbody>();
        initialPosition = thisTransform.position;
        UpdateDirection();
    }

    private void UpdateDirection()
    {
        switch (movementDirection)
        {
            case MovementDirection.left: moveDirection = Vector3.left; break;
            case MovementDirection.right: moveDirection = Vector3.right; break;
            case MovementDirection.forward: moveDirection = Vector3.forward; break;
            case MovementDirection.back: moveDirection = Vector3.back; break;
        }
    }

    private void FixedUpdate()
    {
        if (activateCondition == ActivateCondition.always)
        {
            activated = true;
            ProcessPingpongMotion();
        }
        else if (activateCondition == ActivateCondition.activate && activated)
        {
            ProcessPingpongMotion();
        }
        else if (activateCondition == ActivateCondition.proximity && activated)
        {
            if(proximityZone)
            {
                ProcessPingpongMotion();
            }
        }
    }

    private void ProcessPingpongMotion()
    {
        if (destinationCondition == DestinationCondition.initial) MoveToFinal();
        else if (destinationCondition == DestinationCondition.final) MoveToInital();
    }

    private void MoveToFinal()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);
        float t = percentCompletion;
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        thisRigid.MovePosition(Vector3.Lerp(initialPosition, initialPosition + moveDirection * travelDistance, t));
        if(percentCompletion > 1)
        {
            percentCompletion = 0;
            destinationCondition = DestinationCondition.final;
            activated = false;
        }
    }

    private void MoveToInital()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);
        float t = percentCompletion;
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        thisRigid.MovePosition(Vector3.Lerp(initialPosition, initialPosition + moveDirection * travelDistance, 1 - t));
        if (percentCompletion > 1)
        {
            percentCompletion = 0;
            destinationCondition = DestinationCondition.initial;
            activated = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(activateCondition == ActivateCondition.proximity) activated = true;
    }

    public void Activate()
    {
        activated = true;
    }

    public void Deactivate()
    {
        activated = false;
    }
}
