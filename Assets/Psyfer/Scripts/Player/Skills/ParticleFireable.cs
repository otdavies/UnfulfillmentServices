using UnityEngine;

[CreateAssetMenu(fileName = "ParticleFireable", menuName = "Skills/Visuals/ParticleFireable", order = 1)]
public class ParticleFireable : Fireable
{
    [Header("Specialized Properties")]
    public GameObject prefabToToggle;
    protected GameObject fireableObject;
    protected ParticleSystem[] particleSystems;

    public override void Setup(Player player)
    {
        fireableObject = Instantiate(prefabToToggle);
        fireableObject.SetActive(false);
        fireableObject.transform.parent = player.transform;
        fireableObject.transform.localPosition = firePoint;
        fireableObject.transform.localEulerAngles = fireDirection;
        fireableObject.transform.localScale = Vector3.one * fireScale;

        particleSystems = fireableObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
        }
        fireableObject.SetActive(true);
    }

    public override void Start()
    {
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play();
        }
    }

    public override void End()
    {
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Stop();
        }
    }
}
