public interface Castable
{
    void Cast();
    void ApplyEffectors(ref Effectors playerStatusEffects);
    bool Completed();
    void Channel();
    void Stop();
}
