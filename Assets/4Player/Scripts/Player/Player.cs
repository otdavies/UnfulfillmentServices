using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class Player : MonoBehaviour 
{
    // Public properties
	public float speed = 5f;
	public XboxController controller;
    public LayerMask groundedLayer;
    public Transform visualModel;

    // Player conidition
    public enum CharacterMovement { stationary, moving }
    public enum CharacterCondition { alive, dead }
    public enum CharacterSkill { none, green, red, blue, yellow }
    private CharacterMovement  movementState;
    private CharacterCondition conditionState;
    private CharacterSkill skillState;
    public bool grounded = true;
    private bool casting = false;
    private bool canMove = true;
    private Vector3 lastGroundedPosition = Vector3.up * 10;
    private Vector3 previousTransformPosition = Vector3.zero;
    private float previousTime = 0;

    // Player input and components
    private PlayerPuppet puppet;
    private Transform   thisTransform;
	private Rigidbody   physicsRigid;
	private Vector3     direction;
	private float 	    horizontalInput;
	private float 	    verticalInput;
    private bool        greenDown = false;
    private bool        blueDown = false;
    private bool        blueUp = false;

    // Player skills
    private GreenSkill green;
    private BlueSkill blue;

    private void Start () 
	{
		thisTransform = this.transform;
		physicsRigid = GetComponent<Rigidbody>();
        puppet = visualModel.GetComponent<PlayerPuppet>();

        // Initialize skills
        green = new GreenSkill(thisTransform);
        blue = new BlueSkill(thisTransform);


        previousTransformPosition = this.transform.position;
    }

    // Update Related ------------------------------------------

    private void Update () 
	{
        // Gather input
        UpdateInput();

        // Movement
        UpdateMovementState();
        UpdateMovementDirection();

        // Anti jitter movement
        UpdateVisualPosition();

        // Abilities
        ProcessSkills();
    }

    private void UpdateInput()
    {
        // Input layer
        horizontalInput = XCI.GetAxis(XboxAxis.LeftStickX, controller);
        verticalInput = XCI.GetAxis(XboxAxis.LeftStickY, controller);
        greenDown = XCI.GetButtonDown(XboxButton.A, controller);
        blueDown = XCI.GetButtonDown(XboxButton.X, controller);
        blueUp = XCI.GetButtonUp(XboxButton.X, controller);
    }

    private void UpdateMovementState()
    {
        // Movement states
        grounded = Physics.Raycast(thisTransform.position, -thisTransform.up, this.transform.localScale.y + 0.1f, groundedLayer);
        if (horizontalInput > 0 || horizontalInput < 0 || verticalInput > 0 || verticalInput < 0) movementState = CharacterMovement.moving;
        else movementState = CharacterMovement.stationary;

        if (grounded) lastGroundedPosition = thisTransform.position;
    }

    private void UpdateMovementDirection()
    {
        physicsRigid.angularVelocity = Vector3.zero;

        // Apply movement
        if (movementState == CharacterMovement.moving)
        {
            direction = (Vector3.right * horizontalInput + Vector3.forward * verticalInput);
        }
    }

    private void UpdateVisualPosition()
    {
        visualModel.rotation = thisTransform.rotation;
        Vector3 result = Vector3.Lerp(previousTransformPosition, physicsRigid.transform.position, (Time.time - previousTime) / (Time.fixedDeltaTime));
        visualModel.position = result;
    }

    private void ProcessSkills()
    {
        if (!casting && grounded)
        {
            if (greenDown)
            {
                skillState = CharacterSkill.green;
                casting = true;
                canMove = false;
                green.Cast(new Vector3(horizontalInput, 0, verticalInput), 0.2f, 10);
            }

            if (blueDown)
            {
                skillState = CharacterSkill.blue;
                casting = true;
                canMove = false;
                blue.ChannelStart(6);
                puppet.SetHolding(true);
            }
        }

        if(casting)
        {
            if (skillState == CharacterSkill.blue)
            {
                if (blueUp)
                {
                    skillState = CharacterSkill.none;
                    casting = false;
                    canMove = true;
                    blue.ChannelEnd();
                    puppet.SetHolding(false);
                }

                if(blue.Grabbed())
                {
                    canMove = true;
                }
            }

            if(skillState == CharacterSkill.green && green.Completed())
            {
                skillState = CharacterSkill.none;
                casting = false;
                canMove = true;
            }
        }
    }

    // FixedUpdate Related -----------------------------------------

    private void FixedUpdate()
    {
        if (movementState == CharacterMovement.moving) FixedUpdatePhysicsPosition();
        if(casting) FixedUpdateSkills();

        previousTransformPosition = thisTransform.position;
        previousTime = Time.time;
    }

    private void FixedUpdatePhysicsPosition()
    {
        physicsRigid.MoveRotation(Quaternion.Slerp(thisTransform.rotation, Quaternion.LookRotation(direction), Time.fixedDeltaTime * 12));
        if(canMove) physicsRigid.MovePosition(thisTransform.position + (thisTransform.forward * direction.magnitude * (Time.fixedDeltaTime * speed)));
    }

    private void FixedUpdateSkills()
    {
        if (skillState == CharacterSkill.green) green.UpdateSkill();
        else if (skillState == CharacterSkill.blue) blue.UpdateSkill();
    }


    // MISC State Changes -----------------------------------------
    private void DeathByFalling()
    {
        movementState = CharacterMovement.stationary;
        skillState = CharacterSkill.none;
        conditionState = CharacterCondition.dead;
        this.gameObject.SetActive(false);
        thisTransform.position = lastGroundedPosition;
        physicsRigid.velocity = Vector3.zero;

        Invoke("Respawn", 1);
    }

    private void Respawn()
    {
        gameObject.SetActive(true);
    }
}
