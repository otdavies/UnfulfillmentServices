using System;
using System.Runtime.InteropServices;

public enum StatusEffects
{
    IMMOBILIZED = 0,
    SLOWED = 1,
    HASTENED = 2
}

[StructLayout(LayoutKind.Sequential)]
public struct Effectors
{
    public StatusEffect[] effects;

    public Effectors(StatusEffect[] effects)
    {
        this.effects = effects;
    }
}

public struct StatusEffect
{
    public bool isActive;
    public float period;
    public float severity;

    public StatusEffect(bool isActive, float period, float severity)
    {
        this.isActive = isActive;
        this.period = period;
        this.severity = severity;
    }
}

public interface Castable
{
    void Cast(params object[] cartProperties);
    Effectors Effectors();
    bool Completed();

}

public interface Channelable : Castable
{
    void Channel();
    void Stop();
}
