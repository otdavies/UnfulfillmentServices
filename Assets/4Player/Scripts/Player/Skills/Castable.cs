using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
}

public class StatusEffect
{
    public bool isActive;
    private Coroutine routine;

    protected StatusEffect()
    {
        this.isActive = false;
    }

    public void ApplyEffect(Player player, float duration, float severity)
    {
        if (duration <= 0)
        {
            InitializeEffect(player, duration, severity);
            Start();
            return;
        }

        routine = player.StartCoroutine(LifeCycle(player, duration, severity));
    }

    public void RemoveEffect(Player player)
    {
        player.StopCoroutine(routine);
        End();
    }

    protected IEnumerator LifeCycle(Player player, float duration, float severity)
    {
        // Initizlie
        WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        float percent = 0;

        // Fire off execution
        InitializeEffect(player, duration, severity);
        Start();
        while (percent < 1)
        {
            Refresh(percent);
            percent += Time.deltaTime / duration;
            yield return endOfFrame;
        }
        End();
    }

    protected virtual void InitializeEffect(Player player, float duration, float severity)
    {
    }

    protected virtual void Start()
    {
        isActive = true;
    }

    protected virtual void Refresh(float percent)
    {
    }

    protected virtual void End()
    {
        isActive = false;
    }
}

public class Immobilized : StatusEffect
{
    protected Player player;
    protected float duration;
    protected float severity;

    protected override void InitializeEffect(Player player, float duration, float severity)
    {
        this.player = player;
        this.duration = duration;
        this.severity = severity;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void End()
    {
        base.End();
    }
}

public class Motion : StatusEffect
{
    protected Player player;
    protected float duration;
    protected float severity;

    protected override void InitializeEffect(Player player, float duration, float severity)
    {
        this.player = player;
        this.duration = duration;
        this.severity = severity;
    }

    protected override void Start()
    {
        base.Start();
        player.speed *= severity;
    }

    protected override void End()
    {
        player.speed /= severity;
        base.End();
    }
}

public class Muted : StatusEffect
{

}

public interface Castable
{
    void Cast(params object[] cartProperties);
    void Effectors(Player player, ref Effectors playerStatusEffects);
    bool Completed();
    void Channel();
    void Stop();
}

public abstract class Skill : ScriptableObject, Castable
{
    private Player caster;
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
    }

    public abstract bool CanCast(XboxController controller);

    public abstract void Cast(params object[] cartProperties);
    public abstract void Effectors(Player player, ref Effectors playerStatusEffects);
    public abstract bool Completed();

    public virtual void Channel() { }
    public virtual void Stop() { }
}
