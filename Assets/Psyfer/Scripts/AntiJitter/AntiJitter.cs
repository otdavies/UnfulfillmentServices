﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AntiJitter : MonoBehaviour
{
    public Transform visualModel;

    private Transform thisTransform;
    private Vector3 previousTransformPosition = Vector3.zero;
    private float previousTime = 0;

    public void ResetPosition()
    {
        visualModel.position = thisTransform.position;
        visualModel.rotation = thisTransform.rotation;
    }

    private void OnEnable()
    {
        thisTransform = this.transform;
    }

    private void FixedUpdate()
    {
        previousTransformPosition = thisTransform.position;
        previousTime = Time.time;
    }

    private void LateUpdate()
    {
        visualModel.rotation = thisTransform.rotation;
        Vector3 result = Vector3.Lerp(previousTransformPosition, thisTransform.position, (Time.time - previousTime) / (Time.fixedDeltaTime));
        visualModel.position = result;
    }
}
