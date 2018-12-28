using System;
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

    public ParticleSystem greenEffect;
    public ParticleSystem blueEffect;

    // Player skills
    public Skill[] skills;
    private BlueSkill blue;


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

    public enum CharacterSkill
    {
        none,
        green,
        red,
        blue,
        yellow
    }

    private CharacterMovement movementState;
    private CharacterCondition conditionState;
    private CharacterSkill skillState;
    public bool grounded = true;
    private bool casting = false;
    private bool canMove = true;
    private Vector3 lastGroundedPosition = Vector3.up * 10;
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

    public Effectors PlayerStatusEffects
    {
        get { return playerStatusEffects; }
        set { playerStatusEffects = value; }
    }

    // Player input and components
    private PlayerPuppet puppet;
    private Transform thisTransform;
    private Rigidbody physicsRigid;
    private Vector3 direction;
    private float horizontalInput;
    private float verticalInput;

    private Effectors playerStatusEffects = new Effectors(new StatusEffect[]
    {
        new Immobilized(), new Motion(), new Muted(), 
    });


    private void OnEnable()
    {
        thisTransform = this.transform;
        physicsRigid = GetComponent<Rigidbody>();
        puppet = visualModel.GetComponent<PlayerPuppet>();

        // Initialize skills
        foreach (Skill skill in skills)
        {
            skill.RegisterTo(this);
        }

        blue = new BlueSkill(this);

        previousTransformPosition = this.transform.position;

        conditionState = CharacterCondition.alive;

        blueEffect.Stop();
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
        // Movement states
        grounded = Physics.Raycast(thisTransform.position, -thisTransform.up, this.transform.localScale.y + 0.1f,
            groundedLayer);
        if (horizontalInput > 0 || horizontalInput < 0 || verticalInput > 0 || verticalInput < 0)
            movementState = CharacterMovement.moving;
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
        Vector3 result = Vector3.Lerp(previousTransformPosition, physicsRigid.transform.position,
            (Time.time - previousTime) / (Time.fixedDeltaTime));
        visualModel.position = result;
    }

    private void ProcessSkills()
    {
        //if (!casting)
        //{
        //    if (greenDown)
        //    {
        //        skillState = CharacterSkill.green;
        //        casting = true;
        //        canMove = false;
        //        green.Cast(new Vector3(horizontalInput, 0, verticalInput), 0.4f, 8f);
        //        lastCastable = green;
        //        greenEffect.Play();
        //    }

        //    if (blueDown)
        //    {
        //        skillState = CharacterSkill.blue;
        //        casting = true;
        //        canMove = false;
        //        blue.Cast(6f);
        //        blueEffect.Play();
        //        lastCastable = blue;
        //        puppet.SetHolding(true);
        //    }
        //}

        //if (casting)
        //{
        //    if (skillState == CharacterSkill.blue)
        //    {
        //        if (blueUp)
        //        {
        //            skillState = CharacterSkill.none;
        //            casting = false;
        //            canMove = true;
        //            blue.Stop();
        //            blueEffect.Stop();
        //            puppet.SetHolding(false);
        //        }

        //        if (blue.Grabbed())
        //        {
        //            blueEffect.Stop();
        //            canMove = true;
        //        }
        //    }

        //    if (skillState == CharacterSkill.green && green.Completed())
        //    {
        //        skillState = CharacterSkill.none;
        //        casting = false;
        //        canMove = true;
        //    }
        //}

        if (!IsStatusEffectActive(StatusEffects.MUTED))
        {
            foreach (Skill skill in skills)
            {
                if (skill.CanCast(controller))
                {
                    skill.Cast();
                    skill.Effectors(this, ref playerStatusEffects);
                }
            }
        }
    }

    // FixedUpdate Related -----------------------------------------

    private void FixedUpdate()
    {
        if (movementState == CharacterMovement.moving) FixedUpdatePhysicsPosition();

        FixedUpdateSkills();

        previousTransformPosition = thisTransform.position;
        previousTime = Time.time;
    }

    private void FixedUpdatePhysicsPosition()
    {
        physicsRigid.MoveRotation(Quaternion.Slerp(thisTransform.rotation, Quaternion.LookRotation(direction),
            Time.fixedDeltaTime * 12));
        if (canMove)
            physicsRigid.MovePosition(thisTransform.position +
                                      (thisTransform.forward * direction.magnitude * (Time.fixedDeltaTime * speed)));
    }

    private void FixedUpdateSkills()
    {
        foreach (Skill skill in skills)
        {
            if(!skill.Completed()) skill.Channel();
        }
    }

    // MISC State Changes -----------------------------------------
    public void Despawn()
    {
        if (conditionState != CharacterCondition.alive) return;

        movementState = CharacterMovement.stationary;
        skillState = CharacterSkill.none;
        conditionState = CharacterCondition.dead;
        casting = false;
        blue.Stop();
        puppet.SetHolding(false);
        blueEffect.Stop();
        DeactivateAllEffects();

        PlayerManager.Instance.DespawnPlayer(controller);
    }

    public void Respawn()
    {
        if (conditionState != CharacterCondition.dead) return;

        PlayerManager.Instance.SpawnPlayer(controller);

        conditionState = CharacterCondition.alive;
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
