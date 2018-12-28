using System;
using UnityEngine;
using XboxCtrlrInput;

[CreateAssetMenu(fileName = "HopSkill", menuName = "Skills/Abilities/Hop", order = 1)]
public class GreenSkill : Skill
{
    [Header("Skill Trigger")]
    public XboxButton triggerStart;

    [Header("Time and Distance")]
    public float travelTime = 1;
    public float forwardTravelDistance = 2;
    public float verticalTranvelDistance = 1;
    public float recastPercentageComplete = 0.8f;

    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private float percentCompletion = 0;

    public override void RegisterTo(Player player)
    {
        base.RegisterTo(player);
        playerTransform = player.transform;
        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
    }

    public override bool CanCast(XboxController controller)
    {
        return XCI.GetButtonDown(triggerStart, controller);
    }

    // float time, float dist
    public override void Cast(params object[] castParams)
    {
        //if (castParams == null) return;

        if (!activated)
        {
            activated = true;
        }

        foreach (SkillVisualizers skillVisualizers in onVisualizerStart)
        {
            skillVisualizers.Start();
        }
    }

    public override void ApplyEffectors(ref Effectors playerStatusEffect)
    {
        foreach (SkillArtifacts castEffect in onArtifactStart)
        {
            playerStatusEffect.effects[(int)castEffect.effect].ApplyEffect(caster, castEffect.severity, castEffect.time);
        }
    }

    public override bool Completed()
    {
        return !activated;
    }

    public override void Channel()
    {
        if(activated) Dash();
    }

    public override void Stop()
    {
        percentCompletion = 0;
        activated = false;

        foreach (SkillVisualizers skillVisualizers in onVisualizerStart)
        {
            skillVisualizers.End();
        }
    }

    private void Dash()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);
        //t = t * t * t * (t * (6f * t - 15f) + 10f);
        float t = Mathf.Sin(percentCompletion * Mathf.PI * 0.5f);

        // Apply motion forces
        Vector3 verticalForce = playerRigidbody.transform.up * 75 * ((0.5f - t) * 2) * verticalTranvelDistance;
        Vector3 horizontalForce = playerRigidbody.transform.forward * 5 * forwardTravelDistance * Mathf.Sin(t * Mathf.PI);
        playerRigidbody.AddForce(horizontalForce + verticalForce, ForceMode.Impulse);

        // Apply corrective forces
        playerRigidbody.AddForce(-playerRigidbody.velocity * Mathf.Sin(t * Mathf.PI * 0.5f), ForceMode.Impulse);

        if (percentCompletion > recastPercentageComplete)
        {
            Stop();
        }
    }

}
