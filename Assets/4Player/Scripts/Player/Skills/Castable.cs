public interface Castable
{
    void Cast(params object[] cartProperties);
    void ApplyEffectors(ref Effectors playerStatusEffects);
    bool Completed();
    void Channel();
    void Stop();
}
