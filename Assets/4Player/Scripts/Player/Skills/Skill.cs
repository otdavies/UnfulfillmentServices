﻿using System;
using UnityEngine;
using XboxCtrlrInput;

public enum StatusEffects
{
    IMMOBILIZED = 0,
    MOTION = 1,
    MUTED = 2
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

    public abstract bool CanCast(XboxController controller);
    public abstract void Cast(params object[] cartProperties);
    public abstract void ApplyEffectors(ref Effectors playerStatusEffects);
    public abstract bool Completed();

    public virtual void Channel() { }
    public virtual void Stop() { }

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

    //public SkillArtifacts[] onSkillEnd;
}