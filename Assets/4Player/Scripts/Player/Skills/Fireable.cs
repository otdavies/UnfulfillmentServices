using UnityEngine;

public abstract class Fireable : ScriptableObject
{
    [Header("Default Properties")]
    public Vector3 firePoint;
    public Vector3 fireDirection;
    public float fireScale;

    public abstract void Setup(Player player);
    public abstract void Start();
    public abstract void End();
}
