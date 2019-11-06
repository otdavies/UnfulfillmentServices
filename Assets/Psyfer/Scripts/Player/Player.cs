using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class Player : MonoBehaviour
{
    // Public properties
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;
    public XboxController controller;
    public LayerMask groundedLayer;
    public Transform visualModel;

    // Player skills
    public Skill[] skills;

    public bool grounded = true;

    // Player conidition
    public enum CharacterMovement
    {
        stationary,
        moving
    }

    public enum CharacterCondition
    {
        alive,
        dead
    }

    private CharacterMovement movementState;
    private CharacterCondition conditionState;
    private Vector3 previousTransformPosition = Vector3.zero;
    private float previousTime = 0;

    public Material PlayerMaterial
    {
        set
        {
            PlayerColorableParts[] parts = visualModel.GetComponentsInChildren<PlayerColorableParts>();
            foreach (PlayerColorableParts playerColorableParts in parts)
            {
                playerColorableParts.PartMaterial = value;
            }
        }
    }

    // Player input and components
    private PlayerPuppet puppet;
    private Transform thisTransform;
    private Rigidbody physicsRigid;
    private CapsuleCollider physicsCollider;
    private Vector3 direction;
    private float horizontalInput;
    private float verticalInput;

    public Effectors PlayerStatusEffects
    {
        get { return playerStatusEffects; }
        set { playerStatusEffects = value; }
    }

    private Effectors playerStatusEffects = new Effectors(new StatusEffect[]
    {
        new Muted(), new Motion(), new Rotation(), 
    });


    private void OnEnable()
    {
        thisTransform = this.transform;
        physicsRigid = GetComponent<Rigidbody>();
        physicsCollider = GetComponent<CapsuleCollider>();
        puppet = visualModel.GetComponent<PlayerPuppet>();
        puppet.SetOwner(this);

        // Initialize skills
        for (int index = 0; index < skills.Length; index++)
        {
            // Copy the scriptable object
            skills[index] = Instantiate(skills[index]);
            skills[index].RegisterTo(this);
        }

        previousTransformPosition = this.transform.position;
        conditionState = CharacterCondition.alive;
    }

    // Update Related ------------------------------------------

    private void Update()
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
    }

    private void UpdateMovementState()
    {
        float radius = physicsCollider.radius * 0.9f;
        Vector3 pos = (transform.position + physicsCollider.center) - Vector3.up * (physicsCollider.height - 1) * 0.5f;
        grounded = Physics.CheckSphere(pos, radius, groundedLayer);

        // Movement states
        if (horizontalInput > 0 || horizontalInput < 0 || verticalInput > 0 || verticalInput < 0)
            movementState = CharacterMovement.moving;
        else movementState = CharacterMovement.stationary;
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
        Vector3 result = Vector3.Lerp(previousTransformPosition, physicsRigid.transform.position, (Time.time - previousTime) / Time.fixedDeltaTime);
        visualModel.position = result;
    }

    private void ProcessSkills()
    {
        if (conditionState == CharacterCondition.dead) return;

        if (!IsStatusEffectActive(StatusEffects.MUTED))
        {
            foreach (Skill skill in skills)
            {
                if (skill.CanCast(controller))
                {
                    skill.Cast();
                    skill.ApplyEffectors(ref playerStatusEffects);
                }
            }
        }
    }

    // FixedUpdate Related -----------------------------------------

    private void FixedUpdate()
    {
        if (conditionState == CharacterCondition.dead) return;

        if (movementState == CharacterMovement.moving) FixedUpdatePhysicsPosition();

        FixedUpdateSkills();

        previousTransformPosition = thisTransform.position;
        previousTime = Time.time;
    }

    private void FixedUpdatePhysicsPosition()
    {
        physicsRigid.MoveRotation(Quaternion.Slerp(thisTransform.rotation, Quaternion.LookRotation(direction), Time.fixedDeltaTime * rotationSpeed));
        physicsRigid.MovePosition(thisTransform.position + thisTransform.forward * direction.magnitude * (Time.fixedDeltaTime * moveSpeed));
    }

    private void FixedUpdateSkills()
    {
        foreach (Skill skill in skills)
        {
            if(!skill.Completed()) skill.Channel();
        }
    }

    // MISC State Changes -----------------------------------------

    public void Respawn()
    {
        if (conditionState != CharacterCondition.dead) return;

        PlayerManager.Instance.SpawnPlayer(controller);
        conditionState = CharacterCondition.alive;
    }

    public void Despawn()
    {
        if (conditionState == CharacterCondition.dead) return;

        movementState = CharacterMovement.stationary;
        conditionState = CharacterCondition.dead;
        puppet.SetHolding(false);
        DeactivateAllEffects();

        PlayerManager.Instance.DespawnPlayer(controller);
    }

    // Effector operations
    public bool IsStatusEffectActive(StatusEffects effect)
    {
        return playerStatusEffects.effects[(int) effect].isActive;
    }

    public void DeactivateAllEffects()
    {
        foreach (StatusEffect statusEffect in playerStatusEffects.effects)
        {
            statusEffect.RemoveEffect(this);
        }
    }
}
