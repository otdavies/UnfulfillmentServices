using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BlastSkill", menuName = "Skills/Abilities/Blast", order = 1)]
public class ForceBlastSkill : Skill
{
    [Header("Skill Properties")]
    public float skillLifeTime = 0.5f;
    private float percent = 0;
    private Rigidbody casterRigidbody;

    public override void RegisterTo(Player player)
    {
        base.RegisterTo(player);
        casterRigidbody = player.GetComponent<Rigidbody>();
    }

    public override void Cast()
    {
        base.Cast();
        casterRigidbody.AddExplosionForce(70, caster.transform.position + caster.transform.forward * 2, 4, 0, ForceMode.Impulse);
    }

    public override void Channel()
    {
        percent += Time.fixedDeltaTime;
        if (!(percent > skillLifeTime)) return;

        base.Stop();
        percent = 0;
    }
}
