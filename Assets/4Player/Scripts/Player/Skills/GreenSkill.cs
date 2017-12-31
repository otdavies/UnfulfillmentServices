using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenSkill
{
    private Transform owner;
    private Rigidbody rigid;
    private Vector3 initialPosition;
    private Vector3 initialDirection;
    private float percentCompletion = 0;
    private float travelTime = 1;
    private float travelDistance = 2;
    private bool activated = false;
    private bool blocked = false;

    public GreenSkill(Transform player)
    {
        owner = player;
        rigid = owner.GetComponent<Rigidbody>();
    }

    public void Cast(Vector3 dir, float time, float dist)
    {
        if (!activated)
        {
            travelTime = time;
            travelDistance = dist;
            initialPosition = owner.position;
            initialDirection = dir == Vector3.zero ? owner.forward : dir;
            activated = true;
        }
    }

    public bool Completed()
    {
        return !activated;
    }

    public void UpdateSkill()
    {
        if(activated) Dash();
    }

    private void Dash()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);
        float t = percentCompletion;
        //t = t * t * t * (t * (6f * t - 15f) + 10f);
        t = Mathf.Sin(t * Mathf.PI * 0.5f);
        rigid.AddForce((rigid.transform.forward * 5 * travelDistance) * Mathf.Sin(t * Mathf.PI), ForceMode.Impulse);
        rigid.AddForce(-rigid.velocity * Mathf.Sin(t * Mathf.PI * 0.5f), ForceMode.Impulse);

        if (percentCompletion > 1)
        {
            percentCompletion = 0;
            activated = false;
        }
    }

}
