using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenSkill : Channelable
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
    private Effectors effectors;

    public GreenSkill(Transform player)
    {
        owner = player;
        rigid = owner.GetComponent<Rigidbody>();
        effectors = new Effectors(new StatusEffect[] { new StatusEffect(false, 0, 0), new StatusEffect(false, 0, 0), new StatusEffect(false, 0, 0) });
    }
    // Vector3 dir, float time, float dist
    public void Cast(params object[] castParams)
    {
        if (castParams == null) return;

        Vector3 dir = (Vector3)castParams[0];
        float time = (float)castParams[1];
        float dist = (float)castParams[2];

        if (!activated)
        {
            travelTime = time;
            travelDistance = dist;
            initialPosition = owner.position;
            initialDirection = dir == Vector3.zero ? owner.forward : dir;
            activated = true;
        }
    }

    public Effectors Effectors()
    {
        return effectors;
    }

    public bool Completed()
    {
        return !activated;
    }

    public void Channel()
    {
        if(activated) Dash();
    }

    public void Stop()
    {
        percentCompletion = 0;
        activated = false;
    }

    private void Dash()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);
        float t = percentCompletion;
        //t = t * t * t * (t * (6f * t - 15f) + 10f);
        t = Mathf.Sin(t * Mathf.PI * 0.5f);
        Vector3 force = (rigid.transform.up * 75 * ((0.5f - t) * 2) + rigid.transform.forward * 5 * travelDistance);
        rigid.AddForce(force * Mathf.Sin(t * Mathf.PI), ForceMode.Impulse);
        rigid.AddForce(-rigid.velocity * Mathf.Sin(t * Mathf.PI * 0.5f), ForceMode.Impulse);

        if (percentCompletion > 1)
        {
            percentCompletion = 0;
            activated = false;
        }
    }

}
