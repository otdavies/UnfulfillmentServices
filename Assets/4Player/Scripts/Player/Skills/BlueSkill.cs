﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueSkill
{
    private Transform owner;
    private Rigidbody rigid;
    private Rigidbody heldRigid;
    private RaycastHit[] hits;
    private Vector3 pullTarget;
    private float percentCompletion = 0;
    private float distance = 2;
    private bool activated = false;
    private bool holding = false;

    public BlueSkill(Transform player)
    {
        owner = player;
        rigid = owner.GetComponent<Rigidbody>();
    }

    public void ChannelStart(float dist)
    {
        distance = dist;
        activated = true;
    }

    public void ChannelEnd()
    {
        activated = false;
        holding = false;
    }

    public void UpdateSkill()
    {
        if (activated)
        {
            pullTarget = owner.position + owner.forward * 1.3f + Vector3.up * 0.3f;
            if (!holding) Pull();
            else Hold();
        }
    }

    public bool Grabbed()
    {
        return holding;
    }

    private void Pull()
    {
        hits = Physics.SphereCastAll(owner.position, 1, owner.forward, distance, LayerMask.GetMask("dynamic"));
        foreach (RaycastHit h in hits)
        {
            if (h.rigidbody)
            {
                float dist = 1 - Mathf.Clamp01(Vector3.Distance(pullTarget, h.transform.position));

                // Switch to holding
                if (dist > 0.03f)
                {
                    holding = true;
                    heldRigid = h.rigidbody;
                    return;
                }

                // Added the forces
                h.rigidbody.AddForce((pullTarget - h.point).normalized * 40 * (Mathf.Cos(dist * Mathf.PI * 0.5f)));
                h.rigidbody.AddForce((-h.rigidbody.velocity) * Mathf.Sin(dist * Mathf.PI * 0.5f), ForceMode.Impulse);
                h.rigidbody.AddForce(Vector3.up * 9.78f * 4);
            }
        }
    }

    private void Hold()
    {
        float dist = Mathf.Clamp01(Vector3.Distance(pullTarget, heldRigid.position) - 0.25f);
        heldRigid.velocity *= dist;
        heldRigid.angularVelocity *= dist;
        heldRigid.MovePosition(Vector3.Lerp(pullTarget, heldRigid.position, dist));
        heldRigid.MoveRotation(Quaternion.Lerp(rigid.rotation, heldRigid.rotation, dist));
    }
}
