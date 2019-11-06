using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using XboxCtrlrInput;

public enum StatusEffects
{
    MUTED = 0,
    MOTION = 1,
    ROTATION = 2,
}

public class Effectors
{
    public StatusEffect[] effects;

    public Effectors(StatusEffect[] effects)
    {
        this.effects = effects;
    }

    public void AddStatusEffect(Player player, StatusEffects effect, float severity, float duration)
    {
        effects[(int)effect].ApplyEffect(player, severity, duration);
    }

    public void RemoveStatusEffect(Player player, StatusEffects effect)
    {
        effects[(int)effect].RemoveEffect(player);
    }
}

public abstract class Skill : ScriptableObject, Castable
{
    protected Player caster;
    protected bool activated;

    public bool IsActivated
    {
        get { return activated; }
    }

    public Player Caster
    {
        get { return caster; }
    }

    public virtual void RegisterTo(Player player)
    {
        this.caster = player;
        this.activated = false;

        foreach (SkillVisualizers skillVisualizers in onVisualizerStart)
        {
            skillVisualizers.Setup(player);
        }
    }

    public virtual bool CanCast(XboxController controller)
    {
        return XCI.GetButtonDown(activationButton, controller);
    }

    public virtual void ApplyEffectors(ref Effectors playerStatusEffect)
    {
        foreach (SkillArtifacts castEffect in onArtifactStart)
        {
            playerStatusEffect.effects[(int)castEffect.effect].ApplyEffect(caster, castEffect.severity, castEffect.time);
        }
    }

    public virtual void Cast()
    {
        activated = true;
        foreach (SkillVisualizers skillVisualizers in onVisualizerStart)
        {
            skillVisualizers.Start();
        }
    }

    public virtual void Channel()
    {

    }

    public virtual void Stop()
    {
        activated = false;
        foreach (SkillVisualizers skillVisualizers in onVisualizerStart)
        {
            skillVisualizers.End();
        }
    }

    public virtual bool Completed()
    {
        return !activated;
    }

    [Serializable]
    public class SkillVisualizers
    {
        public Fireable fireable;

        public void Start()
        {
            fireable.Start();
        }

        public void End()
        {
            fireable.End();
        }

        public void Setup(Player player)
        {
            // Copy the scriptable object
            fireable = Instantiate(fireable);
            fireable.Setup(player);
        }
    }

    [Serializable]
    public class SkillArtifacts
    {
        public StatusEffects effect;
        public float time;
        public float severity;
    }

    [Header("Skill Artifacts and Visuals")]
    public SkillArtifacts[] onArtifactStart;
    public SkillVisualizers[] onVisualizerStart;

    [Header("Skill Activation")]
    public XboxButton activationButton;

    //public SkillArtifacts[] onSkillEnd;
}