using System.Collections;
using UnityEngine;

public class StatusEffect
{
    public bool isActive;
    private Coroutine routine;

    protected StatusEffect()
    {
        this.isActive = false;
    }

    public void ApplyEffect(Player player, float severity)
    {
        InitializeEffect(player, 0, severity);
        Start();
    }

    public void ApplyEffect(Player player, float severity, float duration)
    {
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
        player.moveSpeed *= severity;
    }

    protected override void End()
    {
        player.moveSpeed /= severity;
        base.End();
    }
}

public class Muted : StatusEffect
{

}