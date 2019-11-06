using UnityEngine;
using XboxCtrlrInput;

[CreateAssetMenu(fileName = "DashSkill", menuName = "Skills/Abilities/Dash", order = 1)]
public class DashSkill : Skill
{
    [Header("Time and Distance")]
    public float travelTime = 1;
    public float forwardTravelDistance = 2;
    public float verticalTranvelDistance = 1;
    public float recastPercentageComplete = 0.8f;
    public bool requiresGrounded = false;
    public bool requiresNotGrounded = false;
    public bool cancelVerticalVelocityOnCast = false;
    public int numberOfCastsBeforeNeedGround = 1;

    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private float percentCompletion = 0;
    private int airCastCount = 0;

    public override void RegisterTo(Player player)
    {
        base.RegisterTo(player);
        playerTransform = player.transform;
        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
    }

    public override bool CanCast(XboxController controller)
    {
        if (caster.grounded) airCastCount = 0;

        bool canCast = base.CanCast(controller);
        // "Readable"
        return requiresGrounded ? canCast && caster.grounded : requiresNotGrounded ? numberOfCastsBeforeNeedGround > airCastCount && canCast && !caster.grounded : numberOfCastsBeforeNeedGround > airCastCount && canCast;
    }

    public override void Cast()
    {
        base.Cast();
        Vector3 verticalForce = playerRigidbody.transform.up * verticalTranvelDistance;
        if(cancelVerticalVelocityOnCast) playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,  0, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(verticalForce * 10, ForceMode.Impulse);
        airCastCount++;
    }

    public override void Channel()
    {
        if(activated) Dash();
    }

    public override void Stop()
    {
        base.Stop();
        percentCompletion = 0;
    }

    private void Dash()
    {
        percentCompletion += (Time.fixedDeltaTime / travelTime);

        // Apply motion forces
        Vector3 horizontalForce = playerRigidbody.transform.forward * 5 * forwardTravelDistance;
        Vector3 verticalForce = playerRigidbody.transform.up * verticalTranvelDistance;
        playerRigidbody.AddForce(horizontalForce * Mathf.Sin(percentCompletion * Mathf.PI), ForceMode.Impulse);
        playerRigidbody.AddForce(-playerRigidbody.velocity.normalized * verticalTranvelDistance * Mathf.Sin(percentCompletion * Mathf.PI * 0.5f), ForceMode.Impulse);

        playerRigidbody.AddForce(-verticalForce * Mathf.Sin(percentCompletion * Mathf.PI * 0.5f), ForceMode.Impulse);

        if (percentCompletion > recastPercentageComplete)
        {
            Stop();
        }
    }

}
